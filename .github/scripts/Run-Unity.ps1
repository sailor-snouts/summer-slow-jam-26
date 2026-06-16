<#
.SYNOPSIS
  Runs the locally-installed Unity Editor in batch mode for CI jobs on a
  self-hosted Windows runner. Locates the editor matching the project's
  ProjectVersion.txt under the standard Unity Hub install root.

.PARAMETER Mode
  tests        - run EditMode tests (-runTests), fail on test failure
  webgl        - build WebGL via BatchBuild.BuildWebGL
  sln          - generate the .sln/.csproj via CsprojGenerator.Generate (for linting)
  enable-fmod  - turn on the FMOD backend (edits asmdefs); needs the FMOD package
                 installed on the runner. Run this in its own Unity invocation
                 before a webgl/tests run so the FMOD code is compiled in.
  disable-fmod - revert the FMOD backend edits (use after an FMOD run so the
                 persistent self-hosted checkout doesn't drift).
#>
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('tests', 'webgl', 'sln', 'enable-fmod', 'disable-fmod')]
    [string]$Mode
)

$ErrorActionPreference = 'Stop'
$workspace = if ($env:GITHUB_WORKSPACE) { $env:GITHUB_WORKSPACE } else { (Get-Location).Path }

# Resolve the editor that matches the project, so the runner machine can host
# several Unity versions without this hardcoding one.
$version = ((Get-Content "$workspace/ProjectSettings/ProjectVersion.txt" |
        Select-String '^m_EditorVersion: (.+)$').Matches.Groups[1].Value).Trim()
$hubRoot = if ($env:UNITY_HUB_EDITOR_ROOT) { $env:UNITY_HUB_EDITOR_ROOT } else { 'C:\Program Files\Unity\Hub\Editor' }
$unity = Join-Path $hubRoot "$version\Editor\Unity.exe"
if (-not (Test-Path $unity)) {
    throw "Unity $version not found at '$unity'. Install it via Unity Hub on the runner, or set UNITY_HUB_EDITOR_ROOT."
}
Write-Host "Using Unity $version at $unity"

# A stale lock from a crashed prior run blocks batch mode; the runner workspace
# is isolated from your dev clone, so clearing it here is safe.
Remove-Item "$workspace/Temp/UnityLockfile" -ErrorAction SilentlyContinue

$log = Join-Path $workspace "unity-$Mode.log"
Remove-Item $log -ErrorAction SilentlyContinue

$common = @(
    '-batchmode', '-nographics', '-quit',
    '-projectPath', $workspace,
    '-logFile', $log
)

switch ($Mode) {
    'tests' {
        $results = Join-Path $workspace 'test-results.xml'
        # -runTests implies its own -quit/exit; don't pass -quit (Unity warns).
        $args = @(
            '-batchmode', '-nographics',
            '-projectPath', $workspace,
            '-logFile', $log,
            '-runTests', '-testPlatform', 'EditMode',
            '-testResults', $results
        )
    }
    'webgl' { $args = $common + @('-executeMethod', 'JamTemplate.Core.Editor.BatchBuild.BuildWebGL') }
    'sln' { $args = $common + @('-executeMethod', 'JamTemplate.Core.Editor.CsprojGenerator.Generate') }
    'enable-fmod' { $args = $common + @('-executeMethod', 'JamTemplate.Core.Editor.FmodPackageToggle.EnableForCi') }
    'disable-fmod' { $args = $common + @('-executeMethod', 'JamTemplate.Core.Editor.FmodPackageToggle.DisableForCi') }
}

$proc = Start-Process -FilePath $unity -ArgumentList $args -PassThru -Wait -NoNewWindow
if (Test-Path $log) {
    if ($proc.ExitCode -ne 0) {
        # The tail often misses build/compile errors logged earlier, so scan the
        # whole log for them. The full log is uploaded as a CI artifact too.
        Write-Host "----- Unity log: error lines -----"
        $errs = Select-String -Path $log -Pattern 'error CS|\): error|Error building|Shader error|Compilation failed|Exception:|cannot be|Could not|Failed to|Assertion' |
            Select-Object -ExpandProperty Line -First 80
        if ($errs) { $errs | ForEach-Object { Write-Host $_ } }
        else { Write-Host "(no matching error lines; see the tail and the uploaded log artifact)" }
    }
    Write-Host "----- Unity log (tail) -----"
    Get-Content $log -Tail 60
}

if ($proc.ExitCode -ne 0) {
    throw "Unity ($Mode) exited with code $($proc.ExitCode)."
}
Write-Host "Unity ($Mode) succeeded."
