using Unity.Cinemachine;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Points this Cinemachine Camera at the player at runtime. A camera-rig prefab can't store a
    /// reference to the player (the player lives in the scene, not in the prefab), so this finds the
    /// PlayerCharacter on enable and sets it as the Tracking (Follow) target - letting the rig be
    /// dropped into any scene with no hand-wiring.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraFollowsPlayer : MonoBehaviour
    {
        private void OnEnable()
        {
            var cam = GetComponent<CinemachineCamera>();

            // Prefer the active player if it has come up already; otherwise find it in the scene
            // (works even if the player's own OnEnable hasn't run yet).
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
