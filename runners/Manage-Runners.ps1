<#
.SYNOPSIS
  Status and start/stop/restart for the self-hosted runners in runners.yaml,
  individually or all at once.

.PARAMETER Action
  status (default) | start | stop | restart

.PARAMETER Name
  Target one runner (its name or folder). Omit (or 'all') to act on every runner.

.EXAMPLE
  .\Manage-Runners.ps1                          # status table of all runners
  .\Manage-Runners.ps1 start   -Name ust        # start just the template runner
  .\Manage-Runners.ps1 stop    -Name sailor-snouts
  .\Manage-Runners.ps1 restart                  # restart all
#>
param(
    [Parameter(Position = 0)][ValidateSet('status', 'start', 'stop', 'restart')]
    [string]$Action = 'status',
    [string]$Name
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\RunnerLib.ps1"

$config = Read-RunnersConfig
$targets = Select-Runners -Config $config -Name $Name

switch ($Action) {
    'status' {
        $targets | ForEach-Object {
            $dir = Get-RunnerDir -Config $config -Runner $_
            $proc = Get-RunnerProcess -Dir $dir
            $task = Get-ScheduledTask -TaskName (Get-RunnerTaskName $_) -ErrorAction SilentlyContinue
            [PSCustomObject]@{
                Runner    = $_.name
                Scope     = "$($_.type):$($_.target)"
                Folder    = $_.folder
                Running   = if ($proc) { "yes (PID $($proc.ProcessId))" } else { 'no' }
                Autostart = if ($task) { 'on' } else { 'off' }
            }
        } | Format-Table -AutoSize
    }
    'start' { $targets | ForEach-Object { Start-RunnerProcess -Runner $_ -Dir (Get-RunnerDir -Config $config -Runner $_) } }
    'stop' { $targets | ForEach-Object { Stop-RunnerProcess  -Runner $_ -Dir (Get-RunnerDir -Config $config -Runner $_) } }
    'restart' {
        $targets | ForEach-Object {
            $dir = Get-RunnerDir -Config $config -Runner $_
            Stop-RunnerProcess -Runner $_ -Dir $dir
            Start-Sleep -Seconds 2
            Start-RunnerProcess -Runner $_ -Dir $dir
        }
    }
}
