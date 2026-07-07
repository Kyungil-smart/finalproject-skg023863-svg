using UnityEngine;
using UnityEngine.EventSystems;

namespace MyGame
{
    
    public class TitleMenuController : MonoBehaviour
    {
        private GameObject _lastSelected;

        void Update()
        { 
            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current != null)
            {
                _lastSelected = current;
            }
            else if (_lastSelected != null)
            {
                EventSystem.current.SetSelectedGameObject(_lastSelected);
            }
        }
    }
    
}
