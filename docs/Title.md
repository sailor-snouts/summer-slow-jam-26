# Title

Generates the title screen as real, editable Unity UI — a dark background, a "Game Title" heading, and a column of menu buttons. It's an editor authoring tool; the buttons' runtime behavior comes from the shared [Menus](Menus.md) components.

## Setup

1. Run **`Tools ▸ Sailor Snouts ▸ Scenes ▸ Create Title Scene`** → generates/opens `Assets/Features/Core/Scenes/Title.unity`.
2. Set the **New Game** button's **Scene** (on its `Menu Button Action`) to your first gameplay scene — the generator leaves it empty.
3. To regenerate after changing the generator, delete `Title.unity` (+ `.meta`) and re-run the command.

## Generated buttons

| Button | Action | Scene | Visible When |
|---|---|---|---|
| **New Game** | `LoadScene` | *(set this)* | Always (gets `InitialSelection`) |
| **Continue** | `Continue` | — | `WhenSaveExists` |
| **Load Game** | `OpenAdditive` | `Saves` | `WhenSaveExists` |
| **Settings** | `OpenAdditive` | `Settings` | Always |
| **Credits** | `LoadScene` | `Credits` | Always |
| **Quit** | `Quit` | — | Always |

**Continue** resumes the most recent save (loads its recorded scene and applies it — see [Saving](Saving.md)). **Continue** and **Load Game** hide themselves when no save exists, via the `MenuSaveState` seam filled by the Save Manager — so they only appear once the player has a save.

There are no Title-specific components; the buttons use `MenuButtonAction` / `InitialSelection` from the UI feature.

## Dependencies

The Title scene is pure composition: its generator lives in [Core](Core.md) and builds the screen from UI components wired to the Saving/Game extension points.
