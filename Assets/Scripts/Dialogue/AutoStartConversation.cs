using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Starts an NPC's conversation automatically when the scene begins - an opening beat - exactly as
    /// if the player had walked up and interacted. It reuses the NPC's <see cref="ConversationTrigger"/>,
    /// so the conversant, the acting player, the NPC freeze, and the player-input lock all behave like a
    /// normal talk, and the player regains control the instant the conversation ends. Put it on the NPC
    /// itself (it finds the trigger) or anywhere and assign the NPC.
    /// </summary>
    public class AutoStartConversation : MonoBehaviour
    {
        [SerializeField, Tooltip("The NPC to talk to on scene start. Defaults to a ConversationTrigger on this object.")]
        private ConversationTrigger npc;

        [SerializeField, Min(0f), Tooltip("Seconds to wait after the scene loads before starting, so everything is initialized.")]
        private float delay = 0.25f;

        [SerializeField, Tooltip("Only auto-start this conversation once per play session (won't replay if the scene reloads).")]
        private bool playOnce = true;

        // Conversations that have already auto-played this session (by title). Static so it survives
        // scene reloads; cleared on play start.
        private static readonly HashSet<string> played = new();

        private bool holdingLock;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => played.Clear();

        private void Start()
        {
            if (npc == null)
                npc = GetComponent<ConversationTrigger>();
            if (npc == null)
            {
                Debug.LogWarning("[AutoStartConversation] No ConversationTrigger assigned or found on this object.", this);
                return;
            }

            // Play-once: skip entirely (don't even lock) if this conversation already auto-played.
            if (playOnce && !string.IsNullOrEmpty(npc.Conversation) && played.Contains(npc.Conversation))
                return;

            // Lock movement for the whole delay so the player can't wander off before the conversation
            // begins; once it starts, the conversation's own lock (isConversationActive) takes over.
            PlayerInput.Lock();
            holdingLock = true;
            Invoke(nameof(Begin), delay);
        }

        private void Begin()
        {
            if (playOnce && !string.IsNullOrEmpty(npc.Conversation))
                played.Add(npc.Conversation);

            // Same path as the player walking up and interacting: the NPC is the conversant, the player
            // is the actor, the NPC freezes, and player input locks until the conversation ends.
            Transform player = PlayerCharacter.Current != null ? PlayerCharacter.Current.transform : null;
            npc.Interact(player);

            // Hand the lock off to the now-active conversation (if it started); release ours.
            ReleaseLock();
        }

        private void OnDisable()
        {
            // Don't strand the player locked if we're disabled/destroyed before the conversation starts.
            CancelInvoke(nameof(Begin));
            ReleaseLock();
        }

        private void ReleaseLock()
        {
            if (!holdingLock)
                return;
            holdingLock = false;
            PlayerInput.Unlock();
        }

#if UNITY_EDITOR
        // Draw a line to the NPC this will auto-talk to, so the opening-beat trigger's target is
        // obvious in the Scene view. (The component's own icon is set on the script - Bubble.png.)
        private void OnDrawGizmos()
        {
            ConversationTrigger target = npc != null ? npc : GetComponent<ConversationTrigger>();
            if (target != null && target.transform != transform)
            {
                Gizmos.color = new Color(0.24f, 0.69f, 0.78f, 0.9f);
                Gizmos.DrawLine(transform.position, target.transform.position);
            }
        }
#endif
    }
}
