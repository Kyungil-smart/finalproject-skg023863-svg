using UnityEngine;

namespace MyGame
{
    public enum EffectType
    {
        normalEffect
    }
    public class EffectManager : MonoBehaviour
    {
        public static EffectManager Instance;
        
        [SerializeField] private GameObject _player1EffectObject;
        [SerializeField] private GameObject _player2EffectObject;
        
        private Animator _player1EffectAnimator;
        private Animator _player2EffectAnimator;
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            
            _player1EffectAnimator = _player1EffectObject.GetComponent<Animator>();
            _player2EffectAnimator = _player2EffectObject.GetComponent<Animator>();
        }

        public void PlayEffect(EffectType effectType, DamageResult damageResult, Vector2 position, bool isPlayer1)
        {
            Transform transform;
            Animator animator;
            if (isPlayer1)
            {
                transform = _player1EffectObject.transform;
                animator = _player1EffectAnimator;
            }
            else
            {
                transform = _player2EffectObject.transform;
                animator = _player2EffectAnimator;
            }
            
            switch (damageResult)
            {
                case DamageResult.Damage:
                case DamageResult.Dead:
                    transform.localScale = new Vector3(2f, 2f, 1f);
                    break;

                case DamageResult.Guard:
                    transform.localScale = new Vector3(1f, 1f, 1f);
                    break;

                case DamageResult.GuradBreak:
                    transform.localScale = new Vector3(4f, 4f, 1f);
                    break;
            }
            
            transform.position = position;
            animator.SetTrigger(effectType.ToString());
        }
    }
}

