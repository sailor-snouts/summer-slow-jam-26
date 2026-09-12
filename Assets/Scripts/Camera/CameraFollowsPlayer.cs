using Unity.Cinemachine;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraFollowsPlayer : MonoBehaviour
    {
        private void OnEnable()
        {
            var cam = GetComponent<CinemachineCamera>();

            // Fall back to a scene search in case the player's own OnEnable hasn't run yet.
            PlayerCharacter player = PlayerCharacter.Current != null
                ? PlayerCharacter.Current
                : FindAnyObjectByType<PlayerCharacter>();

            if (player != null)
                cam.Follow = player.transform;
            else
                Debug.LogWarning("[CameraFollowsPlayer] No PlayerCharacter in the scene to follow.", this);
        }
    }
}
