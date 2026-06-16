# Audio — FMOD backend

The template ships on Unity's built-in audio (see [Audio.md](Audio.md)). A jam can optionally swap the whole audio backend to **FMOD** at compile time. The cross-cutting seams (menu sounds, transition fades, volume sliders) and the portable `AudioEvent` / `GameAudio` gameplay API are identical on both backends, so swapping changes no call sites. One backend is active per project; Wwise is planned to follow the same pattern.

The base template carries **zero FMOD weight** — no package, no banks, no plugins. Everything FMOD-specific lives behind `#if FMOD_PRESENT` and is dormant until you opt in.

## Enabling FMOD

1. **Install the FMOD Unity package** yourself (the toggle does not download it). FMOD's WebGL support is part of the base integration — no separate download. The `FMODUnity` assembly must exist in the project.
2. **`Tools ▸ Sailor Snouts ▸ Audio ▸ Enable FMOD`.** This edits assembly definitions in place:
   - adds the `FMOD_PRESENT` versionDefine to the asmdefs that carry `#if FMOD_PRESENT` code — `JamTemplate.Audio`, `JamTemplate.Core`, `JamTemplate.Core.Editor`;
   - adds the `FMODUnity` and `Unity.InputSystem` references to `JamTemplate.Audio` only (the one island that compiles against FMOD's runtime and watches for the first-gesture autoplay unlock).
   It refuses to run if the FMOD package isn't installed, and is re-runnable — running it again only adds anything newly required (e.g. after pulling a template update).
3. After Unity recompiles, **`Tools ▸ Sailor Snouts ▸ Audio ▸ Create FMOD Audio Manager Prefab`** to generate `Assets/Features/Core/Prefabs/Resources/Fmod Audio Manager.prefab` — the prefab the bootstrapper spawns at runtime.
4. Configure the prefab (below).

**`Tools ▸ Sailor Snouts ▸ Audio ▸ Disable FMOD`** reverses the asmdef edits and returns the project to Unity's built-in audio. (The generated prefab is left in place but unused — delete it manually if you want.)

LFS patterns for `*.bank` and `*.dylib` are already in `.gitattributes`, so banks and FMOD's native libs are tracked by Git LFS once committed (`.dll`/`.so`/`.a` were already LFS-tracked).

## How it works

When `FMOD_PRESENT` is defined the composition root switches backend:

- `ManagerBootstrapper` spawns **Fmod Audio Manager** instead of **Audio Manager**.
- `FeatureWiring` aliases the active manager type (`ActiveAudioManager`), so the seam bindings stay single-path and `JamTemplate.Core` never names the `FMOD` namespace (only the `JamTemplate.Audio.FmodAudioManager` type).
- `FmodAudioManager` satisfies the same seams (`PlayUi`, `FadeOut`/`FadeIn`, per-category + master volume) and the same `AudioEvent` API.

## Configuration (Fmod Audio Manager prefab)

- **Banks** — bank files to load at startup, without `.bank` (defaults `Master`, `Master.strings`). Author banks **non-streaming / loaded to RAM** so the readiness wait is bounded.
- **Mixer Routing** — the master bus path (`bus:/`) folds in master volume + the global fade; per-category VCA paths (`vca:/Sfx`, `vca:/Music`, `vca:/Ambiance`, `vca:/Dialogue`, `vca:/UI`) carry per-category volume/mute. FMOD VCA/bus volumes are **linear** gain (no dB conversion, unlike the Unity mixer).
  - **Master volume + the global fade work out of the box** (every FMOD project has a master bus).
  - **Per-category volume does not until you set up the VCAs.** A new FMOD Studio project has no VCAs, so `vca:/Sfx` etc. won't resolve and the Settings sliders for those categories silently do nothing (you'll see FMOD "VCA not found" warnings). To wire them up: in FMOD Studio create VCAs named `Sfx`/`Music`/`Ambiance`/`Dialogue`/`UI`, route each event's fader to its VCA, rebuild banks. Or rename the paths on the prefab to match VCAs you already have.
- **UI** — a single `uiClickEvent` plays for all menu button feedback. The clip passed through the UI-sound seam is **ignored** under FMOD (FMOD plays its own authored event), so menu select vs. press resolve to the same event.

## Gameplay

Identical to the default backend — play `AudioEvent` assets:

```csharp
GameAudio.Play(jump);            // uses the AudioEvent's FMOD EventReference
GameAudio.PlayMusic(battleTheme);
```

When FMOD is enabled, `AudioEvent` exposes an **FMOD `EventReference`** field; assign your authored events there. (The Unity `clip` field is ignored by the FMOD backend.)

Two behavioral differences from the Unity backend, both minor: `Music`/`Ambiance` events loop (same as Unity), but every other category — including `Dialogue` and `Ui` — plays as a plain one-shot of its event, so FMOD doesn't replicate the Unity backend's "new dialogue line stops the previous one" behavior (author that into the FMOD event if you need it). And music/ambiance switch by stopping the old event with `ALLOWFADEOUT` and starting the new one, so crossfade/fade timing comes from the FMOD event's own fades rather than the Unity backend's fixed crossfade.

## WebGL specifics

WebGL is the primary ship target, and `FmodAudioManager` is built around three browser constraints:

- **Async bank loading.** Banks stream in over several frames. Playback waits on `HaveAllBanksLoaded` + `AnySampleDataLoading` before resolving buses or playing. Music/ambiance requested before banks are ready are **queued and replayed**; transient one-shots are **dropped** rather than thrown.
- **Autoplay gate.** Browsers keep the audio context suspended until the first real user gesture — the #1 cause of a silent web build. `FmodAudioManager` watches for the first key/click/tap/gamepad input (via the Input System) and calls `NotifyUserGesture()` → `RuntimeManager.CoreSystem.mixerResume()`. It detects input directly rather than piggybacking on UI sounds, which under FMOD often carry no clip and so never fire. Focus/visibility changes suspend and resume too. (Enabling FMOD adds the `Unity.InputSystem` reference to the Audio assembly for this.)
- **Single-threaded (HTML5).** No threads; raise the DSP buffer if audio stutters.

Unsupported on WebGL: convolution reverb, ambisonic/Resonance spatializers (verify per project).

## Status

Phases 1–3 and 6 are implemented (portable `AudioEvent` + facade, `FmodAudioManager`, conditional bootstrap/wiring, the scripted opt-in, LFS, docs). The FMOD C# compiles against FMOD 2.03.

**Phase 4 (CI) — done, pending a live runner check.** The cloud static-checks + meta hygiene pass both FMOD-absent and FMOD-present (the meta check skips plugin-bundle internals like FMOD's macOS `*.bundle`s). `FmodPackageToggle.EnableForCi`/`DisableForCi` are public `-executeMethod` hooks, and `Run-Unity.ps1` has `enable-fmod`/`disable-fmod` modes. Backend-neutral EditMode tests (`AudioEventTests`) are in place. A manual-dispatch `webgl-build-fmod` job (in `ci.yml`) enables FMOD, builds WebGL, and always reverts the asmdef edits so the persistent checkout doesn't drift. It needs the FMOD package pre-installed in the self-hosted runner's workspace (FMOD isn't committed; `clean: false` keeps it between runs) — install it once under `Assets/Plugins/FMOD`. The job itself is unverified until first dispatched on a runner that has FMOD.

**Phase 5 (WebGL gate) — pending a real build.** The WebGL-tolerant code is in place and the first-gesture resume is hardened (input-based). What remains needs FMOD banks + a build: the first real FMOD WebGL build to itch, verifying all banks async-load before the first gameplay scene, first audio unlocks on the first input, suspend/resume on focus, and banks are non-streaming/in-RAM.

**Note on committing FMOD:** the template stays FMOD-absent — the FMOD package and the asmdef enable-edits are a per-project opt-in and are not committed to the template. A jam that wants FMOD installs it and runs Enable in its own repo.
