using UnityEngine;

namespace MyGame
{
    public enum FighterActionID
    {
        Idle,
        Forward,
        Backward,
    }
    
    public class Fighter
    {
        private Vector2 _position;
        public FighterActionID CurrentActionID { get; private set; }

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
    }
}


