using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    public class SceneEntrance : MonoBehaviour
    {
        [Tooltip("Id an exit refers to when it wants the player to appear here.")]
        [SerializeField] private string id;

        [Tooltip("Which way the player faces when arriving here.")]
        [SerializeField] private Facing4 facing = Facing4.Down;

        public string Id => id;

        public Facing4 Facing => facing;

#if UNITY_EDITOR
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
