using System;
using UnityEngine;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Decoupling seam for UI feedback sounds, so buttons can click without the UI
    /// assembly depending on the Audio assembly. The Audio feature sets the player
    /// (the AudioManager does this in its Awake). With nothing wired up, sounds
    /// are silently skipped.
    /// </summary>
    public static class UiSounds
    {
        /// <summary>Set by the Audio feature: plays a one-shot UI sound at a volume (0..1).</summary>
        public static Action<AudioClip, float> Player;

        /// <summary>Plays a one-shot UI sound, if a player is wired up.</summary>
        public static void Play(AudioClip clip, float volume = 1f)
        {
            if (clip != null)
                Player?.Invoke(clip, volume);
        }
    }
}
