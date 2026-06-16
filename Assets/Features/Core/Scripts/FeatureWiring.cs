using JamTemplate.Audio;
// The active audio backend is chosen at compile time. Both managers expose the
// same seam surface (PlayUi / FadeOut / FadeIn / Set|GetVolume / Set|GetMasterVolume),
// so aliasing the type keeps the wiring below backend-agnostic and FMOD-free in Core.
#if FMOD_PRESENT
using ActiveAudioManager = JamTemplate.Audio.FmodAudioManager;
#else
using ActiveAudioManager = JamTemplate.Audio.AudioManager;
#endif
using JamTemplate.Game;
using JamTemplate.Menus;
using JamTemplate.Saving;
using JamTemplate.SceneManagement;
using JamTemplate.Settings;
using JamTemplate.SplashScreens;
using UnityEngine;

namespace JamTemplate.Core
{
    /// <summary>
    /// The composition root's wiring: connects every feature's extension points
    /// to the feature that implements them, in one place. Providers resolve
    /// manager instances lazily, so wiring order never matters and a missing
    /// manager degrades to each extension point's documented fallback.
    /// </summary>
    public static class FeatureWiring
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Wire()
        {
            // Menus -> Game: menu Quit/Resume buttons.
            GameActions.AvailabilityProvider = () => GameManager.Instance != null;
            GameActions.QuitProvider = () => With(GameManager.Instance, game => game.Quit());
            GameActions.ResumeProvider = () => With(GameManager.Instance, game => game.Resume());
            GameActions.TogglePauseProvider = () => With(GameManager.Instance, game => game.TogglePause());

            // Menus -> Audio: button click/select sounds.
            UiSounds.Player = (clip, volume) => With(ActiveAudioManager.Instance, audio => audio.PlayUi(clip, volume));

            // Menus -> Saving: save-gated buttons and Continue.
            MenuSaveState.HasSaveProvider = () => SaveManager.Instance != null && SaveManager.Instance.HasAnySave();
            MenuSaveState.ContinueProvider = () => With(SaveManager.Instance, saves => saves.Continue());

            // Menus -> SceneManagement: menu buttons load scenes behind transitions.
            MenuSceneRouter.LoadProvider = scene => Transitions(m => m.Load(scene), () => UnityEngine.SceneManagement.SceneManager.LoadScene(scene));
            MenuSceneRouter.OpenAdditiveProvider = scene => Transitions(m => m.OpenAdditive(scene), () => UnityEngine.SceneManagement.SceneManager.LoadScene(scene, UnityEngine.SceneManagement.LoadSceneMode.Additive));
            MenuSceneRouter.CloseAdditiveProvider = scene => Transitions(m => m.CloseAdditive(scene), () => UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene));

            // SceneManagement -> Audio: fade audio with the transition cover/reveal.
            TransitionAudio.FadeOutProvider = duration => With(ActiveAudioManager.Instance, audio => audio.FadeOut(duration));
            TransitionAudio.FadeInProvider = duration => With(ActiveAudioManager.Instance, audio => audio.FadeIn(duration));

            // Game -> SceneManagement: the pause flow shares the transition overlay.
            PauseOverlayTransition.TryBeginProvider = () =>
                SceneTransitionManager.Instance == null || SceneTransitionManager.Instance.TryBeginExternalTransition();
            PauseOverlayTransition.EndProvider = () => With(SceneTransitionManager.Instance, t => t.EndExternalTransition());
            PauseOverlayTransition.CoverProvider = () =>
                SceneTransitionManager.Instance != null ? SceneTransitionManager.Instance.Transition.Cover() : null;
            PauseOverlayTransition.RevealProvider = () =>
                SceneTransitionManager.Instance != null ? SceneTransitionManager.Instance.Transition.Reveal() : null;

            // Saving -> SceneManagement: Continue/Load restore the save's scene.
            SaveSceneRouter.IsBusyProvider = () =>
                SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning;
            SaveSceneRouter.LoadProvider = scene => Transitions(m => m.Load(scene), () => UnityEngine.SceneManagement.SceneManager.LoadScene(scene));

            // SplashScreens -> SceneManagement: fades around each splash + the final load.
            SplashTransition.SnapCoveredProvider = () => With(SceneTransitionManager.Instance, t => t.Transition.SnapCovered());
            SplashTransition.CoverProvider = () =>
                SceneTransitionManager.Instance != null ? SceneTransitionManager.Instance.Transition.Cover() : null;
            SplashTransition.RevealProvider = () =>
                SceneTransitionManager.Instance != null ? SceneTransitionManager.Instance.Transition.Reveal() : null;
            SplashTransition.LoadSceneProvider = scene => Transitions(m => m.Load(scene), () => UnityEngine.SceneManagement.SceneManager.LoadScene(scene));

            // Settings -> Audio + Menus: volumes and the UI scale.
            SettingsHooks.ApplyAudio = ApplyAudio;
            SettingsHooks.CaptureAudio = CaptureAudio;
            SettingsHooks.ApplyUiScale = UIScale.Set;
            SettingsHooks.GetUiScale = () => UIScale.Current;
        }

        private static void Transitions(System.Action<SceneTransitionManager> withManager, System.Action fallback)
        {
            SceneTransitionManager manager = SceneTransitionManager.Instance;
            if (manager != null)
                withManager(manager);
            else
                fallback();
        }

        // Null-propagation on a UnityEngine.Object skips Unity's destroyed-object
        // check (UNT0008); this helper does the lifetime-aware test instead.
        private static void With<T>(T manager, System.Action<T> action) where T : Object
        {
            if (manager != null)
                action(manager);
        }

        private static void ApplyAudio(SettingsState s)
        {
            ActiveAudioManager audio = ActiveAudioManager.Instance;
            if (audio == null)
                return;

            audio.SetMasterVolume(s.masterVolume);
            audio.SetVolume(AudioCategory.Sfx, s.sfxVolume);
            audio.SetVolume(AudioCategory.Music, s.musicVolume);
            audio.SetVolume(AudioCategory.Ambiance, s.ambianceVolume);
            audio.SetVolume(AudioCategory.Dialogue, s.dialogueVolume);
            audio.SetVolume(AudioCategory.Ui, s.uiVolume);
        }

        private static void CaptureAudio(SettingsState s)
        {
            ActiveAudioManager audio = ActiveAudioManager.Instance;
            if (audio == null)
                return;

            s.masterVolume = audio.GetMasterVolume();
            s.sfxVolume = audio.GetVolume(AudioCategory.Sfx);
            s.musicVolume = audio.GetVolume(AudioCategory.Music);
            s.ambianceVolume = audio.GetVolume(AudioCategory.Ambiance);
            s.dialogueVolume = audio.GetVolume(AudioCategory.Dialogue);
            s.uiVolume = audio.GetVolume(AudioCategory.Ui);
        }
    }
}
