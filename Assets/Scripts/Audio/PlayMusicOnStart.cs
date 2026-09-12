using JamTemplate.Audio;
using UnityEngine;

namespace Game
{
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
