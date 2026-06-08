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
        
        private FighterData _fighterData;

        public void BattleSetup(FighterData fighterData, Vector2 position)
        {
            _fighterData = fighterData;
            _position = position;
        }

        public void UpdateInput(InputData input)
        {
            if (input.MoveX > 0)
            {
                CurrentActionID = FighterActionID.Forward;
            }
            else if (input.MoveX < 0)
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
            switch(CurrentActionID)
            {
                case FighterActionID.Forward:
                    _position.x += _fighterData.forwardSpeed * Time.fixedDeltaTime;
                    break;

                case FighterActionID.Backward:
                    _position.x -= _fighterData.backwardSpeed * Time.fixedDeltaTime;
                    break;
            }
        }
    }
}


