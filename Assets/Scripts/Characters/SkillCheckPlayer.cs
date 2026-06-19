using UnityEngine;

namespace Game
{
    /// <summary>
    /// Marks this object's selected <see cref="Character"/> as the default "player" used by skill
    /// checks that don't name a character (notably dialogue checks). Put it on your player.
    /// </summary>
    [RequireComponent(typeof(Character))]
    [DisallowMultipleComponent]
    public class SkillCheckPlayer : MonoBehaviour
    {
        private Character character;

        private void Awake() => character = GetComponent<Character>();

        private void OnEnable()
        {
            if (character != null)
                SkillCheck.DefaultCharacter = character.Data;
        }

        private void OnDisable()
        {
            // Only clear it if we're still the active default (don't stomp a newer player).
            if (character != null && SkillCheck.DefaultCharacter == character.Data)
                SkillCheck.DefaultCharacter = null;
        }
    }
}
