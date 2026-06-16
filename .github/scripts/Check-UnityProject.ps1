# Static project checks that need no Unity license. Each guards against a
# failure mode this repo has actually hit. Run from the repo root:
#   pwsh .github/scripts/Check-UnityProject.ps1

$ErrorActionPreference = "Stop"
$root = (Get-Location).Path
$failures = New-Object System.Collections.Generic.List[string]

function Fail([string]$message) { $script:failures.Add($message) }

# --- 1. Architecture: features are pure islands -----------------------------
# Every asmdef under Assets/Features except Core/Core.Editor must reference no
# JamTemplate assembly; only Core and Core.Editor may, and nothing may
# reference Core back (no cycles into the composition root).

$asmdefs = Get-ChildItem "$root/Assets/Features" -Recurse -Filter *.asmdef
foreach ($file in $asmdefs) {
    $json = Get-Content $file.FullName -Raw | ConvertFrom-Json
    $jamRefs = @($json.references | Where-Object { $_ -like "JamTemplate*" })
    $isCore = $json.name -eq "JamTemplate.Core" -or $json.name -eq "JamTemplate.Core.Editor"

    if (-not $isCore -and $jamRefs.Count -gt 0) {
        Fail "Architecture: $($json.name) references [$($jamRefs -join ', ')] - features must reference no JamTemplate assembly; wire through an extension point and Core instead."
    }
    if ($jamRefs -contains "JamTemplate.Core" -and $json.name -ne "JamTemplate.Core.Editor") {
        Fail "Architecture: $($json.name) references JamTemplate.Core - nothing may depend on the composition root."
    }
}
Write-Output "checked $($asmdefs.Count) asmdefs"

# --- 2. Meta hygiene ---------------------------------------------------------
# Every asset under Assets/ needs a .meta, every .meta needs its asset, and no
# two .meta files may share a GUID (copy-paste duplication corrupts references).

# Unity imports plugin bundles (.bundle/.framework/.plugin/.androidlib) as a
# single opaque asset — files inside them have no .meta by design (e.g. FMOD's
# macOS *.bundle plugins). Skip package contents; the package dir itself still
# gets a .meta and is checked.
$packageContents = '\.(bundle|framework|plugin|androidlib)[\\/]'

$assetRoot = "$root/Assets"
$entries = Get-ChildItem $assetRoot -Recurse -Force | Where-Object { $_.Name -ne ".DS_Store" }
foreach ($entry in $entries) {
    if ($entry.FullName -match $packageContents) { continue }
    $isMeta = $entry.Extension -eq ".meta"
    $relative = $entry.FullName.Substring($root.Length + 1)
    if ($isMeta) {
        $target = $entry.FullName.Substring(0, $entry.FullName.Length - 5)
        if (-not (Test-Path -LiteralPath $target)) { Fail "Meta: orphaned $relative (no matching asset)" }
    }
    else {
        if (-not (Test-Path -LiteralPath "$($entry.FullName).meta")) { Fail "Meta: $relative has no .meta file" }
    }
}

$guids = @{}
foreach ($meta in (Get-ChildItem $assetRoot -Recurse -Filter *.meta)) {
    if ($meta.FullName -match $packageContents) { continue }
    $match = Select-String -LiteralPath $meta.FullName -Pattern "^guid: ([0-9a-f]{32})" | Select-Object -First 1
    if ($null -eq $match) { Fail "Meta: $($meta.FullName.Substring($root.Length + 1)) has no guid line"; continue }
    $guid = $match.Matches[0].Groups[1].Value
    if ($guids.ContainsKey($guid)) {
        Fail "Meta: duplicate guid $guid in $($meta.FullName.Substring($root.Length + 1)) and $($guids[$guid])"
    }
    else { $guids[$guid] = $meta.FullName.Substring($root.Length + 1) }
}
Write-Output "checked $($guids.Count) meta guids"

# --- 3. Build settings: every scene exists and its GUID is current -----------
# Scenes deleted and regenerated keep their path but get a new GUID; a stale
# entry silently resolves as 'missing' in build profiles.

$buildSettings = Get-Content "$root/ProjectSettings/EditorBuildSettings.asset" -Raw
$sceneMatches = [regex]::Matches($buildSettings, "path: (Assets/[^\r\n]+\.unity)\r?\n\s+guid: ([0-9a-f]{32})")
foreach ($m in $sceneMatches) {
    $scenePath = $m.Groups[1].Value
    $listedGuid = $m.Groups[2].Value
    $metaPath = "$root/$scenePath.meta"
    if (-not (Test-Path -LiteralPath $metaPath)) {
        Fail "BuildSettings: $scenePath is listed but the scene (or its .meta) does not exist"
        continue
    }
    $actual = (Select-String -LiteralPath $metaPath -Pattern "^guid: ([0-9a-f]{32})").Matches[0].Groups[1].Value
    if ($actual -ne $listedGuid) {
        Fail "BuildSettings: $scenePath listed with guid $listedGuid but the asset's guid is $actual (stale entry - re-register the scene)"
    }
}
Write-Output "checked $($sceneMatches.Count) build-settings scenes"

# --- Result -------------------------------------------------------------------
if ($failures.Count -gt 0) {
    Write-Output ""
    Write-Output "FAILED ($($failures.Count) problem(s)):"
    $failures | ForEach-Object { Write-Output "  - $_" }
    exit 1
}
Write-Output ""
Write-Output "All checks passed."
exit 0
