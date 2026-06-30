using System;
using UnityEngine;

namespace MyGame
{
    // 게임 화면에서 플레이어의 애니메이션 등을 출력하는 역할
    public class FighterView : MonoBehaviour
    {
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        [SerializeField]private float AnimationSampleOffset = 0.05f;
        
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
            //if (gameObject.name == "Player2") return;
            
            if (_fighter == null) return;
            
            _animator.speed = 0;
            
            //string actionName = _fighter.CurrentActionName;

            string actionName = _fighter.CurrentActionName;
            
            float currentFrame = (float)_fighter.CurrentActionFrame;
            
            float fullFrame;
            
            if (_fighter.IsDamaged || _fighter.IsGuarded)
            {
                fullFrame = _fighter.HitStunFrame - 1f;
                // Debug.Log(_fighter.HitStunFrame);
            }
            else
            {
                fullFrame = (float)_fighter.FighterData.ActionDatas[_fighter.CurrentActionID].frameCount - 1f;
            }
            
            float normalizedTime = (currentFrame + AnimationSampleOffset )/ fullFrame;
            
            // Debug.Log($"[FighterView] normalizedTime {normalizedTime}");
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
            if (_fighter == null) return;
            
            _spriteRenderer.transform.localPosition = new Vector3((float)_fighter.ShakeSpritePower / 8, 0, 0);
        }
    }

}
