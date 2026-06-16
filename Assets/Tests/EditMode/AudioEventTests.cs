using JamTemplate.Audio;
using NUnit.Framework;
using UnityEngine;

namespace JamTemplate.Tests
{
    /// <summary>
    /// Backend-neutral audio behaviour that holds on the default (Unity) backend:
    /// the portable <see cref="AudioEvent"/> payload defaults and the <see cref="GameAudio"/>
    /// facade's null-safety (gameplay must be able to play sounds before any manager
    /// exists without throwing). Runs with FMOD absent.
    /// </summary>
    public class AudioEventTests
    {
        [Test]
        public void NewAudioEventHasSfxDefaults()
        {
            var audioEvent = ScriptableObject.CreateInstance<AudioEvent>();
            try
            {
                Assert.AreEqual(AudioCategory.Sfx, audioEvent.category);
                Assert.AreEqual(1f, audioEvent.volume);
                Assert.AreEqual(0f, audioEvent.pitchVariation);
                Assert.IsNull(audioEvent.clip);
            }
            finally
            {
                Object.DestroyImmediate(audioEvent);
            }
        }

        [Test]
        public void AudioFacadeDoesNotThrowWithoutAManager()
        {
            // No manager is bootstrapped in EditMode tests, so these route through the
            // facade's null guard. The contract is "never throw", whether or not a
            // manager happens to exist.
            var audioEvent = ScriptableObject.CreateInstance<AudioEvent>();
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    GameAudio.Play(audioEvent);
                    GameAudio.PlayMusic(audioEvent);
                    GameAudio.PlayAmbiance(audioEvent);
                    GameAudio.StopMusic();
                    GameAudio.StopAmbiance();
                    GameAudio.Play(null);
                });
            }
            finally
            {
                Object.DestroyImmediate(audioEvent);
            }
        }
    }
}
