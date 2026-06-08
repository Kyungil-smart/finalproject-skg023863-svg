using UnityEngine;
using UnityEngine.InputSystem;

namespace MyGame
{
    public class InputController : MonoBehaviour
    {
        private MyGameInputAction _inputAction;
        private InputData _player1InputData;
        private InputData _player2InputData;
        
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

        void OnPlayer1Attack(InputAction.CallbackContext ctx)
        {
            _player1InputData.Attack = ctx.ReadValueAsButton();
        }

        void OnPlayer1Move(InputAction.CallbackContext ctx)
        {
            Vector2 inputVector = ctx.ReadValue<Vector2>();
            _player1InputData.MoveX = inputVector.x;
        }
        
        void OnPlayer2Attack(InputAction.CallbackContext ctx)
        {
            _player2InputData.Attack = ctx.ReadValueAsButton();
        }

        void OnPlayer2Move(InputAction.CallbackContext ctx)
        {
            Vector2 inputVector = ctx.ReadValue<Vector2>();
            _player2InputData.MoveX = inputVector.x;
        }

        void EnableInit()
        {
            _inputAction.asset.Enable();
            
            _inputAction.PlayerAction.P1Attack.started += OnPlayer1Attack;
            _inputAction.PlayerAction.P1Attack.canceled += OnPlayer1Attack;
            _inputAction.PlayerAction.P1Move.performed += OnPlayer1Move;
            _inputAction.PlayerAction.P1Move.canceled += OnPlayer1Move;
            
            _inputAction.PlayerAction.P2Attack.started += OnPlayer2Attack;
            _inputAction.PlayerAction.P2Attack.canceled += OnPlayer2Attack;
            _inputAction.PlayerAction.P2Move.performed += OnPlayer2Move;
            _inputAction.PlayerAction.P2Move.canceled += OnPlayer2Move;
        }

        void DisableInit()
        {
            _inputAction.PlayerAction.P1Attack.started -= OnPlayer1Attack;
            _inputAction.PlayerAction.P1Attack.canceled -= OnPlayer1Attack;
            _inputAction.PlayerAction.P1Move.performed -= OnPlayer1Move;
            _inputAction.PlayerAction.P1Move.canceled -= OnPlayer1Move;
            
            _inputAction.PlayerAction.P2Attack.started -= OnPlayer2Attack;
            _inputAction.PlayerAction.P2Attack.canceled -= OnPlayer2Attack;
            _inputAction.PlayerAction.P2Move.performed -= OnPlayer2Move;
            _inputAction.PlayerAction.P2Move.canceled -= OnPlayer2Move;
            
            _inputAction.asset.Disable();
        }
    }
}

