# Menus

The shared UI toolkit: one component turns any Unity UI Button into a menu action, plus polish components (hover-pop, controller focus, blurred backgrounds), self-healing scene guards (`EnsureCamera`, `EnsureEventSystem`), and an editor scene builder the generators in [Core](Core.md) use. It references no other feature — it owns static extension points (`MenuSaveState`, `UIScale`, `GameActions`, `UiSounds`, `MenuSceneRouter`) that Core wires to the managers at startup.

## Components

**Menu Button Action** — `Sailor Snouts/Menu Button Action` (`[RequireComponent(typeof(Button))]`)
Turns a Button into an action; auto-subscribes to `onClick` in `Awake` (no manual wiring). Fields:
- **Action** (`MenuAction`): `LoadScene`, `Quit`, `Event`, `Resume`, `OpenAdditive`, `CloseSelf`, `Continue`.
- **Scene** — shown only for `LoadScene`/`OpenAdditive`; a Build Settings dropdown.
- **Visible When** (`SaveVisibility`): `Always`, `WhenSaveExists`, `WhenNoSave` — save-gated visibility, evaluated once in `Start` (hides the GameObject when unmet). This is how Continue/Load Game appear only when a save exists.
- **On Click** (`UnityEvent`) — shown only for `Action = Event`.

Action behavior: `LoadScene`/`OpenAdditive`/`CloseSelf` → `MenuSceneRouter` (transition-backed when Core wires it, raw `SceneManager` fallback otherwise); `Continue` → `MenuSaveState.Continue()`; `Quit`/`Resume` → `GameActions`; `Event` → the UnityEvent.

**Menu Button** — `Sailor Snouts/Menu Button` — scale-pop while highlighted (hover or controller focus) + optional `selectSound`/`pressSound` (via the `UiSounds` seam, filled by the AudioManager). Fields: `highlightScale` (1.1), `scaleDuration` (0.12).

**Initial Selection** — `Sailor Snouts/Initial Selection` — on `Start`, selects its GameObject so keyboard/controller navigation has a starting point. Put it on the first button.

**Ensure Event System** — `Sailor Snouts/Ensure Event System` — creates an EventSystem (Input System UI module) only if none exists. For additive overlays.

**Ensure Camera** — `Sailor Snouts/Ensure Camera` — creates a plain 2D camera (+ AudioListener) only if none exists. Lets overlay scenes render when opened standalone.

**UI Scaler** — `Sailor Snouts/UI Scaler` (`[RequireComponent(typeof(CanvasScaler))]`) — applies the global `UIScale.Current` by dividing the CanvasScaler reference resolution; re-applies on change. Auto-added to canvases built by `MenuSceneBuilder`.

**Scene Blur Background** — `Sailor Snouts/Scene Blur Background` — captures a camera, blurs it (shader `Hidden/JamTemplate/Blur`), and shows it as a full-screen backdrop. Fields: `sourceCamera`, `downsample` (1–4), `iterations` (1–8), `blurSize`, `sortingOrder`.

## Extension points

The UI owns its extension points — `MenuSaveState` (save-gated buttons), `UIScale` (canvas scaling), `GameActions` (Quit/Resume), `UiSounds` (click sounds), `MenuSceneRouter` (scene loads) — and [Core](Core.md)'s `FeatureWiring` fills them at startup. Each falls back safely when unwired: raw scene loads, no sounds, no save detected.

`UIInput.Configure(InputSystemUIInputModule)` wires an input module to the project's Input Actions (used by `EnsureEventSystem` and the scene builder).

## MenuSceneBuilder (editor only)

`public static class MenuSceneBuilder` (wrapped in `#if UNITY_EDITOR`) assembles menu UI as real, editable GameObjects and generates scenes — nothing is built at runtime. The scene generators across the template call it. Key methods: `EnsureScene`, `OpenOrCreate`, `CreateCanvas`, `CreateEventSystem`, `CreateBackground`, `CreateText`, `CreateButtonColumn`, `CreateButton`, `CreateActionlessButton`, `CreateScrollView`, `CreateSectionHeader`, `CreateSettingRow`, `CreateSlider`, `CreateDropdown`, `CreateToggle`, `RegisterInBuildSettings`, `Stretch`. Constants `DarkBackground`, `ButtonColor`.

## Usage

Configure a button in the inspector: add a Button, attach **Menu Button Action**, set **Action** (e.g. `OpenAdditive`), pick the **Scene** (e.g. `Settings`); for a save-gated button set **Visible When** = `WhenSaveExists`. For custom logic set `Action = Event` and wire the **On Click** field.

```csharp
using JamTemplate.Menus;
UIScale.Set(1.25f); // 125% — every live UIScaler re-applies
```

## Dependencies

None — engine packages only (Input System, TextMeshPro, Odin Inspector). The UI owns its extension points (`MenuSaveState`, `UIScale`, `GameActions`, `UiSounds`, `MenuSceneRouter`); [Core](Core.md) wires them to the managers at startup, and every one falls back safely when unwired.
