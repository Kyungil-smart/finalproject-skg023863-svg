using UnityEngine;
using UnityEngine.EventSystems;

namespace MyGame
{
    public enum OptionType
    {
        None,
        MasterVolume,
        BgmVolume,
        SeVolume
    }
    
    public class OptionMenuSelect : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        [SerializeField] private RectTransform _cursorPoint;
        [SerializeField] private OptionMenuController _menu;
        [SerializeField] private OptionType _optionType;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnSelect(BaseEventData eventData)
        {
            _menu.MoveCursor(_cursorPoint);
            _menu.SetCursorIndex(_optionType);
        }
    }
}

