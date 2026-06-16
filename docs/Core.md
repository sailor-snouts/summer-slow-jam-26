# Core

The **composition root** — the one feature that knows every other feature and ties the game together. Everything composition-shaped lives here: manager spawning, the wiring between features, every scene generator and editor tool, all prefabs, and shared content. Features themselves reference **nothing** (engine packages only); each defines small static extension points, and Core fills them at startup.

```
features (pure islands):   Audio  Game  SceneManagement  Menus  Saving  Settings  SplashScreens  Credits
                              ▲      ▲        ▲           ▲     ▲        ▲           ▲           ▲
                              └──────┴────────┴───────────┴─────┴────────┴───────────┴───────────┘
                                                        Core
```

## Manager bootstrap

`ManagerBootstrapper` runs before the first scene loads and spawns every global manager from a prefab in `Assets/Features/Core/Prefabs/Resources/` (Audio, Game, Scene Transition, Save, Settings, High Score). **The prefab is the source of truth for a manager's inspector values** — scene-placed copies self-destruct as duplicates. To add a manager: make a prefab, add one `EnsureFromPrefab` line.

## Feature wiring

`FeatureWiring` connects every feature's extension points in one file, with lazy lambdas that resolve manager instances at call time — wiring order never matters, and a missing manager degrades to each hook's documented fallback (raw scene loads, no fades, silent no-op sounds). The full map:

| Extension point (owner) | Wired to | Purpose |
|---|---|---|
| `GameActions` (Menus) | GameManager | Menu Quit/Resume/TogglePause |
| `UiSounds` (Menus) | AudioManager | Button click/select sounds |
| `MenuSaveState` (Menus) | SaveManager | Save-gated buttons, Continue |
| `MenuSceneRouter` (Menus) | SceneTransitionManager | Menu scene loads behind transitions |
| `UIScale` (Menus) | ← SettingsHooks | Global UI scale |
| `TransitionAudio` (SceneManagement) | AudioManager | Audio fade during transitions |
| `PauseOverlayTransition` (Game) | SceneTransitionManager | Pause flow shares the overlay |
| `SaveSceneRouter` (Saving) | SceneTransitionManager | Continue/Load scene restore |
| `SplashTransition` (SplashScreens) | SceneTransitionManager | Splash fades + final load |
| `SettingsHooks` (Settings) | AudioManager + `UIScale` | Volumes, UI scale apply/capture |

To rewire a behavior (or stub one out), edit `FeatureWiring` — it's the only place cross-feature behavior is decided.

## Editor tools (Core/Editor)

Every `Tools ▸ Sailor Snouts` menu item lives here: the scene generators (Title, Pause, Settings, Saves, High Scores, Credits, Win, Lose), **Regenerate All Scenes** (deletes + recreates every generated scene, with a confirmation prompt), the Create-Manager tools, Delete Save Data, and Remove Redundant Managers. They build screens out of the features' runtime components plus the Menus feature's `MenuSceneBuilder`.

The generated scenes themselves live in `Core/Scenes/` (registered in Build Settings) — they're composition artifacts, the same as the generators that build them. `Assets/Scenes/` is left for the project's own scenes (Splash and gameplay), which the tools never touch.

Each tool still routes through `ToolRegistry.Run(id, default)` — game code can swap any implementation with `ToolRegistry.Override(id, impl)` from an `[InitializeOnLoad]` class; the id is the menu path under `Tools/Sailor Snouts/` (e.g. `"Scenes/Create Title Scene"`).

**Optional, package-gated:** `Tools ▸ Sailor Snouts ▸ Create 2D Gameplay Camera` builds a Cinemachine rig (a `CinemachineBrain` on the Main Camera + a `CinemachineCamera` following a placeholder) in the open scene — a starting point for a gameplay camera. It only compiles when the Cinemachine package is present (`CINEMACHINE_3` version-define on the editor asmdef); without Cinemachine the menu item simply isn't there, and nothing else depends on it. The menu cameras stay plain — Cinemachine is for gameplay only.

## Prefabs & content

All prefabs live in `Core/Prefabs/` (the six manager prefabs sit in `Prefabs/Resources/` so the bootstrapper can `Resources.Load` them), ScriptableObjects in `Core/ScriptableObjects/`, and sample splash media in `Core/Media/`. Features ship code only.

## Extension-point pattern

A feature that needs something from outside defines a static class with provider delegates and a safe fallback, and Core wires it:

```csharp
// In the feature:
public static class MyHook
{
    public static Action<string> DoThingProvider;            // set by Core
    public static void DoThing(string x)
    {
        if (DoThingProvider != null) DoThingProvider(x);
        else /* documented fallback */;
    }
}

// In Core's FeatureWiring.Wire():
MyHook.DoThingProvider = x => SomeManager.Instance?.DoThing(x);
```

## Files

- `Scripts/ManagerBootstrapper.cs` — spawns the managers (pre-scene-load).
- `Scripts/FeatureWiring.cs` — fills every feature's extension points.
- `Scripts/ToolRegistry.cs` — overridable editor tool registry.
- `Editor/` — all generators and tools (`JamTemplate.Core.Editor`, editor-only).
- `Scenes/` — the generated menu/overlay scenes (registered in Build Settings).
- `Prefabs/` (managers under `Prefabs/Resources/`), `ScriptableObjects/`, `Media/` — prefabs and content.

## Dependencies

All of them — that's the point. Nothing references Core back, so there are no cycles, and any feature folder is copyable into another project on its own (bring your own wiring, or accept the fallbacks).
