using UnityEngine;
using UnityEngine.InputSystem;

namespace MyGame
{
    public class OptionInputController : MonoBehaviour
    {
        [SerializeField] private OptionMenuController _menu;
        private MyGameInputAction _inputAction;
        
        void Awake()
        {
            if (_menu == null) _menu = GetComponent<OptionMenuController>();
            _inputAction = new MyGameInputAction();
        }

        void OnEnable()
        {
            _inputAction.asset.Enable();

            _inputAction.UI.Player1Navigate.performed += ChangeVolume;
            _inputAction.UI.Player2Navigate.performed += ChangeVolume;
        }

        void OnDisable()
        {
            _inputAction.UI.Player1Navigate.performed -= ChangeVolume;
            _inputAction.UI.Player2Navigate.performed -= ChangeVolume;
            
            _inputAction.asset.Disable();
        }

        void ChangeVolume(InputAction.CallbackContext ctx)
        {
            Vector2 inputVector = ctx.ReadValue<Vector2>();

            switch (_menu.cursorIndex)
            {
                case OptionType.MasterVolume:
                    if (inputVector.x > 0) _menu.MasterVolumeUp();
                    else if (inputVector.x < 0) _menu.MasterVolumeDown();
                    break;
                case OptionType.BgmVolume:
                    if (inputVector.x > 0) _menu.BgmVolumeUp();
                    else if (inputVector.x < 0) _menu.BgmVolumeDown();
                    break;
                case OptionType.SeVolume:
                    if (inputVector.x > 0) _menu.SeVolumeUp();
                    else if (inputVector.x < 0) _menu.SeVolumeDown();
                    break;
            }
        }
    }
}

