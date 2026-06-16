using System;
using System.Collections;

namespace JamTemplate.Game
{
    /// <summary>
    /// Extension point for the visual transition around the pause overlay. Core
    /// wires the providers to the Scene Transition Manager so the pause flow can
    /// reserve and animate the shared overlay surface; with nothing wired, the
    /// pause scene snaps in and out with no fade.
    /// </summary>
    public static class PauseOverlayTransition
    {
        /// <summary>Set by Core: reserves the shared transition surface. Return false when busy.</summary>
        public static Func<bool> TryBeginProvider;

        /// <summary>Set by Core: releases the reservation.</summary>
        public static Action EndProvider;

        /// <summary>Set by Core: covers the screen. May be null (no fade).</summary>
        public static Func<IEnumerator> CoverProvider;

        /// <summary>Set by Core: reveals the screen. May be null (no fade).</summary>
        public static Func<IEnumerator> RevealProvider;

        /// <summary>Reserves the transition surface. True when nothing is wired (no surface to fight over).</summary>
        public static bool TryBegin() => TryBeginProvider == null || TryBeginProvider();

        /// <summary>Releases the reservation, if wired.</summary>
        public static void End() => EndProvider?.Invoke();

        /// <summary>The cover animation, or null when none is wired.</summary>
        public static IEnumerator Cover() => CoverProvider?.Invoke();

        /// <summary>The reveal animation, or null when none is wired.</summary>
        public static IEnumerator Reveal() => RevealProvider?.Invoke();
    }
}
