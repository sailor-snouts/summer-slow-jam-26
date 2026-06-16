using System;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Decoupling seam for app-level actions, so menu buttons can quit, resume or
    /// toggle pause without the UI assembly depending on the Game assembly. The
    /// Game feature sets the providers (the GameManager does this in its Awake).
    /// With nothing wired up, every call is a no-op.
    /// </summary>
    public static class GameActions
    {
        /// <summary>Set by Core: whether a game manager is present.</summary>
        public static Func<bool> AvailabilityProvider;

        /// <summary>Set by Core: quits the game.</summary>
        public static Action QuitProvider;

        /// <summary>Set by Core: resumes from pause.</summary>
        public static Action ResumeProvider;

        /// <summary>Set by Core: pauses if playing, resumes if paused.</summary>
        public static Action TogglePauseProvider;

        /// <summary>Whether a game manager is wired up and present.</summary>
        public static bool IsAvailable =>
            AvailabilityProvider != null ? AvailabilityProvider() : QuitProvider != null;

        /// <summary>Quits the game, if a provider is wired up.</summary>
        public static void Quit() => QuitProvider?.Invoke();

        /// <summary>Resumes from pause, if a provider is wired up.</summary>
        public static void Resume() => ResumeProvider?.Invoke();

        /// <summary>Toggles pause, if a provider is wired up.</summary>
        public static void TogglePause() => TogglePauseProvider?.Invoke();
    }
}
