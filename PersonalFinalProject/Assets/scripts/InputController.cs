using UnityEngine;
using UnityEngine.InputSystem;

namespace MyGame
{
    // 게임 내의 입력 담당
    public class InputController : MonoBehaviour
    {
        private MyGameInputAction _inputAction; // New InputSystem
        private InputData _player1InputData; // 입력에 따른 1p의 입력정보
        private InputData _player2InputData; // 입력에 따른 2p의 입력정보
        
        public InputData Player1InputData => _player1InputData;
        public InputData Player2InputData => _player2InputData;

        void Awake()
        {
            _inputAction = new MyGameInputAction();
        }

        void OnEnable()
        {
            EnableInit();
        }

        void OnDisable()
        {
            DisableInit();
        }

        // // 1p의 공격 입력
        // void OnPlayer1Attack(InputAction.CallbackContext ctx)
        // {
        //     _player1InputData.Attack = ctx.ReadValueAsButton();
        // }
        //
        // // 1p의 이동 입력
        // void OnPlayer1Move(InputAction.CallbackContext ctx)
        // {
        //     Vector2 inputVector = ctx.ReadValue<Vector2>();
        //     _player1InputData.MoveX = inputVector.x;
        // }
        //
        // // 2p의 공격 입력
        // void OnPlayer2Attack(InputAction.CallbackContext ctx)
        // {
        //     _player2InputData.Attack = ctx.ReadValueAsButton();
        // }
        //
        // // 2p의 이동 입력
        // void OnPlayer2Move(InputAction.CallbackContext ctx)
        // {
        //     Vector2 inputVector = ctx.ReadValue<Vector2>();
        //     _player2InputData.MoveX = inputVector.x;
        // }

        public InputData GetPlayer1InputData()
        {
            InputData input = new InputData();

            Vector2 move = _inputAction.PlayerAction.P1Move.ReadValue<Vector2>();

            if (move.x < 0)
            {
                input.Input |= (int)InputDefine.Left;
            }

            if (move.x > 0)
            {
                input.Input |= (int)InputDefine.Right;
            }

            if (_inputAction.PlayerAction.P1Attack.IsPressed())
            {
                input.Input |= (int)InputDefine.Attack;
            }

            return input;
        }
        
        public InputData GetPlayer2InputData()
        {
            InputData input = new InputData();

            Vector2 move = _inputAction.PlayerAction.P2Move.ReadValue<Vector2>();

            if (move.x < 0)
            {
                input.Input |= (int)InputDefine.Left;
            }

            if (move.x > 0)
            {
                input.Input |= (int)InputDefine.Right;
            }

            if (_inputAction.PlayerAction.P2Attack.IsPressed())
            {
                input.Input |= (int)InputDefine.Attack;
            }

            return input;
        }

        void EnableInit()
        {
            _inputAction.asset.Enable();
            
            // _inputAction.PlayerAction.P1Attack.started += OnPlayer1Attack;
            // _inputAction.PlayerAction.P1Attack.canceled += OnPlayer1Attack;
            // _inputAction.PlayerAction.P1Move.performed += OnPlayer1Move;
            // _inputAction.PlayerAction.P1Move.canceled += OnPlayer1Move;
            //
            // _inputAction.PlayerAction.P2Attack.started += OnPlayer2Attack;
            // _inputAction.PlayerAction.P2Attack.canceled += OnPlayer2Attack;
            // _inputAction.PlayerAction.P2Move.performed += OnPlayer2Move;
            // _inputAction.PlayerAction.P2Move.canceled += OnPlayer2Move;
        }

        void DisableInit()
        {
            // _inputAction.PlayerAction.P1Attack.started -= OnPlayer1Attack;
            // _inputAction.PlayerAction.P1Attack.canceled -= OnPlayer1Attack;
            // _inputAction.PlayerAction.P1Move.performed -= OnPlayer1Move;
            // _inputAction.PlayerAction.P1Move.canceled -= OnPlayer1Move;
            //
            // _inputAction.PlayerAction.P2Attack.started -= OnPlayer2Attack;
            // _inputAction.PlayerAction.P2Attack.canceled -= OnPlayer2Attack;
            // _inputAction.PlayerAction.P2Move.performed -= OnPlayer2Move;
            // _inputAction.PlayerAction.P2Move.canceled -= OnPlayer2Move;
            
            _inputAction.asset.Disable();
        }
    }
}

