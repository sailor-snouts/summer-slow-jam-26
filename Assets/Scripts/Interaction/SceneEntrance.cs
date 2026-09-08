using UnityEngine;

namespace Game
{
    /// <summary>
    /// A named spawn point in a scene. When the player arrives from a <see cref="SceneExitTrigger"/>
    /// that targeted this <see cref="Id"/>, the player is moved here and faces <see cref="Facing"/>.
    /// Put an empty GameObject with this component at each doorway/entry. Ids are matched by string, so
    /// keep them consistent with the exits that point here (e.g. "FromBar", "FrontDoor").
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneEntrance : MonoBehaviour
    {
        [Tooltip("Id an exit refers to when it wants the player to appear here.")]
        [SerializeField] private string id;

        [Tooltip("Which way the player faces when arriving here.")]
        [SerializeField] private Facing4 facing = Facing4.Down;

        /// <summary>The id exits match against to land the player here.</summary>
        public string Id => id;

        /// <summary>The direction the player faces on arrival.</summary>
        public Facing4 Facing => facing;

#if UNITY_EDITOR
        // Draw a marker with a stub pointing the arrival facing, so entrances are easy to place/read.
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.4f, 1f, 0.5f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, 0.3f);

            Vector3 dir = facing switch
            {
                Facing4.Up => Vector3.up,
                Facing4.Left => Vector3.left,
                Facing4.Right => Vector3.right,
                _ => Vector3.down,
            };
            Gizmos.DrawLine(transform.position, transform.position + dir * 0.6f);
        }
#endif
    }
}
