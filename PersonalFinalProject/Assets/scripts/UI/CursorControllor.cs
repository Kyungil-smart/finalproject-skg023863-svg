using UnityEngine;

namespace MyGame
{
    // 메뉴 항목이 선택되었을 때 해당 항목의 위치로 커서를 이동시키는 기능을 담당.
    // 추상클래스로 커서 기능이 필요한 UI가 상속받을 수 있도록 구현함. 
    public abstract class CursorControllor : MonoBehaviour 
    {
        [SerializeField] private GameObject _cursor;

        public void MoveCursor(RectTransform cursorPoint)
        {
            _cursor.transform.position = cursorPoint.position;
        }
    }
}

