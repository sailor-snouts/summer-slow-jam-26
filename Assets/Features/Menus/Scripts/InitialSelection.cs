using UnityEngine;
using UnityEngine.EventSystems;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Selects this GameObject once the scene starts, so keyboard and controller
    /// navigation has a starting point. Put it on the first menu button.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Initial Selection")]
    public class InitialSelection : MonoBehaviour
    {
        private void Start()
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
