#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace JamTemplate.Core
{
    /// <summary>
    /// Editor seam for the Tools ▸ Sailor Snouts menu. Every feature menu item
    /// runs through <see cref="Run"/> with its default implementation, so Core
    /// (or game code) can swap any tool out with <see cref="Override"/> — e.g.
    /// replace a feature's scene generator with one that ties several features
    /// together. Register overrides from an <c>[InitializeOnLoad]</c> class so
    /// they exist before the menu is used. A tool's id is its menu path under
    /// <c>Tools/Sailor Snouts/</c>, e.g. <c>"Scenes/Create Title Scene"</c>.
    /// </summary>
    public static class ToolRegistry
    {
        private static readonly Dictionary<string, Action> Overrides = new Dictionary<string, Action>();

        /// <summary>Replaces tool <paramref name="id"/>'s implementation. Pass null to restore the feature default.</summary>
        public static void Override(string id, Action implementation)
        {
            if (implementation == null)
                Overrides.Remove(id);
            else
                Overrides[id] = implementation;
        }

        /// <summary>Runs the override registered for <paramref name="id"/>, or <paramref name="featureDefault"/> when none is.</summary>
        public static void Run(string id, Action featureDefault)
        {
            if (Overrides.TryGetValue(id, out Action implementation))
                implementation();
            else
                featureDefault?.Invoke();
        }
    }
}
#endif
