using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MyGame
{
    public class OpenOptionController : MonoBehaviour
    {
        private MyGameInputAction _inputAction;
        
        private bool _isBattleEnd;
        
        private bool _isOptionUIOpen;
        
        void Awake()
        {
            _inputAction = new MyGameInputAction();
        }

        void OnEnable()
        {
            _inputAction.asset.Enable();
            _inputAction.UI.Escape.performed += HandleOptionUI;
        }

        void OnDisable()
        {
            _inputAction.UI.Escape.performed -= HandleOptionUI;
            _inputAction.asset.Disable();
        }
        
        void HandleOptionUI(InputAction.CallbackContext ctx)
        {
            if (_isBattleEnd) return;
            
            if (ctx.performed)
            {
                if (OptionManager.Instance.IsOptionUIOpen)
                {
                    OptionManager.Instance.CloseOptionUI();
                }
                else
                {
                    OptionManager.Instance.OpenOptionUI();
                }
            }
        }

        public void SetBattleEnd(bool IsBattleEnd)
        {
            _isBattleEnd = IsBattleEnd;
        }
    }
}

