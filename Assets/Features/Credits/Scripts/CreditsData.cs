using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using TMPro;
using UnityEngine;

namespace JamTemplate.Credits
{
    /// <summary>One credits section: a role heading and the people credited under it.</summary>
    [System.Serializable]
    public class CreditsSection
    {
        [Tooltip("Role heading, e.g. \"Programming\".")]
        public string role = "Role";

        [Tooltip("People credited under this role, one per line.")]
        public List<string> names = new List<string>();
    }

    /// <summary>
    /// Credits data — the role sections plus the TMP text prefabs the Credits
    /// scene's <see cref="CreditsBuilder"/> instantiates for each section.
    /// Restyle the prefabs to change how role headings and names look.
    /// </summary>
    [CreateAssetMenu(fileName = "Credits", menuName = "Sailor Snouts/Credits")]
    public class CreditsData : ScriptableObject
    {
        [Header("Prefabs")]
        [Tooltip("TMP text prefab instantiated as each role heading.")]
        public TMP_Text rolePrefab;

        [Tooltip("TMP text prefab instantiated as each section's names.")]
        public TMP_Text namesPrefab;

        [Header("Sections")]
#if ODIN_INSPECTOR
        [ListDrawerSettings(ListElementLabelName = nameof(CreditsSection.role))]
#endif
        [Tooltip("Role sections, top to bottom. Add, remove and reorder freely.")]
        public List<CreditsSection> sections = new List<CreditsSection>
        {
            new CreditsSection { role = "Design", names = new List<string> { "Your Name" } },
            new CreditsSection { role = "Programming", names = new List<string> { "Your Name" } },
            new CreditsSection { role = "Art", names = new List<string> { "Your Name" } },
            new CreditsSection { role = "Music & Sound", names = new List<string> { "Your Name" } },
        };
    }
}
