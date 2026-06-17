# Sailor Snouts — 2D Game Jam Template

A reusable starting point for 2D game jams, built in **Unity 6** (URP, new Input System). It ships the boring-but-essential systems — splash screens, scene transitions, a title menu, pause, settings, saving, high scores, audio, credits, and win/lose screens — so you can spend the jam on your actual game.

> **Branding vs code:** the game/brand name is **Sailor Snouts** (editor menus, Add Component menu, log prefixes). The C# namespaces and assembly definitions keep the internal name `JamTemplate.*` on purpose — don't rename them to match the brand.

## Philosophy

Every system is a **pure feature module** under `Assets/Features/<Feature>/`, each with its own assembly definition (`JamTemplate.<Feature>.asmdef`) that references **no other feature** — only engine packages. Features that need something from outside define a small static extension point with a safe fallback; the **[Core](docs/Core.md)** feature is the composition root that ties the game together: it spawns the managers, wires every extension point in one file (`FeatureWiring`), and owns all scene generators, editor tools, prefabs, and shared content. Copy any feature folder into another project and it compiles on its own — bring your own wiring or accept the fallbacks. Structure is kept minimal and explicit — serialized references over hidden singletons where practical.

Most UI is **generated as real, editable scenes** from the editor (nothing built at runtime), via `Tools ▸ Sailor Snouts`. Generated scenes are yours to restyle.

## Quick start

> Starting a jam? [docs/StartingAJam.md](docs/StartingAJam.md) is the end-to-end checklist (scenes, audio, optional FMOD + Visual Scripting, itch deploy).

1. Open the project in **Unity 6** (matching the version in `ProjectSettings/ProjectVersion.txt`).
2. The persistent managers (Audio, Scene Transition, Game, Settings, Save, High Score) are spawned automatically at startup by the **Core** feature — see [docs/Core.md](docs/Core.md) — so every scene has them, whether you launch from Splash or open a single scene directly. The first scene is **`Assets/Scenes/Splash.unity`**, which plays the splash sequence then loads the next scene.
3. Use the **`Tools ▸ Sailor Snouts`** menu to create things:
   - **`Scenes/…`** — generates a menu/overlay scene (Title, Pause, Settings, Saves, High Scores, Credits, Win, Lose) into `Assets/Features/Core/Scenes/` and registers it in Build Settings. **`Regenerate All Scenes`** deletes and recreates all of them at once (with a confirmation prompt) — a one-click reset to the stock baseline. Your `Assets/Scenes/` holds the project's own scenes (Splash and your gameplay).
   - **`Managers/…`** — managers run automatically (see step 2), but these tools can place one physically in a scene if you want to tweak its prefab values.
   - **`Tools/…`** — utilities: `Remove Redundant Managers From Build Scenes` strips leftover manager objects back out of scenes, and `Delete Save Data` wipes local saves/prefs for a clean first-run test.
4. Wire your gameplay scene names into the buttons (e.g. the Title's **New Game** button) and the Splash's **Next Scene** field.

To regenerate one generated scene after changing its generator, delete the `.unity` file (and its `.meta`) and re-run its `Tools ▸ Sailor Snouts ▸ Scenes` command — or run **`Regenerate All Scenes`** to redo every generated scene at once.

## Features

| Feature | What it gives you | Docs |
|---|---|---|
| Core | Composition root: spawns managers, wires features, owns tools & prefabs | [docs/Core.md](docs/Core.md) |
| Splash Screens | Image/video splash sequence at boot | [docs/SplashScreens.md](docs/SplashScreens.md) |
| Scene Management | Cover → load → reveal transitions, additive overlays | [docs/SceneManagement.md](docs/SceneManagement.md) |
| Menus | Menu buttons, scene builder, scaling, polish components | [docs/Menus.md](docs/Menus.md) |
| Title | Generated title menu (New Game / Continue / Load / …) | [docs/Title.md](docs/Title.md) |
| Game | App state, quit, pause/resume + timescale | [docs/Game.md](docs/Game.md) |
| Pause | Generated pause overlay scene | [docs/Pause.md](docs/Pause.md) |
| Settings | Audio/display/graphics settings with apply/confirm/revert | [docs/Settings.md](docs/Settings.md) |
| Audio | Mixer-routed audio: music, ambiance, SFX, fades, volume | [docs/Audio.md](docs/Audio.md) |
| Saving | Event-driven save slots + auto-save | [docs/Saving.md](docs/Saving.md) |
| High Scores | Persistent descending leaderboard | [docs/HighScores.md](docs/HighScores.md) |
| Credits | Data-driven scrollable credits screen | [docs/Credits.md](docs/Credits.md) |
| End Screen | Generated Win and Lose scenes | [docs/EndScreen.md](docs/EndScreen.md) |

Cross-cutting (not feature modules): an optional [FMOD audio backend](docs/AudioFmod.md) the audio system can swap to, and [Visual Scripting](docs/VisualScripting.md) for designer-built game logic without C#.

## CI

GitHub Actions ([.github/workflows/ci.yml](.github/workflows/ci.yml)) has two tiers:

**Cloud, license-free, every push/PR** — the always-on gate:
- **Static checks** — [a script](.github/scripts/Check-UnityProject.ps1) enforcing the architecture (feature asmdefs reference no `JamTemplate.*` assembly; nothing references Core), meta hygiene (missing/orphaned `.meta`, duplicate GUIDs), that every Build-Settings scene exists with a current GUID, plus `dotnet format whitespace` against [.editorconfig](.editorconfig).

**Self-hosted, manual dispatch** (Actions ▸ CI ▸ Run workflow) — the Unity jobs:
- **EditMode tests** — runs the smoke tests in `Assets/Tests/EditMode` (manager prefabs load, build list valid, feature independence). Compilation also runs [Microsoft.Unity.Analyzers](https://github.com/microsoft/Microsoft.Unity.Analyzers) (vendored in `Assets/Plugins/Analyzers`), so UNT diagnostics show in the editor console and logs.
- **C# style + Roslyn analysis** — Unity generates the solution (`CsprojGenerator`), then `dotnet format style` enforces the naming rules in [.editorconfig](.editorconfig) and `dotnet format analyzers` reports diagnostics.
- **WebGL build** — produces a downloadable WebGL build artifact, proving web compatibility continuously.
- **WebGL build (FMOD)** — *only relevant to jams using the optional [FMOD backend](docs/AudioFmod.md)*: enables FMOD, builds WebGL, then reverts the asmdef edits so the persistent checkout doesn't drift. Requires the FMOD package installed in the runner's workspace.

The Unity jobs run on a **self-hosted runner**, not the cloud: Unity 6 Personal licenses use account-token licensing and **cannot be activated headlessly** (there's no `.ulf` to hand a hosted runner — manual activation was removed, and game-ci/credential/CLI activation all fail for free accounts on Unity 6). The runner uses this machine's already-activated Unity instead — no Unity secrets at all. Set it up via [`runners/`](runners/README.md); it installs as an auto-restarting Windows service and one machine can serve many repos. A fork without the runner still gets the full cloud static-checks tier.

After the WebGL build, a final **cloud** job redeploys it:
- **Deploy to itch.io** — downloads the WebGL artifact and pushes it to itch.io with [butler](https://itch.io/docs/butler/) as a browser-playable build. License-free, so it runs in the cloud — no runner needed for the deploy itself.

## Deploy to itch.io

The `deploy-itch` job runs automatically after a successful WebGL build — on a push to `main` or on manual dispatch (it `needs` that build, so a push to `main` queues the WebGL build on the self-hosted runner first). All per-project config lives **outside** the workflow, so the same `ci.yml` retargets for every jam without edits:

| Where | Name | Value |
|---|---|---|
| Repo **secret** | `BUTLER_API_KEY` | An itch.io API key — itch.io ▸ [Settings ▸ API keys](https://itch.io/user/settings/api-keys) ▸ **Generate new key** |
| Repo **variable** | `ITCH_TARGET` | The itch target `user/game`, e.g. `roboparker/jamtemplate` |
| Repo **variable** | `ITCH_CHANNEL` | *(optional)* butler channel, defaults to `html5` |

Set them with the `gh` CLI (or via the repo's Settings ▸ Secrets and variables ▸ Actions):

```bash
gh secret   set BUTLER_API_KEY --body "<itch api key>"
gh variable set ITCH_TARGET     --body "roboparker/jamtemplate"
```

If the secret or `ITCH_TARGET` is missing, the job **skips with a notice** instead of failing — a fresh fork that hasn't set up itch yet still gets a green run.

**Make the itch project Restricted, then configure the embed (once):**

1. Create the project at itch.io ▸ [Dashboard ▸ Create new project](https://itch.io/game/new). Set **Kind of project: HTML**, and under **Visibility & access** choose **Restricted** so jam builds aren't public until you flip it to Public.
2. Dispatch CI once (Actions ▸ CI ▸ Run workflow). The first butler push creates an `html5` upload on the project.
3. On the project's edit page, tick **"This file will be played in the browser"** on that upload and set the viewport (e.g. `960×600`) and fullscreen button. itch remembers this for every later push — subsequent deploys just replace the build in place.

`butler push` is incremental and idempotent: each deploy stamps the run number as the user version and only uploads changed blocks.

## Starting a new jam from this template

This repo keeps **Git LFS** for its binary assets, and GitHub doesn't allow LFS repos to be marked as "template repos" — so there's no green *Use this template* button. Start a new jam by cloning instead:

```bash
# 1. Clone the template into a new project (no template history needed)
git clone --depth 1 git@github.com:roboparker/unity-starter-template.git my-jam
cd my-jam
git lfs pull                       # fetch the LFS-backed binaries
Remove-Item -Recurse -Force .git   # drop template history
git init -b main; git add .; git commit -m "Start my-jam from template"

# 2. Create the GitHub repo and push
gh repo create my-jam --private --source=. --remote=origin --push

# 3. Point CI at a fresh itch project (see "Deploy to itch.io" above)
gh secret   set BUTLER_API_KEY --body "<itch api key>"
gh variable set ITCH_TARGET     --body "<user>/<game>"

# 4. Add the new repo to the self-hosted runner fleet
#    Add it to runners/runners.config.json and re-run the installer.
#    See runners/README.md.
```

That's the whole loop: clone → new GitHub repo → itch secret/variable → register the runner. Cloud static checks work immediately; the Unity + deploy jobs light up once the runner is registered for the new repo.

## Odin Inspector (optional)

The template uses [Odin Inspector](https://odininspector.com/) — a **paid asset** — for nicer inspectors (scene dropdowns, conditional fields). All template code guards its Odin usage behind `#if ODIN_INSPECTOR`, so the project compiles without it; you just lose the inspector niceties. **Before publishing this repo or sharing it with people who don't own a license, remove Odin:** delete `Assets/Plugins/Sirenix/` (and its `.meta`), and remove the `ODIN_INSPECTOR*`/`ODIN_VALIDATOR*` entries from `Project Settings ▸ Player ▸ Scripting Define Symbols` for every platform.

## Conventions

- **Managers** are persistent singletons (`Instance`, `DontDestroyOnLoad`), spawned automatically at startup by the [Core](docs/Core.md) feature from its prefabs — no per-scene placement needed; any duplicate self-destroys.
- **Generated scenes** are additive overlays where it makes sense (Pause, Settings, Saves, High Scores) and rely on the underlying scene's camera/EventSystem (`EnsureCamera` / `EnsureEventSystem` cover standalone testing).
- **Save data** lives under `Application.persistentDataPath` (`Saves/slot*.json`, `autosave.json`, `highscores.json`).

## License

Copyright (c) 2026 Robert Parker

Licensed under the **MIT License** — see [LICENSE](LICENSE) for the full text. Use it freely for jams, commercial games, or anything else; just keep the copyright and license notice.
