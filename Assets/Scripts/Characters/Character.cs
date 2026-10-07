using UnityEngine;

namespace Game
{
    // DefaultExecutionOrder: set the DialogueActor's name before the Dialogue System reads it.
    [ExecuteAlways]
    [DefaultExecutionOrder(-100)]
    // RequireComponent isn't inherited from SpriteEntity, so restate the sprite/collider parts here.
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Mover))]
    public class Character : SpriteEntity
    {
        [Tooltip("Which character this GameObject is.")]
        [SerializeField] private CharacterData data;

        [Tooltip("At runtime, copy the character's name onto a DialogueActor on this object so dialogue addresses it as that actor.")]
        [SerializeField] private bool applyToDialogueActor = true;

        [Tooltip("Which way the character faces to begin with - and what the editor preview shows.")]
        [SerializeField] private Facing4 defaultFacing = Facing4.Down;

        private Mover mover;
        private Facing4 currentFacing = Facing4.Down;
        private bool walking;
        private int walkFrame;
        private float frameTimer;

        public CharacterData Data => data;

        public string Name => data != null ? data.DisplayName : name;

        public Sprite ProfilePicture => data != null ? data.ProfilePicture : null;

        public int GetStat(Stat stat) => data != null ? data.Get(stat) : CharacterData.MinValue;

        protected override Sprite CurrentSprite
        {
            get
            {
                if (data == null)
                    return null;
                if (Application.isPlaying)
                {
                    if (walking)
                    {
                        Sprite frame = CurrentWalkFrame();
                        if (frame != null)
                            return frame; // otherwise fall through to the standing sprite
                    }

                    Sprite worn = Outfits.WornSprite(data, currentFacing);
                    if (worn != null)
                        return worn;
                    return data.GetSprite(currentFacing);
                }
                return data.GetSprite(defaultFacing);
            }
        }

        public void SetData(CharacterData newData)
        {
            data = newData;
            RefreshSprite();
            if (Application.isPlaying && applyToDialogueActor && data != null)
                ApplyToDialogueActor();
        }

        protected virtual void Awake()
        {
            mover = GetComponent<Mover>();
            currentFacing = defaultFacing;

            // Don't dirty the DialogueActor in edit mode.
            if (Application.isPlaying && applyToDialogueActor && data != null)
                ApplyToDialogueActor();
        }

        // The outfit currently on: the equipped one, else the character's default.
        private OutfitData ActiveOutfit
        {
            get
            {
                if (data == null)
                    return null;
                OutfitData equipped = Outfits.GetEquipped(data);
                return equipped != null ? equipped : data.DefaultOutfit;
            }
        }

        protected virtual void Update()
        {
            if (!Application.isPlaying || mover == null)
                return;

            Vector2 move = mover.MoveDirection;
            bool nowWalking = move.sqrMagnitude > 1e-6f;
            bool dirty = false;

            if (nowWalking)
            {
                Facing4 next = FromVector(move);
                if (next != currentFacing)
                {
                    currentFacing = next;
                    ResetWalkCycle(); // a new direction starts its own cycle from the first frame
                    dirty = true;
                }
            }

            if (nowWalking != walking)
            {
                walking = nowWalking;
                ResetWalkCycle();
                dirty = true;
            }

            if (walking)
            {
                OutfitData outfit = ActiveOutfit;
                int cycle = WalkCycleLength();
                float fps = outfit != null ? outfit.WalkFps : 0f;
                if (cycle > 1 && fps > 0f)
                {
                    frameTimer += Time.deltaTime;
                    float frameLength = 1f / fps;
                    while (frameTimer >= frameLength)
                    {
                        frameTimer -= frameLength;
                        walkFrame = (walkFrame + 1) % cycle;
                        dirty = true;
                    }
                }
            }

            if (dirty)
                RefreshSprite();
        }

        private void ResetWalkCycle()
        {
            walkFrame = 0;
            frameTimer = 0f;
        }

        private Sprite[] WalkFrames()
        {
            OutfitData outfit = ActiveOutfit;
            return outfit != null ? outfit.GetWalkFrames(currentFacing) : null;
        }

        // A single walk frame alternates with standing (2 steps); a longer list loops through itself.
        private int WalkCycleLength()
        {
            Sprite[] frames = WalkFrames();
            if (frames == null || frames.Length == 0)
                return 0;
            return frames.Length == 1 ? 2 : frames.Length;
        }

        // The walk frame to show now, or null to show the standing sprite.
        private Sprite CurrentWalkFrame()
        {
            Sprite[] frames = WalkFrames();
            if (frames == null || frames.Length == 0)
                return null;
            if (frames.Length == 1)
                return walkFrame % 2 == 0 ? frames[0] : null; // even = step pose, odd = standing
            return frames[walkFrame % frames.Length];
        }

        public void SetFacing(Facing4 facing)
        {
            currentFacing = facing;
            RefreshSprite();
        }

        private static Facing4 FromVector(Vector2 v)
        {
            if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
                return v.x > 0f ? Facing4.Right : Facing4.Left;
            return v.y > 0f ? Facing4.Up : Facing4.Down;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Outfits.Changed += OnOutfitChanged;
        }

        protected virtual void OnDisable()
        {
            Outfits.Changed -= OnOutfitChanged;
        }

        private void OnOutfitChanged(CharacterData changed)
        {
            if (changed == data)
                RefreshSprite();
        }

        private void ApplyToDialogueActor()
        {
            // Fully-qualified to bind the base type (avoid the wrapper/namespace clash). Portrait is NOT
            // set here on purpose: dialogue portraits come from the Dialogue Editor, not CharacterData.
            var dialogueActor = GetComponent<PixelCrushers.DialogueSystem.DialogueActor>();
            if (dialogueActor == null)
                dialogueActor = gameObject.AddComponent<PixelCrushers.DialogueSystem.DialogueActor>();

            dialogueActor.actor = data.DisplayName;
        }
    }
}
