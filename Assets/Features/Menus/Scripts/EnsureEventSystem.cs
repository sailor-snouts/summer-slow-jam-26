using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Guarantees an EventSystem exists for this scene's UI, creating one only if
    /// none is present. Put it on overlay scenes (loaded additively) that may sit
    /// over a scene without an EventSystem of its own, without risking a second
    /// EventSystem when one already exists.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Ensure Event System")]
    public class EnsureEventSystem : MonoBehaviour
    {
        private void Awake()
        {
            if (EventSystem.current != null)
                return;

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            UIInput.Configure(go.GetComponent<InputSystemUIInputModule>());
        }
    }
}
