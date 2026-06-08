using UnityEngine;

namespace MyGame
{
    // 캐릭터들의 행동을 열거형으로 정리
    public enum FighterActionID
    {
        Idle,
        Forward,
        Backward,
        NAttack,
    }
    
    // 
    public class Fighter
    {
        private Vector2 _position;
        public Vector2 Position => _position;
        public FighterActionID CurrentActionID { get; private set; } // 현재 액션

        private bool _isFaceRight = true;
        public bool IsFaceRight => _isFaceRight;
        
        private InputData _currentInput;

        private int _currentActionFrame;
        public int CurrentActionFrame => _currentActionFrame;
        
        private FighterData _fighterData;

        public void BattleSetup(FighterData fighterData, Vector2 position, bool isFaceRight)
        {
            _fighterData = fighterData;
            _position = position;
            _isFaceRight = isFaceRight;
            
            SetCurrentAction(FighterActionID.Idle);
        }

        public void UpdateInput(InputData input)
        {
            _currentInput = input;
        }
        
        public void UpdateMovement()
        {
            float faceDir = _isFaceRight ? 1f : -1f;
            
            switch(CurrentActionID)
            {
                case FighterActionID.Forward:
                    _position.x += _fighterData.forwardSpeed * faceDir * Time.fixedDeltaTime;
                    break;

                case FighterActionID.Backward:
                    _position.x -= _fighterData.backwardSpeed * faceDir * Time.fixedDeltaTime;
                    break;
            }
        }
        
        public void IncrementActionFrame()
        {
            _currentActionFrame++;
        }
        private void RequestAction(FighterActionID actionID)
        {
            if(CurrentActionID == actionID)
                return;

            SetCurrentAction(actionID);
        }

        public void UpdateAction()
        {
            Debug.Log(
                $"{CurrentActionID} / {CurrentActionFrame}");
            
            if(CurrentActionID == FighterActionID.NAttack)
            {
                if(CurrentActionFrame >= 24)
                {
                    RequestAction(FighterActionID.Idle);
                }

                return;
            }

            if(_currentInput.Attack)
            {
                RequestAction(FighterActionID.NAttack);
                return;
            }

            bool isForward;
            bool isBackward;

            if (_isFaceRight)
            {
                isForward = _currentInput.MoveX > 0;
                isBackward = _currentInput.MoveX < 0;
            }
            else
            {
                isForward = _currentInput.MoveX < 0;
                isBackward = _currentInput.MoveX > 0;
            }

            if (isForward)
            {
                RequestAction(FighterActionID.Forward);
            }
            else if (isBackward)
            {
                RequestAction(FighterActionID.Backward);
            }
            else
            {
                RequestAction(FighterActionID.Idle);
            }
        }
        
        private void SetCurrentAction(FighterActionID actionID)
        {
            Debug.Log($"SetCurrentAction : {actionID}");
            
            CurrentActionID = actionID;
            _currentActionFrame = 0;
        }

        public void UpdateFacingDirection(Fighter opponent)
        {
            _isFaceRight = _position.x < opponent.Position.x;
        }
    }
}


