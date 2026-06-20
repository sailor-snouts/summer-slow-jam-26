using UnityEngine;

namespace Game
{
    /// <summary>Something the player can interact with — talk to, open, pick up, etc.</summary>
    public interface IInteractable
    {
        /// <summary>Run the interaction, initiated by <paramref name="initiator"/> (usually the player).</summary>
        void Interact(Transform initiator);
    }
}
