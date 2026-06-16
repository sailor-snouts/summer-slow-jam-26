using System;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Decoupling seam that lets menu buttons react to save state without the UI
    /// assembly depending on the Saving assembly. The Saving feature sets the
    /// providers (the SaveManager does this in its Awake); menus read
    /// <see cref="HasSave"/> and call <see cref="Continue"/>. With nothing wired up
    /// (no SaveManager present), <see cref="HasSave"/> is false and
    /// <see cref="Continue"/> is a no-op.
    /// </summary>
    public static class MenuSaveState
    {
        /// <summary>Set by the Saving feature: returns whether any save exists.</summary>
        public static Func<bool> HasSaveProvider;

        /// <summary>Set by the Saving feature: resumes the most recent save.</summary>
        public static Action ContinueProvider;

        /// <summary>Whether a save exists. False until something provides an answer.</summary>
        public static bool HasSave => HasSaveProvider != null && HasSaveProvider();

        /// <summary>Resumes the most recent save, if a provider is wired up.</summary>
        public static void Continue() => ContinueProvider?.Invoke();
    }
}
