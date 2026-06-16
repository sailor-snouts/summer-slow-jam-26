<#
  Shared helpers for the runner scripts. Dot-source it:
      . "$PSScriptRoot\RunnerLib.ps1"

  Includes a tiny YAML reader for runners.yaml (NOT a general parser — it only
  understands the documented flat shape: top-level scalars, a [a, b] labels
  array, and a 'runners:' list of maps), plus helpers to resolve, launch, stop,
  and auto-start each runner. Runners run in a titled, visible console so you
  can tell them apart and close individual ones.
#>

function ConvertFrom-YamlScalar {
    param([string]$Value)
    $v = $Value.Trim()
    if ($v.Length -ge 2 -and $v[0] -eq '"' -and $v[-1] -eq '"') { return $v.Substring(1, $v.Length - 2) }
    if ($v.Length -ge 2 -and $v[0] -eq "'" -and $v[-1] -eq "'") { return $v.Substring(1, $v.Length - 2) }
    if ($v -match '^(true|yes)$') { return $true }
    if ($v -match '^(false|no)$') { return $false }
    return $v
}

function ConvertFrom-YamlArray {
    param([string]$Value)
    $v = $Value.Trim().Trim('[', ']').Trim()
    if ($v.Length -eq 0) { return @() }
    return @($v.Split(',') | ForEach-Object { ConvertFrom-YamlScalar $_ })
}

function Read-RunnersConfig {
    param([string]$Path = (Join-Path $PSScriptRoot 'runners.yaml'))
    if (-not (Test-Path -LiteralPath $Path)) { throw "Config not found: $Path" }

    $installRoot = $null
    $labels = @()
    $runners = @()
    $cur = $null
    $inRunners = $false

    foreach ($raw in Get-Content -LiteralPath $Path) {
        $line = ($raw -replace '#.*$', '').TrimEnd()
        if ($line.Trim().Length -eq 0) { continue }

        # New list item: "  - key: value"
        if ($line -match '^\s*-\s*([A-Za-z_]\w*):\s*(.*)$') {
            if ($cur) { $runners += [pscustomobject]$cur }
            $cur = [ordered]@{}
            $cur[$matches[1]] = ConvertFrom-YamlScalar $matches[2]
            continue
        }
        # Indented field of the current item: "    key: value"
        if ($inRunners -and $cur -and $line -match '^\s{2,}([A-Za-z_]\w*):\s*(.*)$') {
            $cur[$matches[1]] = ConvertFrom-YamlScalar $matches[2]
            continue
        }
        # Top-level "key:" or "key: value"
        if ($line -match '^([A-Za-z_]\w*):\s*(.*)$') {
            if ($cur) { $runners += [pscustomobject]$cur; $cur = $null }
            switch ($matches[1]) {
                'runners' { $inRunners = $true }
                'labels' { $labels = ConvertFrom-YamlArray $matches[2]; $inRunners = $false }
                'installRoot' { $installRoot = ConvertFrom-YamlScalar $matches[2]; $inRunners = $false }
                default { $inRunners = $false }
            }
            continue
        }
    }
    if ($cur) { $runners += [pscustomobject]$cur }

    [pscustomobject]@{ installRoot = $installRoot; labels = $labels; runners = $runners }
}

function Get-RunnerDir {
    param($Config, $Runner)
    Join-Path $Config.installRoot $Runner.folder
}

function Select-Runners {
    # Filter the config's runners by an optional -Name (matches name or folder).
    param($Config, [string]$Name)
    if ([string]::IsNullOrWhiteSpace($Name) -or $Name -eq 'all') { return $Config.runners }
    $hit = $Config.runners | Where-Object { $_.name -eq $Name -or $_.folder -eq $Name }
    if (-not $hit) { throw "No runner named '$Name' in runners.yaml (have: $(($Config.runners.name) -join ', '))." }
    return $hit
}

function Get-RunnerTaskName { param($Runner) "GitHubRunner-$($Runner.name)" }

function Get-RunnerProcess {
    # The Runner.Listener process for this runner (by its install dir), or $null.
    param([string]$Dir)
    Get-CimInstance Win32_Process -Filter "Name = 'Runner.Listener.exe'" -ErrorAction SilentlyContinue |
        Where-Object { $_.ExecutablePath -and $_.ExecutablePath.StartsWith($Dir, [System.StringComparison]::OrdinalIgnoreCase) } |
        Select-Object -First 1
}

function Start-RunnerProcess {
    # Launch the runner in its own titled, visible console window.
    param($Runner, [string]$Dir)
    if (Get-RunnerProcess -Dir $Dir) { Write-Host "$($Runner.name): already running."; return }
    $runCmd = Join-Path $Dir 'run.cmd'
    if (-not (Test-Path -LiteralPath $runCmd)) { Write-Warning "$($Runner.name): not installed ($runCmd missing)."; return }
    # title sets the console caption so the window is identifiable.
    Start-Process -FilePath 'cmd.exe' -WorkingDirectory $Dir `
        -ArgumentList "/c", "title Runner $($Runner.name) & `"$runCmd`""
    Write-Host "$($Runner.name): started (window titled 'Runner $($Runner.name)')."
}

function Stop-RunnerProcess {
    # Stop a runner: kill its console tree (run.cmd relaunches the listener, so
    # killing the listener alone isn't enough) and any leftover listener.
    param($Runner, [string]$Dir)
    $stopped = $false
    Get-CimInstance Win32_Process -Filter "Name = 'cmd.exe'" -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -and $_.CommandLine -match [regex]::Escape($Dir) } |
        ForEach-Object { & taskkill /PID $_.ProcessId /T /F *> $null; $stopped = $true }
    $proc = Get-RunnerProcess -Dir $Dir
    if ($proc) { & taskkill /PID $proc.ProcessId /T /F *> $null; $stopped = $true }
    if ($stopped) { Write-Host "$($Runner.name): stopped." } else { Write-Host "$($Runner.name): was not running." }
}

function Set-RunnerAutostart {
    # Create/refresh (or remove) the at-logon scheduled task that launches the
    # runner in a titled, visible console as the current user.
    param($Runner, [string]$Dir, [bool]$Enabled)
    $taskName = Get-RunnerTaskName $Runner
    $existing = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

    if (-not $Enabled) {
        if ($existing) { Unregister-ScheduledTask -TaskName $taskName -Confirm:$false; Write-Host "$($Runner.name): autostart OFF (task removed)." }
        else { Write-Host "$($Runner.name): autostart already off." }
        return
    }

    $runCmd = Join-Path $Dir 'run.cmd'
    $user = "$env:USERDOMAIN\$env:USERNAME"
    $action = New-ScheduledTaskAction -Execute 'cmd.exe' `
        -Argument "/c title Runner $($Runner.name) & `"$runCmd`"" -WorkingDirectory $Dir
    $trigger = New-ScheduledTaskTrigger -AtLogOn -User $user
    $principal = New-ScheduledTaskPrincipal -UserId $user -LogonType Interactive -RunLevel Limited
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries `
        -ExecutionTimeLimit ([TimeSpan]::Zero) -MultipleInstances IgnoreNew
    Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger `
        -Principal $principal -Settings $settings -Force | Out-Null
    Write-Host "$($Runner.name): autostart ON (at-logon task '$taskName')."
}

function Test-RunnerAdmin {
    if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
            ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Run this from an elevated PowerShell (Administrator)."
    }
}
