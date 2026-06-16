# Settings

Audio, display, and graphics settings with an **apply → confirm-with-timer → keep/revert** flow (so a broken resolution can roll back). The `SettingsManager` loads saved preferences over the engine's current state at boot and applies them; the settings screen edits a working copy that only commits on Apply + Keep.

## Setup

1. Nothing to place — the Settings Manager is spawned automatically at startup by the [Core](Core.md) feature (persistent singleton, initializes before the screen/binders). To also drop a copy in a scene, run **`Tools ▸ Sailor Snouts ▸ Managers ▸ Create Settings Manager`**.
2. Generate the screen with **`Tools ▸ Sailor Snouts ▸ Scenes ▸ Create Settings Scene`** (registered in Build Settings). Open it from any menu via a `Menu Button Action` set to `OpenAdditive` → `Settings`.

On boot the manager reads saved PlayerPrefs over a live engine snapshot and applies them. The binders fall back to live engine values when no manager is present (so the scene previews correctly standalone).

## Edit lifecycle

`BeginEdit()` copies the confirmed settings into a `Working` copy the UI mutates → `ApplyWorking()` pushes Working to the engine (audio/display/graphics) without saving → `Confirm()` saves what was applied to PlayerPrefs → `Revert()` restores the pre-apply state. The confirm dialog auto-reverts after a timer if not kept.

**Volume sliders preview live** (audio can't break the display, so it skips the confirm flow); closing the screen without applying restores the confirmed volumes. `ResetWorking()` (the screen's Reset button) returns Working to default volumes/UI scale/graphics while keeping resolution, window mode and quality.

## Components

- **Settings Manager** — `Sailor Snouts/Settings Manager` — owns `Confirmed`/`Working` state, applies to the engine, persists.
- **Settings Screen** — `Sailor Snouts/Settings Screen` — on a screen, calls `BeginEdit()` so binders read fresh values.
- **Audio Volume Slider** — `Sailor Snouts/Audio Volume Slider` — binds a Slider to a volume `channel` (Master/Sfx/Music/Ambiance/Dialogue/Ui).
- **Settings Dropdown** — `Sailor Snouts/Settings Dropdown` — binds a TMP dropdown to a `setting`: `Resolution`, `WindowMode`, `Quality`, `FrameRate`, `AntiAliasing`, `UiScale`.
- **VSync Toggle** — `Sailor Snouts/VSync Toggle`.
- **Settings Apply Button** / **Settings Confirm Dialog** — `Sailor Snouts/Settings Apply Button` / `Sailor Snouts/Settings Confirm Dialog` — drive ApplyWorking + the keep/revert countdown (`revertAfter` seconds).
- **Settings Reset Button** — `Sailor Snouts/Settings Reset Button` — resets Working to defaults (see above).

## API

```csharp
public static SettingsManager Instance { get; }
public SettingsState Confirmed { get; }
public SettingsState Working { get; }
public event Action WorkingChanged;          // re-sync UI (e.g. after revert)

public void BeginEdit();
public void ApplyWorking();
public void Confirm();
public void Revert();
public void ResetWorking();                   // defaults, keeping resolution/quality
public void ApplyAudio(SettingsState s);      // volumes only (live preview)

public static SettingsState CaptureEngine();  // live engine + AudioManager snapshot
```

`SettingsState` fields: `masterVolume`, `sfxVolume`, `musicVolume`, `ambianceVolume`, `dialogueVolume`, `uiVolume`, `resolutionWidth/Height`, `fullScreenMode`, `qualityLevel`, `vSync`, `targetFrameRate`, `antiAliasing`, `uiScale`.

## Platform notes

- **WebGL:** the browser owns the canvas size, window mode, vsync and frame pacing, so the Resolution, Window Mode, VSync and Frame Rate rows hide themselves at runtime and the manager never calls `Screen.SetResolution` there. Audio, quality, anti-aliasing and UI scale all still work.
- **Anti-aliasing** is applied through the active URP asset's `msaaSampleCount` (`QualitySettings.antiAliasing` is ignored under URP).

## UI scale

The `uiScale` setting flows to every canvas through the `UIScale` seam (see [Menus](Menus.md)): `SettingsManager` calls `UIScale.Set(...)` on apply, and each canvas's `UIScaler` rescales live.

## Dependencies

None — engine packages only (TextMeshPro, URP). Owns the `SettingsHooks` extension points (volumes, UI scale); [Core](Core.md) wires them to the AudioManager and the UI's `UIScale`.
