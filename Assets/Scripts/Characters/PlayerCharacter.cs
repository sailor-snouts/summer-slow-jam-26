using UnityEngine;

namespace Game
{
    public class PlayerCharacter : Character
    {
        [Header("Characters")]
        [SerializeField] private CharacterData characterA;
        [SerializeField] private CharacterData characterB;

        private bool usingA = true;

        // Static so the swap choice survives scene loads; cleared on play start so it lasts the session only.
        private static CharacterData sessionCharacter;

        public static PlayerCharacter Current { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            Current = null;
            sessionCharacter = null;
        }

        public static CharacterData CurrentData => Current != null ? Current.Active : null;

        public CharacterData Active => usingA ? characterA : characterB;

        protected override void OnEnable()
        {
            base.OnEnable();
            Current = this;

            // Restore the character chosen earlier this session; if nothing matches, keep the serialized default.
            if (sessionCharacter == characterB)
                usingA = false;
            else if (sessionCharacter == characterA)
                usingA = true;

            Apply();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (Current == this)
                Current = null;
        }

        // Runs after every object in the loaded scene is awake, so all SceneEntrances exist.
        private void Start()
        {
            if (!Application.isPlaying)
                return;
            PlaceAtPendingEntrance();
        }

        private void PlaceAtPendingEntrance()
        {
            string id = SceneTransition.PendingEntrance;
            if (string.IsNullOrEmpty(id))
                return;

            SceneTransition.PendingEntrance = null; // consume it so it applies to this arrival only

            SceneEntrance[] entrances = FindObjectsByType<SceneEntrance>(FindObjectsInactive.Include);
            foreach (SceneEntrance entrance in entrances)
            {
                if (entrance.Id != id)
                    continue;
                transform.position = entrance.transform.position;
                SetFacing(entrance.Facing);
                return;
            }

            Debug.LogWarning(
                $"[PlayerCharacter] No SceneEntrance with id '{id}' in this scene; " +
                "staying at the authored start position.", this);
        }

        public void Swap()
        {
            usingA = !usingA;
            Apply();
        }

        public bool SetActiveByName(string actorName)
        {
            if (characterA != null && characterA.DisplayName == actorName)
            {
                usingA = true;
                Apply();
                return true;
            }
            if (characterB != null && characterB.DisplayName == actorName)
            {
                usingA = false;
                Apply();
                return true;
            }

            Debug.LogWarning(
                $"[PlayerCharacter] No player character named '{actorName}'. Options are " +
                $"'{(characterA != null ? characterA.DisplayName : "none")}' and " +
                $"'{(characterB != null ? characterB.DisplayName : "none")}'.", this);
            return false;
        }

        private void Apply()
        {
            CharacterData active = Active;
            if (active == null)
                return;

            sessionCharacter = active;

            // Start them in their default outfit if none is equipped yet, so the worn outfit is always
            // well-defined and its modifiers apply.
            if (Outfits.GetEquipped(active) == null && active.DefaultOutfit != null)
                Outfits.Equip(active, active.DefaultOutfit);

            SetData(active);
            SkillCheck.DefaultCharacter = active;
        }
    }
}
