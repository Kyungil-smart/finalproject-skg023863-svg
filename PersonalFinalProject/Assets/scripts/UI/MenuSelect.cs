using UnityEngine;
using UnityEngine.EventSystems;

namespace MyGame
{
    // 메뉴 항목의 선택 처리와 선택된 항목의 위치로 커서 이동을 요청하는 기능을 담당
    public class MenuSelect : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        [SerializeField] private RectTransform _cursorPoint;
        [SerializeField] CursorControllor _cursorControllor;

        public void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnSelect(BaseEventData eventData)
        {
            _cursorControllor.MoveCursor(_cursorPoint);
        }
    }
}

