using UnityEngine;

namespace MyGame
{
    // 캐릭터들의 행동을 열거형으로 정리
    public enum FighterActionID
    {
        Idle,
        Forward,
        Backward,
    }
    
    // 
    public class Fighter
    {
        private Vector2 _position;
        public Vector2 Position => _position;
        public FighterActionID CurrentActionID { get; private set; } // 현재 액션

        private bool _isFaceRight = true;
        public bool IsFaceRight => _isFaceRight;
        
        private FighterData _fighterData;

        public void BattleSetup(FighterData fighterData, Vector2 position, bool isFaceRight)
        {
            _fighterData = fighterData;
            _position = position;
            _isFaceRight = isFaceRight;
        }

        public void UpdateInput(InputData input)
        {
            bool isForward;
            bool isBackward;

            if (_isFaceRight)
            {
                isForward = input.MoveX > 0;
                isBackward = input.MoveX < 0;
            }
            else
            {
                isForward = input.MoveX < 0;
                isBackward = input.MoveX > 0;
            }

            if (isForward)
            {
                CurrentActionID = FighterActionID.Forward;
            }
            else if (isBackward)
            {
                CurrentActionID = FighterActionID.Backward;
            }
            else
            {
                CurrentActionID = FighterActionID.Idle;
            }
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
    }
}


