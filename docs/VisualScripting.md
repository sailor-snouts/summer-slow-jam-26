# Visual Scripting

Unity's first-party node-based scripting (the rebranded *Bolt*), included by default
(`com.unity.visualscripting`). It lets non-programmers build game logic as graphs in
the editor — events wired to actions — without writing C#. It's here so designers can
drive the game in-engine; the programmer stays in C# for systems and hot paths.

It is **not** a feature module (it's an engine capability, not a `JamTemplate.*` island),
so it has no asmdef and no extension-point wiring — graphs call straight into Unity and
into whatever template APIs you expose to the node library (below).

## How designers use it

- **Script Graph** — a flow/logic graph asset (the Blueprints analog): an event node
  (Start, Update, On Trigger Enter, custom) flows into action nodes.
- **State Graph** — a visual state machine.
- A graph runs on a GameObject via a **Script Machine** component pointing at a Script
  Graph asset (or an embedded graph).

Quick start: `Assets ▸ Create ▸ Visual Scripting ▸ Script Graph`, add a **Script Machine**
to a GameObject, assign the graph, then open it and wire `On Start → …`.

## Exposing the template's systems as nodes (one-time setup)

Out of the box, graphs see Unity's API and `Assembly-CSharp`. To make the template's own API
available as nodes (play an `AudioEvent`, load a scene, pause the game, …):

1. **Edit ▸ Project Settings ▸ Visual Scripting**.
2. Under **Node Library**, add these assemblies:

   | Assembly | Nodes for |
   |---|---|
   | `JamTemplate.Audio` | `GameAudio.Play/PlayMusic/StopMusic`, `AudioManager` |
   | `JamTemplate.Game` | `GameManager` — pause, resume, quit |
   | `JamTemplate.SceneManagement` | `SceneTransitionManager` — load scenes, additive overlays |
   | `JamTemplate.Saving` | `SaveManager`, `HighScoreManager` |
   | `JamTemplate.Settings` | `SettingsManager` |

   Plus your jam's own gameplay assembly (`Assembly-CSharp`, already included, or your custom
   `.asmdef`). Skip `JamTemplate.Core`, any `*.Editor`, `Menus`, `SplashScreens`, `Credits` —
   designers don't drive those directly, and a leaner library regenerates faster.
3. Under **Type Options**, add `AudioEvent` and `AudioCategory` (so an Audio Event asset is
   selectable as a graph variable / object field). Add your own data types and enums as you make them.
4. Click **Regenerate Nodes**. Re-run it whenever you add or rename exposed types.

Order doesn't matter — the lists are sets; Regenerate Nodes builds the union. `GameAudio`
(not `Audio`) is the gameplay audio entry point — see [Audio.md](Audio.md).

### The "editor-only APIs" warning is expected

Adding an assembly may show: *"Nodes from this assembly can use editor-only APIs. Using them
might cause errors in builds."* This is an informational caution, **not** an error — keep the
assembly. It means the assembly has some `#if UNITY_EDITOR` members (a few template runtime
assemblies do, e.g. SceneManagement's scene-name dropdown) and Visual Scripting reflects the
editor build, so it sees them. The curated entry points above are all runtime-safe. The risk
only materializes if a designer wires up an actual editor-only node — and then the build fails
*loudly* at compile time citing `UnityEditor`, so it can't ship by accident; just remove the
offending node.

## WebGL / CI — AOT (handled automatically)

WebGL builds with IL2CPP (ahead-of-time), so Visual Scripting needs an **AOT pre-build**
that generates an `AotStubs.cs` to stop the managed-code stripper from removing APIs graphs
call via reflection. Since VS 1.5.1 this runs **automatically** on build, and VS 1.7.5 fixed
it for command-line `-batchmode` IL2CPP builds (BOLT-1649). This project is on **1.9.11**, so
the template's self-hosted `-batchmode` WebGL build generates the stubs on its own — **nothing
to commit, no extra step.**

You do **not** need to commit `AotStubs.cs`. That workaround is specific to **Unity Cloud
Build**, which suppresses the domain reload between the prebuild and build phases — this
template builds on a self-hosted runner, not Unity Cloud Build, so it doesn't apply. (The
stub is generated transiently during the build and removed after.)

Two caveats worth knowing:
- If you ever switch CI to **Unity Cloud Build**, you'd then need to do one local build and
  commit `Assets/Unity.VisualScripting.Generated/VisualScripting.Core/AotStubs.cs`. The
  `.gitignore` already leaves that file trackable (only the regenerable `UnitOptions.db`
  cache is ignored), so the workaround is available if needed.
- The first WebGL build that actually uses graphs is the real confirmation — sanity-check in
  the browser that graph behavior runs. If a jam never uses graphs, none of this matters.

## Notes

- **Performance** — fine for high-level game logic; for per-frame hot loops, do it in C#.
- **Source control** — graph assets serialize cleanly. The node-options DB is a local cache
  (gitignored); `AotStubs.cs` is the only generated file you commit.
- **Maturity** — usable and first-party, but less mature/performant than Unreal Blueprints;
  Unity has invested in it only lightly. Lean on C# for anything load-bearing.
