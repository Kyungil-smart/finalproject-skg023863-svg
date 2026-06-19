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
            _animator = GetComponentInChildren<Animator>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    
        public void Initialize(Fighter fighter)
        {
            _fighter = fighter;    
        }

        void FixedUpdate()
        {
            ShakeSprite();
            PlayFighterAnimation();
            UpdateFighterPosition();
        }
        
        void PlayFighterAnimation()
        {
            if (_fighter == null) return;
            
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
            if (_fighter == null) return;

            transform.position = _fighter.Position;
            _spriteRenderer.flipX = !_fighter.IsFaceRight;
        }

        void ShakeSprite()
        {
            _spriteRenderer.transform.localPosition = new Vector3((float)_fighter.ShakeSpritePower / 6, 0, 0);
        }
    }

}
