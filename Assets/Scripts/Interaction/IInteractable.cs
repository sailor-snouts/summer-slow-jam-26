using UnityEngine;

namespace Game
{
    public interface IInteractable
    {
        void Interact(Transform initiator);

        // Verb shown in the control hint, e.g. "talk" or "interact".
        string InteractVerb { get; }
    }
}
