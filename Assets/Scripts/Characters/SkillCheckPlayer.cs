using UnityEngine;

namespace Game
{
    /// <summary>
    /// Marks this object's <see cref="CharacterStats"/> as the default "player" used by skill
    /// checks that don't name a character (notably dialogue checks). Put it on your player.
    /// Keeps <see cref="CharacterStats"/> itself generic — the "who is the player" binding lives here.
    /// </summary>
    [RequireComponent(typeof(CharacterStats))]
    [DisallowMultipleComponent]
    public class SkillCheckPlayer : MonoBehaviour
    {
        private CharacterStats stats;

        private void Awake() => stats = GetComponent<CharacterStats>();

        private void OnEnable() => SkillCheck.DefaultStats = stats;

        private void OnDisable()
        {
            // Only clear it if we're still the active default (don't stomp a newer player).
            if (SkillCheck.DefaultStats == stats)
                SkillCheck.DefaultStats = null;
        }
    }
}
