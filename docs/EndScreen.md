# End Screen

Generates two simple end-of-game scenes — a **Win** scene and a **Lose** scene — as real, editable Unity UI: a dark background, a large result message, and **Play Again** / **Quit** buttons. No runtime components or assets; build them once and edit like any scene.

## Setup

- **`Tools ▸ Sailor Snouts ▸ Scenes ▸ Create Win Scene`** → `Assets/Features/Core/Scenes/Win.unity` ("You Win!").
- **`Tools ▸ Sailor Snouts ▸ Scenes ▸ Create Lose Scene`** → `Assets/Features/Core/Scenes/Lose.unity` ("Game Over").

Both register in Build Settings. Load them from your game with `SceneTransitionManager.Instance.Load("Win")` / `"Lose"`.

> The **Play Again** button is generated with an **empty scene name** — set it to the scene you want to restart into. **Quit** needs no configuration.

## What the scenes contain

No End Screen components (the generator is editor-only). Each scene has a canvas + EventSystem + dark background, a bold **Message** text, and a button column: **Play Again** (`LoadScene`, gets `InitialSelection`) and **Quit** (`MenuAction.Quit`). Restyle freely.

## Dependencies

The Win/Lose scenes are pure composition: their generator lives in [Core](Core.md) and builds the screens from UI components.
