using System;
using UnityEngine;

namespace JamTemplate.Saving
{
    /// <summary>
    /// Saves and restores this object's position and rotation. Doubles as the
    /// reference example for the save pattern: subscribe to
    /// <see cref="SaveManager.Saving"/>/<see cref="SaveManager.Loading"/> in
    /// OnEnable/OnDisable, write your own state under a unique <see cref="key"/> on
    /// save, and read it back on load. Copy this shape for any object that needs to
    /// persist — inventory, score, unlocked levels, etc.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Transform Save Participant")]
    [DisallowMultipleComponent]
    public class TransformSaveParticipant : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Unique key this object's data is stored under. Must differ from every other participant.")]
        private string key = "object";

        [Serializable]
        private struct State
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        private void OnEnable()
        {
            if (SaveManager.Instance == null)
                return;

            SaveManager.Instance.Saving += OnSaving;
            SaveManager.Instance.Loading += OnLoading;
        }

        private void OnDisable()
        {
            if (SaveManager.Instance == null)
                return;

            SaveManager.Instance.Saving -= OnSaving;
            SaveManager.Instance.Loading -= OnLoading;
        }

        private void OnSaving(SaveData data)
        {
            data.Set(key, new State { position = transform.position, rotation = transform.rotation });
        }

        private void OnLoading(SaveData data)
        {
            if (data.TryGet(key, out State state))
            {
                transform.SetPositionAndRotation(state.position, state.rotation);
            }
        }
    }
}
