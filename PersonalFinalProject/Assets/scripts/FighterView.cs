using System;
using UnityEngine;

namespace MyGame
{
    // 게임 화면에서 플레이어의 애니메이션 등을 출력하는 역할
    public class FighterView : MonoBehaviour
    {
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private Fighter _fighter;
        public Fighter Fighter => _fighter;
        
        void Awake()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
    
        public void Initialize(Fighter fighter)
        {
            _fighter = fighter;    
        }

        // void Update()
        // {
        //     if (gameObject.name == "Player2") return;
        //     Debug.Log((FighterActionID)_fighter.CurrentActionID);
        //     PlayAnimation();
        //     UpdateFighterPosition();
        // }

        void FixedUpdate()
        {
            PlayAnimation();
            UpdateFighterPosition();
        }
        
        void PlayAnimation()
        {
            if (_fighter == null) return;
            
            // Fighter의 CurrentActionID에 따라 출력할 애니메이션을 변경
            // Debug.Log($"현재 액션 : {_fighter.CurrentActionID}");
            // if (_fighter.isHitStopEnd)
            //     _animator.speed = 1;
            // else
            //     _animator.speed = 0;
            
            
            _animator.speed = 0;
            string actionName =
                ((FighterActionID)_fighter.CurrentActionID)
                .ToString();
            
            float currentFrame = (float)_fighter.CurrentActionFrame;
            
            float fullFrame =
                (float)_fighter.FighterData.ActionDatas[_fighter.CurrentActionID].frameCount - 1f;
            
            float normalizedTime = currentFrame / fullFrame;
            
            _animator.Play(actionName, 0, normalizedTime);
        }

        void UpdateFighterPosition()
        {
            if (_fighter == null)
                return;

            transform.position = _fighter.Position;
            _spriteRenderer.flipX = !_fighter.IsFaceRight;
        }
    }

}
