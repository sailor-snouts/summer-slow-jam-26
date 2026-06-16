# Splash Screens

Plays an ordered list of splash screens (images and/or videos) at game start, then transitions to a configured next scene. The controller builds its own full-screen canvas at runtime, so it drops into an otherwise empty scene. Players can optionally skip individual splashes or the whole sequence.

## Setup

This feature ships as a **prefab**, not a generated scene — there's no Tools menu item.

1. Add `Assets/Features/Core/Prefabs/Splash Screen Controller.prefab` to your first/splash scene (the default `Splash.unity` already has it).
2. On the **Splash Screen Controller**, set **Next Scene** (must be an enabled Build Settings scene — it's validated), and populate the **Splash Sequence** (`entries`) list.
3. Optionally adjust skip behavior.

Sample media is under `Assets/Features/Core/Media/`.

## Components & data

**Splash Screen Controller** — `Sailor Snouts/Splash Screen Controller` (`[DisallowMultipleComponent]`)
- **Next Scene** — scene loaded after the last splash (default `"SampleScene"`).
- **Splash Sequence** — the `SplashEntry` list, in order.
- **Allow Skip** (default true) — any key/click/tap/gamepad button skips the current splash; Escape skips the whole sequence.
- **Skip Input Delay** (default 0.35s) — ignores skip input briefly so a held button can't blow past the first splash.

**SplashEntry** (serializable data, one per splash):
- **Media Type** — `Image` or `Video`.
- **Image** (`Sprite`) — for image entries (letterboxed).
- **Video** (`VideoClip`) — for video entries; the splash holds until the clip ends. Video plays visuals only — use the audio clip for sound. **Imported clips do not play on WebGL** — use Video Url there.
- **Video Url** — streams the video from a URL, or a path relative to `Assets/StreamingAssets/` (e.g. `intro.mp4`). Takes precedence over the clip, and is the only video option that works on WebGL (entries with only a clip are skipped there with a warning).
- **Background Color** — solid fill behind the media.
- **Hold Duration** (image only, default 1.5s) — how long the image stays.
- **Audio Clip** + **Audio Volume** (0–1) — optional sound when the splash appears.

Fades each splash in/out via the Scene Transition Manager, then calls `SceneTransitionManager.Instance.Load(nextScene)` (falling back to a raw load if no transition manager exists). No public scripting API — it's self-driving.

## Dependencies

None — Input System (with legacy fallback), `UnityEngine.Video`, Odin Inspector. Owns the `SplashTransition` extension point ([Core](Core.md) wires the transition manager into it; hard cuts + raw load when unwired). The controller prefab and sample media live in Core.
