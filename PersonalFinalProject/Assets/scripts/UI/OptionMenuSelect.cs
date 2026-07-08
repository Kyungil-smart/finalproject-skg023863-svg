using UnityEngine;
using UnityEngine.EventSystems;

namespace MyGame
{
    public class OptionMenuSelect : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        [SerializeField] private RectTransform _cursorPoint;
        [SerializeField] private OptionMenuController _menu;

        public void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnSelect(BaseEventData eventData)
        {
            _menu.MoveCursor(_cursorPoint);
        }
    }
}

