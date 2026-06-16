# Audio

Central audio service. Splits audio across five categories routed through a Unity `AudioMixer`, plays one-shot SFX/UI/dialogue and looping music/ambiance with fades and crossfades, and exposes per-category + master volume, mute, and a global fade. The `AudioManager` persists across scene loads, so music carries through transitions.

## Setup

1. Nothing to place — the Audio Manager is spawned automatically at startup by the [Core](Core.md) feature (from `Assets/Features/Core/Prefabs/Resources/Audio Manager.prefab`). Edit that prefab to configure routing (below). To also drop a copy in a scene, run **`Tools ▸ Sailor Snouts ▸ Managers ▸ Create Audio Manager`** (re-running just selects the existing one — no duplicates).
2. On the **Audio Manager** component, assign your `AudioMixer` to **Mixer**, and assign each category's `AudioMixerGroup` under **Category Routing** (Sfx, Music, Ambiance, Dialogue, Ui).
3. Expose a volume parameter on the mixer for each category. The expected exposed-parameter names are `SfxVolume`, `MusicVolume`, `AmbianceVolume`, `DialogueVolume`, `UiVolume` (each editable via the bus's `volumeParameter` field).

On `Awake` it warns if no mixer is assigned, or if a configured parameter isn't actually exposed.

## Component

**Audio Manager** — `Sailor Snouts/Audio Manager` (`[DisallowMultipleComponent]`)
- **Mixer** — the `AudioMixer`.
- **Category Routing** — five buses (`sfx`, `music`, `ambiance`, `dialogue`, `ui`), each with a `group` (`AudioMixerGroup`) and `volumeParameter` (exposed dB param name).
- **Default Fade Duration** — fade/crossfade time used by Play/Stop calls that don't specify one (default `1`).

## API

```csharp
public enum AudioCategory { Sfx, Music, Ambiance, Dialogue, Ui }

public static AudioManager Instance { get; }

// Looping music & ambiance (crossfade / fade)
public void PlayMusic(AudioClip clip);
public void PlayMusic(AudioClip clip, float fadeDuration);
public void StopMusic();  public void StopMusic(float fadeDuration);
public void PlayAmbiance(AudioClip clip);
public void PlayAmbiance(AudioClip clip, float fadeDuration);
public void StopAmbiance();  public void StopAmbiance(float fadeDuration);

// One-shots
public void PlaySfx(AudioClip clip, float volume = 1f);
public void PlaySfx(AudioClip clip, float volume, float pitchVariation); // ±random pitch per shot
public void PlayUi(AudioClip clip, float volume = 1f);
public void PlayDialogue(AudioClip clip, float volume = 1f); // replaces current line
public void StopDialogue();

// Volume & mute (linear 0..1, clamped)
public void SetVolume(AudioCategory category, float volume01);
public float GetVolume(AudioCategory category);
public void SetMasterVolume(float volume01);
public float GetMasterVolume();
public void SetMute(AudioCategory category, bool mute);
public bool IsMuted(AudioCategory category);
public void ToggleMute(AudioCategory category);

// Global fade (unscaled time — works while paused)
public void FadeOut(float duration);
public void FadeIn(float duration);
```

Notes: volumes convert to dB with a `-80 dB` silence floor. When the mixer exposes a `MasterVolume` parameter (the shipped one does), master and the global fade drive it directly and each category parameter carries only `(muted ? 0 : categoryVolume)`; without one, master and fade are folded into every category parameter instead. `PlayMusic`/`PlayAmbiance(null)` fades out; replaying the same clip is a no-op — unless that clip is mid-stop-fade, in which case the stop is cancelled and it fades back in.

## Portable audio (`AudioEvent`)

Gameplay code should play sounds through `AudioEvent` assets and the static `GameAudio` facade rather than calling the manager directly. This is the one part of the audio API that **doesn't change when a project swaps to the FMOD backend** (see [Backends](#backends)).

```csharp
// An AudioEvent asset (Create ▸ Sailor Snouts ▸ Audio Event):
//   category, volume, pitchVariation, clip   (+ an FMOD EventReference when FMOD is enabled)

public static class GameAudio          // JamTemplate.Audio
{
    public static void Play(AudioEvent e);          // dispatched by e.category
    public static void PlayMusic(AudioEvent e);     // looping
    public static void PlayAmbiance(AudioEvent e);  // looping
    public static void StopMusic();
    public static void StopAmbiance();
    public static void FadeOut(float duration);     // duck all audio (death, cutscene)
    public static void FadeIn(float duration);
}
```

```csharp
using JamTemplate.Audio;

[SerializeField] private AudioEvent jump;
[SerializeField] private AudioEvent battleTheme;

GameAudio.Play(jump);          // one-shot through the Sfx/Ui/Dialogue source for its category
GameAudio.PlayMusic(battleTheme);
```

`GameAudio` is null-safe (no-ops until the manager exists), so gameplay never has to check. `AudioManager` also has matching `Play(AudioEvent)` / `PlayMusic(AudioEvent)` / `PlayAmbiance(AudioEvent)` overloads that resolve to the clip-based methods above. With the default backend an `AudioEvent`'s payload is its `clip`; pitch variation applies to Sfx one-shots only.

## Backends

The audio system is **backend-neutral**. The default backend is Unity's built-in audio (this `AudioManager`). A jam can optionally swap to **FMOD** at compile time; the cross-cutting seams (menu sounds, transition fades, volume sliders, wired in [Core](Core.md)) and the portable `AudioEvent` / `Audio` API above are unchanged either way. Only one backend is active per project.

See **[AudioFmod.md](AudioFmod.md)** for enabling FMOD, configuration, and the WebGL specifics. Wwise is planned to follow the same pattern later.

## Usage

```csharp
using JamTemplate.Audio;

AudioManager.Instance.PlayMusic(battleTheme, 2f);     // crossfade in
AudioManager.Instance.PlaySfx(hitSfx);                // one-shot
AudioManager.Instance.SetVolume(AudioCategory.Music, 0.5f);
AudioManager.Instance.FadeOut(1f);                    // fade everything out
```

The **Settings** feature drives this manager's volumes through its audio sliders.

## Dependencies

None — `UnityEngine.Audio` only. [Core](Core.md) wires the UI's `UiSounds` and SceneManagement's `TransitionAudio` to this manager.
