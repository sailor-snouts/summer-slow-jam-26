using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AutoStartConversation : MonoBehaviour
    {
        [SerializeField, Tooltip("The NPC to talk to on scene start. Defaults to a ConversationTrigger on this object.")]
        private ConversationTrigger npc;

        [SerializeField, Min(0f), Tooltip("Seconds to wait after the scene loads before starting, so everything is initialized.")]
        private float delay = 0.25f;

        [SerializeField, Tooltip("Only auto-start this conversation once per play session (won't replay if the scene reloads).")]
        private bool playOnce = true;

        // Static so it persists across scene changes (returning to a scene won't replay); cleared only
        // on play start by ResetState below, so a fresh Play run replays but a single run does not.
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

            string conversation = npc.Conversation;

            // Mark it played now (before the delay) so leaving the scene mid-delay still counts and it
            // won't replay on return.
            if (playOnce && !string.IsNullOrEmpty(conversation))
            {
                if (played.Contains(conversation))
                    return;
                played.Add(conversation);
            }

            // Lock movement for the delay so the player can't wander off before the conversation begins;
            // once it starts, the conversation's own lock (isConversationActive) takes over.
            PlayerInput.Lock();
            holdingLock = true;
            Invoke(nameof(Begin), delay);
        }

        private void Begin()
        {
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
