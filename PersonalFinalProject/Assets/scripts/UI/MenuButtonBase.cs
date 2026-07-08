using UnityEngine;
using UnityEngine.EventSystems;

namespace MyGame
{
    public class MenuButtonBase : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}

