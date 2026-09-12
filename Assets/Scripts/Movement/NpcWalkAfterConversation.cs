using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Mover))]
    [DisallowMultipleComponent]
    public class NpcWalkAfterConversation : MonoBehaviour
    {
        [SerializeField, Tooltip("Points the NPC walks through in order once the conversation ends. Empty = do nothing.")]
        private List<Transform> waypoints = new();

        [SerializeField, Min(0f), Tooltip("How close (world units) counts as reaching a waypoint.")]
        private float arriveDistance = 0.15f;

        [SerializeField, Min(0f), Tooltip("Walk speed along the path. 0 keeps the Mover's own speed.")]
        private float walkSpeed = 0f;

        [SerializeField, Min(0f), Tooltip("Seconds to wait after the conversation ends before the NPC starts walking. The player stays locked during the wait.")]
        private float startDelay = 0f;

        [SerializeField, Tooltip("Keep the player locked until the NPC finishes the path (that is the whole point of this beat).")]
        private bool lockPlayerUntilArrived = true;

        [SerializeField, Tooltip("Destroy this GameObject once it reaches the end of the path.")]
        private bool destroyOnArrive;

        [SerializeField, Min(0f), Tooltip("Safety: give up and return control after this many seconds if the path can't be finished (0 = no limit).")]
        private float maxWalkSeconds = 12f;

        private Mover mover;
        private NpcController npc;
        private bool walking;
        private bool pending;
        private float delayTimer;
        private bool holdingLock;
        private int index;
        private float walkTimer;
        private NpcWalkMode resumeMode;
        private float cachedSpeed;
        private bool speedCached;

        private bool HasPath
        {
            get
            {
                if (waypoints == null)
                    return false;
                foreach (Transform w in waypoints)
                    if (w != null)
                        return true;
                return false;
            }
        }

        private void Awake()
        {
            mover = GetComponent<Mover>();
            npc = GetComponent<NpcController>();
        }

        // OnConversationStart / OnConversationEnd are SendMessage'd by the Dialogue System - names must match exactly.
        private void OnConversationStart(Transform actor)
        {
            if (!HasPath)
                return;

            // Take the lock now, while the conversation is already holding the player, so there is no
            // free frame between the conversation ending and the walk starting.
            if (lockPlayerUntilArrived && !holdingLock)
            {
                PlayerInput.Lock();
                holdingLock = true;
            }
        }

        private void OnConversationEnd(Transform actor)
        {
            if (!HasPath)
            {
                ReleaseLock();
                return;
            }

            resumeMode = npc != null ? npc.CurrentMode : NpcWalkMode.None;
            if (npc != null)
                npc.SetWalkMode(NpcWalkMode.None); // stop wandering; NPC stands still through the delay, then we drive it

            delayTimer = startDelay;
            pending = true;
        }

        private void BeginWalk()
        {
            pending = false;

            if (walkSpeed > 0f)
            {
                cachedSpeed = mover.MoveSpeed;
                speedCached = true;
                mover.MoveSpeed = walkSpeed;
            }

            index = 0;
            walkTimer = 0f;
            walking = true;
        }

        private void Update()
        {
            if (pending)
            {
                delayTimer -= Time.deltaTime;
                if (delayTimer <= 0f)
                    BeginWalk();
                return;
            }

            if (!walking)
                return;

            // Give up so the player is never locked forever if the NPC gets stuck on geometry.
            if (maxWalkSeconds > 0f)
            {
                walkTimer += Time.deltaTime;
                if (walkTimer >= maxWalkSeconds)
                {
                    Arrive();
                    return;
                }
            }

            while (index < waypoints.Count)
            {
                Transform wp = waypoints[index];
                if (wp == null)
                {
                    index++;
                    continue;
                }

                Vector2 toTarget = (Vector2)wp.position - (Vector2)transform.position;
                if (toTarget.magnitude <= arriveDistance)
                {
                    index++;
                    continue;
                }

                mover.MoveDirection = toTarget.normalized;
                return;
            }

            Arrive();
        }

        private void Arrive()
        {
            walking = false;
            pending = false;

            if (mover != null)
                mover.MoveDirection = Vector2.zero;

            ReleaseLock();

            if (destroyOnArrive)
            {
                Destroy(gameObject);
                return;
            }

            if (speedCached && mover != null)
            {
                mover.MoveSpeed = cachedSpeed;
                speedCached = false;
            }
            if (npc != null)
                npc.SetWalkMode(resumeMode); // hand movement back to the NPC's normal behavior
        }

        private void OnDisable()
        {
            walking = false;
            pending = false;
            ReleaseLock(); // never strand the player locked if we're disabled mid-walk or mid-delay
        }

        private void ReleaseLock()
        {
            if (!holdingLock)
                return;
            holdingLock = false;
            PlayerInput.Unlock();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Count == 0)
                return;

            Gizmos.color = new Color(1f, 0.7f, 0.2f, 0.9f);
            Vector3 from = transform.position;
            foreach (Transform wp in waypoints)
            {
                if (wp == null)
                    continue;
                Gizmos.DrawLine(from, wp.position);
                Gizmos.DrawWireSphere(wp.position, arriveDistance);
                from = wp.position;
            }
        }
#endif
    }
}
