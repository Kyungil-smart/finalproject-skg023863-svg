using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MyGame
{
    public class BattleSceneOptionController: MonoBehaviour
    {
        private MyGameInputAction _inputAction;

        void Awake()
        {
            _inputAction = new MyGameInputAction();
        }

        void OnEnable()
        {
            _inputAction.asset.Enable();
            _inputAction.UI.Escape.performed += OpenOptionUI;
        }

        void OnDisable()
        {
            _inputAction.UI.Escape.performed -= OpenOptionUI;
            _inputAction.asset.Disable();
        }
        
        void OpenOptionUI(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                OptionManager.Instance.OpenOptionUI();
            }
        }
    }
}

