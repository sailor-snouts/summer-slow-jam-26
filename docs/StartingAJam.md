# Starting a new jam

A checklist for spinning up a jam from this template. Most of it is optional — the
default project already runs. See the [README](../README.md) for the philosophy and the
per-feature docs for detail; this page is just the order of operations.

## 1. Create the project
- Make your jam repo from the template (copy or "Use this template" — note the repo can't
  be a GitHub *template* because it uses Git LFS, so cloning/forking is fine).
- Open in **Unity 6** (match `ProjectSettings/ProjectVersion.txt`).
- Set your game's display name + company in **Project Settings ▸ Player**. Leave the
  `JamTemplate.*` namespaces/asmdefs alone (the brand name "Sailor Snouts" in menus/logs is
  cosmetic; the code name is intentional).

## 2. Sanity-check the baseline
- Press Play from `Assets/Scenes/Splash.unity` — splash → title should work.
- Do one build (editor or the CI `webgl` job) so you know the toolchain is good before you
  pile on content.

## 3. Scenes
- `Tools ▸ Sailor Snouts ▸ Scenes ▸ …` generates the menu/overlay scenes; **Regenerate All
  Scenes** resets them to baseline. Restyle the generated scenes freely.
- Put your gameplay scenes in `Assets/Scenes/`.
- Wire scene names: the Splash's **Next Scene**, the Title's **New Game** button, etc.

## 4. Audio (works out of the box)
- Create sounds as **AudioEvent** assets (`Create ▸ Sailor Snouts ▸ Audio Event`) and play
  them from gameplay with `GameAudio.Play(audioEvent)` / `PlayMusic` — see [Audio.md](Audio.md).
- Set the menu button click sounds, mixer groups, etc. as desired.

## 5. Optional — FMOD audio backend
Only if a jam wants FMOD instead of Unity audio (one backend per jam). Full steps in
[AudioFmod.md](AudioFmod.md), in short:
1. Install the FMOD Unity package.
2. `Tools ▸ Sailor Snouts ▸ Audio ▸ Enable FMOD`, then **Create FMOD Audio Manager Prefab**.
3. In FMOD Studio: author banks, and create VCAs `Sfx/Music/Ambiance/Dialogue/UI` (else the
   per-category volume sliders no-op). Assign the prefab's bank names + UI click event.
4. Put your FMOD `EventReference`s on the AudioEvent assets.
5. Commit the FMOD package + the asmdef changes **in your jam repo** (the template itself
   stays FMOD-free).

## 6. Optional — Visual Scripting for designers
The package ships by default and the template's Node Library is already configured, so the
template's APIs show up as nodes (you may need one **Regenerate Nodes** click to build the
local cache — see [VisualScripting.md](VisualScripting.md)). WebGL AOT is handled
automatically on this version (1.9.11) for the self-hosted CI build — no `AotStubs.cs` to
commit unless you switch to Unity Cloud Build.

## 7. Deploy to itch.io
CI pushes WebGL builds to itch with butler (see [README ▸ CI](../README.md#ci)). Configure
once on the jam repo:
- Secret **`BUTLER_API_KEY`** — an itch.io API key (itch.io ▸ Settings ▸ API keys).
- Variable **`ITCH_TARGET`** — `user/game` (e.g. `roboparker/my-jam`).
- Variable **`ITCH_CHANNEL`** — optional, defaults to `html5`.
The self-hosted Unity jobs (build/test) are **manual-dispatch only**; the cloud static
checks run on every push.

## 8. Before you ship
- `Tools ▸ Sailor Snouts ▸ Tools ▸ Remove Redundant Managers From Build Scenes` if you
  placed any managers into scenes manually.
- Build WebGL, play it in a browser, check audio actually starts (the first-input gate),
  and confirm volume sliders + saves work.
