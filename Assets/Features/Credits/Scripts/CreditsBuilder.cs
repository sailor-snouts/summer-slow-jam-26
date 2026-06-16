using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JamTemplate.Credits
{
    /// <summary>
    /// Instantiates the role and names text prefabs from <see cref="data"/> into
    /// <see cref="content"/> at startup, populating the credits screen.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Credits Builder")]
    [DisallowMultipleComponent]
    public class CreditsBuilder : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Credits data: sections plus the role/names prefabs.")]
        private CreditsData data;

        [SerializeField]
        [Tooltip("Container the section prefabs are added under (typically the scroll view's Content).")]
        private RectTransform content;

        private void Awake()
        {
            if (data == null || content == null)
                return;

            foreach (CreditsSection section in data.sections)
            {
                if (section == null || !HasAnyName(section.names))
                    continue;

                if (data.rolePrefab != null)
                {
                    TMP_Text role = Instantiate(data.rolePrefab, content);
                    role.text = section.role;
                }

                if (data.namesPrefab != null)
                {
                    TMP_Text names = Instantiate(data.namesPrefab, content);
                    names.text = string.Join("\n", section.names);
                }
            }
        }

        private static bool HasAnyName(List<string> names)
        {
            if (names == null)
                return false;
            foreach (string name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                    return true;
            }
            return false;
        }
    }
}
