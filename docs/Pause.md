# Pause

A generated **pause overlay scene**: a camera-less additive UI with a dimming backdrop, a "Paused" heading, and Resume / Settings / Quit buttons. At runtime it's loaded additively by the [Game Manager](Game.md) when the game pauses; its buttons call back into the Game Manager and open Settings.

## Setup

1. Run **`Tools ▸ Sailor Snouts ▸ Scenes ▸ Create Pause Scene`** → generates `Assets/Features/Core/Scenes/Pause.unity` (and registers it in Build Settings).
2. Assign that scene to the **Game Manager ▸ Pause Scene** field.
3. Trigger pausing from your code via `GameManager.Instance.Pause()` / `TogglePause()` — it loads this overlay; the Resume button unloads it.

It is **not** opened directly — the Game Manager loads/unloads it behind a transition fade.

## What the scene contains

The Pause feature has **no runtime components of its own** (`PauseSceneSetup` is an editor-only generator). The generated scene uses shared [Menus](Menus.md) pieces:
- A `Pause Canvas` (sort order 1000) with `EnsureEventSystem`.
- A dimming backdrop (blocks clicks to the game underneath).
- A bold "Paused" heading.
- A button column: **Resume** (`MenuAction.Resume`), **Settings** (`OpenAdditive` → `"Settings"`), **Quit** (`MenuAction.Quit`). Resume gets `InitialSelection` (default focus).

## Usage

```csharp
using JamTemplate.Game;

GameManager.Instance.Pause();   // loads the assigned pause overlay; Resume button calls Resume()
```

## Dependencies

The Pause scene is pure composition: its generator lives in [Core](Core.md) and builds the screen from UI components; the Resume/Quit buttons reach the GameManager through the UI's `GameActions` extension point.
