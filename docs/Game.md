# Game

An app-level controller — the **Game Manager** — that tracks high-level state (Playing vs Paused), quits the app, and drives the pause overlay by freezing time and loading the pause scene additively behind a transition.

## Setup

1. Nothing — the Game Manager is spawned automatically at startup by the [Core](Core.md) feature (from `Assets/Features/Core/Prefabs/Resources/Game Manager.prefab`). It's a persistent singleton (`Instance`, `DontDestroyOnLoad`). To place one in a scene manually anyway, run **`Tools ▸ Sailor Snouts ▸ Managers ▸ Create Game Manager`**.
2. Generate the pause overlay with **`Tools ▸ Sailor Snouts ▸ Scenes ▸ Create Pause Scene`** (see [Pause](Pause.md)), then assign it to the Game Manager's **Pause Scene** field (a Build Settings dropdown).

## Component

**Game Manager** — `Sailor Snouts/Game Manager` (`[DisallowMultipleComponent]`)
- **Pause Scene** — the scene loaded additively as the pause overlay (picked from Build Settings).

## API

```csharp
public enum GameState { Playing, Paused }

public static GameManager Instance { get; }
public GameState State { get; }                 // default Playing
public bool IsPaused { get; }
public event Action<GameState> StateChanged;

public void Quit();                             // Application.Quit() (stops Play mode in editor)
public void Pause(float pausedTimeScale = 0f);  // 0 = freeze, e.g. 0.1f = slow-mo (clamped >= 0)
public void Resume();                           // unloads the overlay, restores prior timeScale
public void TogglePause(float pausedTimeScale = 0f);
```

`Pause` captures the live `Time.timeScale` before freezing and restores it on `Resume`. Re-entrant/redundant calls are ignored, as are calls while a scene transition is running (the pause flow and the transition manager share the overlay surface). If the pause scene is unassigned or missing from Build Settings, `Pause` warns and does nothing — it never freezes time without an overlay to resume from. A Single scene load while paused resets the state to Playing, so the game can't arrive in a new scene stuck paused.

On `Quit`: it stops Play mode in the editor and quits standalone builds, but does nothing on WebGL (the Quit buttons hide themselves there — see [Menus](Menus.md)).

## Usage

```csharp
using JamTemplate.Game;
using UnityEngine.InputSystem;  // this project is Input System-only

if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
    GameManager.Instance.TogglePause();      // or Pause(0.1f) for slow-motion

GameManager.Instance.StateChanged += s => Debug.Log($"State -> {s}");
```

Note: pausing freezes scaled time, but it does **not** block input — code that reads input in `Update` (or uses unscaled time) keeps reacting while paused or mid-transition. Subscribe to `StateChanged` (or check `IsPaused` / `SceneTransitionManager.Instance.IsTransitioning`) to gate gameplay input.

The generated Pause overlay's buttons call `Resume()` / `Quit()` for you.

## Dependencies

None — engine packages only (Input System, Odin Inspector). Owns the `PauseOverlayTransition` extension point ([Core](Core.md) wires the transition manager into it; the pause overlay snaps with no fade when unwired). [Core](Core.md) also wires the UI's `GameActions` to this manager.
