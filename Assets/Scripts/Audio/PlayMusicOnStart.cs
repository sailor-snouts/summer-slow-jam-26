using JamTemplate.Audio;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Plays a looping Music AudioEvent when this object starts. Drop it in a scene and assign the
    /// background track. Music is a persistent, looping channel in the audio manager, so one of these
    /// near the start of the game is enough - the track keeps playing across scene loads, and playing
    /// the same track again just no-ops. The loop itself comes from the FMOD event's loop region.
    /// </summary>
    public class PlayMusicOnStart : MonoBehaviour
    {
        [SerializeField, Tooltip("The Music-category AudioEvent to play. Its FMOD event should have a loop region so it repeats.")]
        private AudioEvent track;

        private void Start()
        {
            if (track != null)
                GameAudio.PlayMusic(track);
            else
                Debug.LogWarning("[PlayMusicOnStart] No track assigned.", this);
        }
    }
}
