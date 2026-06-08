using UnityEngine;

namespace MyGame
{
    // 게임 화면에서 플레이어의 애니메이션 등을 출력하는 역할
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
            PlayAnimation();
            UpdateFighterPosition();
        }

        void PlayAnimation()
        {
            if (_fighter == null)
                return;

            // Fighter의 CurrentActionID에 따라 출력할 애니메이션을 변경
            _animator.SetInteger("ActionID", (int)_fighter.CurrentActionID);
        }

        void UpdateFighterPosition()
        {
            if (_fighter == null)
                return;

            transform.position = _fighter.Position;
        }
    }

}
