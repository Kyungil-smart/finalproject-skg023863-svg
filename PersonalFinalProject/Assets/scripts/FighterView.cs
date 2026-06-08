using UnityEngine;

namespace MyGame
{
    public class FighterView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private Fighter _fighter;
    
        public void Initialize(Fighter fighter)
        {
            _fighter = fighter;    
        }

        void Update()
        {
            if (_fighter == null)
                return;

            _animator.SetInteger(
                "ActionID",
                (int)_fighter.CurrentActionID);
        }
    }

}
