<#
.SYNOPSIS
  Register the self-hosted runners declared in runners.yaml and apply each one's
  autostart setting. Idempotent — re-run after editing the YAML.

.DESCRIPTION
  For each runner (all, or just -Name) it downloads the runner package (cached),
  registers it with the shared labels at a SHORT path, then creates or removes
  its at-logon scheduled task to match `autostart`. Runners are NOT Windows
  services: Unity 6 ties the Personal license to the signed-in user and a
  Microsoft-account login can't be a service logon, so they run in your session.

  Needs an elevated PowerShell and the gh CLI authenticated with admin on the
  targets. Stop a runner before re-registering it (Manage-Runners.ps1 stop -Name).

.PARAMETER Name
  Only act on this runner (its name or folder). Omit to do every runner.

.PARAMETER Force
  Re-register even if the runner is already configured. Without it, an
  already-registered runner is left as-is and only its autostart is reapplied
  (so flipping `autostart` in the YAML doesn't disrupt a running runner).
#>
param(
    [string]$Name,
    [switch]$Force,
    [string]$ConfigPath = (Join-Path $PSScriptRoot 'runners.yaml')
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\RunnerLib.ps1"
Test-RunnerAdmin
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI (gh) not found. Install it and run 'gh auth login' first."
}

$config = Read-RunnersConfig -Path $ConfigPath
$labels = ($config.labels -join ',')
New-Item -ItemType Directory -Force -Path $config.installRoot | Out-Null

# Deep package paths break Unity even with the OS flag, so keep git happy too;
# the real fix is the short installRoot. Set both, harmless if already set.
git config --system core.longpaths true 2>$null
New-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem' `
    -Name LongPathsEnabled -Value 1 -PropertyType DWORD -Force | Out-Null

# Cache the latest runner package once.
$runnerTag = (gh api repos/actions/runner/releases/latest --jq '.tag_name')
$zipName = "actions-runner-win-x64-$($runnerTag.TrimStart('v')).zip"
$zipPath = Join-Path $config.installRoot $zipName
if (-not (Test-Path $zipPath)) {
    Write-Host "Downloading runner $($runnerTag.TrimStart('v'))..."
    Invoke-WebRequest "https://github.com/actions/runner/releases/download/$runnerTag/$zipName" -OutFile $zipPath
}

foreach ($runner in (Select-Runners -Config $config -Name $Name)) {
    Write-Host "`n=== $($runner.name) ($($runner.type): $($runner.target)) ==="
    $dir = Get-RunnerDir -Config $config -Runner $runner
    $configured = Test-Path (Join-Path $dir '.runner')

    if ($configured -and -not $Force) {
        # Already registered: leave it running, just reconcile autostart.
        Write-Host "$($runner.name): already registered; applying autostart only (use -Force to re-register)."
    }
    else {
        $apiPath = if ($runner.type -eq 'org') { "orgs/$($runner.target)/actions/runners/registration-token" }
        else { "repos/$($runner.target)/actions/runners/registration-token" }
        $token = gh api -X POST $apiPath --jq '.token'
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($token)) {
            Write-Warning "Skipping $($runner.name): no registration token (need admin on $($runner.target); for an org: gh auth refresh -h github.com -s admin:org)."
            continue
        }

        if ($configured) {
            Write-Host "Re-registering."
            Stop-RunnerProcess -Runner $runner -Dir $dir
            Push-Location $dir; & .\config.cmd remove --token $token 2>$null; Pop-Location
        }
        else {
            New-Item -ItemType Directory -Force -Path $dir | Out-Null
            Expand-Archive -Path $zipPath -DestinationPath $dir -Force
        }

        Push-Location $dir
        & .\config.cmd --unattended --replace --url "https://github.com/$($runner.target)" `
            --token $token --name $runner.name --labels $labels --work '_work'
        $ok = ($LASTEXITCODE -eq 0)
        Pop-Location
        if (-not $ok) { Write-Warning "$($runner.name): configuration failed - not registered."; continue }
        Write-Host "$($runner.name): registered with labels [$labels]."
    }

    $autostart = [bool]$runner.autostart
    Set-RunnerAutostart -Runner $runner -Dir $dir -Enabled $autostart
    if ($autostart) { Start-RunnerProcess -Runner $runner -Dir $dir }
}

Write-Host "`nDone. Manage with .\Manage-Runners.ps1 status"
