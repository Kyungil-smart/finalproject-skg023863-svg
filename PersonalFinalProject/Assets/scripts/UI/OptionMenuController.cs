using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MyGame
{
    public class OptionMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject _titleOption;
        
        [SerializeField] private Button _masterVolumeButton;
        [SerializeField] private Button _BgmVolumeButton;
        [SerializeField] private Button _SeVolumeButton;
        
        [SerializeField] private RectTransform _cursor;

        public void MoveCursor(RectTransform _cursorPoint)
        {
            _cursor.position = _cursorPoint.position;
        }
        
        public void MasterVolumeUp()
        {
            _masterVolumeButton.Select();
        }

        public void MasterVolumeDown()
        {
            _masterVolumeButton.Select();
        }

        public void BgmVolumeUp()
        {
            _BgmVolumeButton.Select();
        }

        public void BgmVolumeDown()
        {
            _BgmVolumeButton.Select();
        }

        public void SeVolumeUp()
        {
            _SeVolumeButton.Select();
        }

        public void SeVolumeDown()
        {
            _SeVolumeButton.Select();
        }
        
        public void QuitOptionUI()
        {
            gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(_titleOption);
        }
    }
}

