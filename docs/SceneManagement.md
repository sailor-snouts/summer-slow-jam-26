# Scene Management

A drop-in transition system. A persistent manager runs a **cover → load → reveal** sequence when changing scenes, with an optional loading-progress display and audio fade, and can also open/close additive overlay scenes (e.g. settings) on top of the current scene. The covering visual (fade / slide / shrink) is swappable.

## Setup

The Scene Transition Manager is spawned automatically at startup by the [Core](Core.md) feature (from `Assets/Features/Core/Prefabs/Resources/SceneTransitionManager.prefab`). `[RequireComponent]` adds `SceneTransition` and `SceneLoader` automatically; it persists across scene loads and builds its overlay canvas at runtime. Target scenes must be in **Build Settings**. To also drop a copy in a scene, run **`Tools ▸ Sailor Snouts ▸ Managers ▸ Create Scene Transition Manager`**.

## Components

**Scene Transition Manager** — `Sailor Snouts/Scene Transition Manager` (the one you normally touch)
- **Show Progress** / **Show Text** / **Show Bar** / **Progress Color** — loading display options.
- **Fade Audio** — fade all audio out on cover, in on reveal (uses `AudioManager` if present).

**Scene Transition** — `Sailor Snouts/Scene Transition` — the visual half. Builds a full-screen overlay and animates a covering panel.
- **Color** — overlay tint.
- **Effect** — the transition animation, a polymorphic `[SerializeReference]` field; pick `Fade`, `Slide`, or `Shrink` in the inspector and its options appear inline.

**Scene Loader** — `Sailor Snouts/Scene Loader` — async single-mode loading with progress.

### Transition effects

Chosen on `Scene Transition ▸ Effect` (not separate components). Each effect shares:
- **Cover Duration** / **Reveal Duration** (seconds, independent halves)
- **Curve** — easing; the **Apply Preset Curve** button drops in a preset (Linear, EaseIn, EaseOut, EaseInOut, Smooth)
- `FadeTransitionEffect` (default), `SlideTransitionEffect` (+ `direction`), `ShrinkTransitionEffect`.

Add your own by writing a `[Serializable]` subclass of `SceneTransitionEffect` overriding `ResetState`/`Apply`. Animations run on unscaled time, so they work while paused.

## API

```csharp
public static SceneTransitionManager Instance { get; }
public bool IsTransitioning { get; }
public SceneTransition Transition { get; }

public void Load(string sceneName);          // cover, load (Single), reveal
public void OpenAdditive(string sceneName);  // overlay a scene behind a fade
public void CloseAdditive(string sceneName); // unload an overlay behind a fade

public bool TryBeginExternalTransition();    // reserve the overlay for your own cover/reveal
public void EndExternalTransition();         // release it (the pause flow uses this pair)
```

`Load`/`OpenAdditive`/`CloseAdditive` no-op (with a warning) while `IsTransitioning`. `OpenAdditive` skips if already loaded; `CloseAdditive` skips if not loaded. `Load` resets `Time.timeScale = 1` at the start.

While an overlay is open, `OpenAdditive` disables interaction on every previously loaded scene's root canvases (via a temporary `CanvasGroup`), so the pointer and gamepad navigation can't reach the menu underneath; `CloseAdditive` restores them and re-selects whatever was focused before the overlay opened, keeping keyboard/controller navigation alive.

`SceneLoader` exposes `Progress`, `IsLoading`, `event Action<float> ProgressChanged`, `event Action Loaded`, and `IEnumerator LoadRoutine(string)`.

## Usage

```csharp
using JamTemplate.SceneManagement;

SceneTransitionManager.Instance.Load("Level1");          // change scene with a fade
SceneTransitionManager.Instance.OpenAdditive("Settings"); // overlay settings
SceneTransitionManager.Instance.CloseAdditive("Settings");
```

Used by the menu buttons (`MenuButtonAction`), the Game pause flow, the splash sequence, and Continue.

## Dependencies

None — engine packages only (Odin Inspector). Owns the `TransitionAudio` extension point ([Core](Core.md) wires the AudioManager into it; no fade when unwired). Editor-only `SceneCatalog.GetSceneNames()` backs inspector scene dropdowns.
