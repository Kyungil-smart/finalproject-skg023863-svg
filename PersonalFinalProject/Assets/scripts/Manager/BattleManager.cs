using System.Collections.Generic;
using System;
using UnityEngine;

namespace MyGame
{
    public enum BattleState
    {
        Intro,
        Battle,
        KO,
        End,
    }
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] private InputController _inputController;
        [SerializeField] private GameObject _player1;
        [SerializeField] private GameObject _player2;
        [SerializeField] private FighterData[] _fighterDataList;
        [SerializeField] private GameObject _battleUIEffectGameObject;
        [SerializeField] private GameObject _ResultUIGameObject;

        private float _mapMaxX;
        private float _mapMinX;

        [SerializeField] private int _playerUpLayer;
        [SerializeField] private int _playerDownLayer;

        private Fighter _fighter1;
        public Fighter Fighter1 => _fighter1;
        
        private Fighter _fighter2;
        public Fighter Fighter2 => _fighter2;

        public List<Fighter> fighters = new();

        private FighterView _fighter1View;
        private FighterView _fighter2View;
        
        private BattleUIEffect _battleUIEffect;
        
        private ResultUIControllor _resultUI;

        private BattleSceneOptionController _battleSceneOptionController;
        
        private BattleState _battleState = BattleState.Intro;

        public int maxRoundWin;
        public int Fighter1RoundWinCount { get; private set; }
        public int Fighter2RoundWinCount { get; private set; }

        private float _timer;
        private float _koTimer;

        private int _roundCount;
        private bool _isFinalRound;

        private bool _isWinnerPlayerOne;
        
        [SerializeField]private float _introStateTime;
        [SerializeField]private float _koStateTime;
        [SerializeField]private float _endStateTime;
        [SerializeField]private float _waitKoTime;

        public event Action OnFightStateIntro; // Intro 상태가 됐을 때 실행시킬 이벤트
        public event Action OnSetWinMarker; // 라운드에서 승리했을 시 승리 표시를 활성화하는 이벤트
        public event Action OnResetBattle;  // 게임을 다시 리셋할 때 실행시킬 이벤트.
                                            // BattleManager 외부에서 리셋을 진행할 때 필요한 메서드들 구독.
        
        void Awake()
        {
            _mapMaxX = GameManager.Instance.MapMaxX;
            _mapMinX = GameManager.Instance.MapMinX;

            _roundCount = 1;
            _isFinalRound = false;
            
            _battleUIEffect = _battleUIEffectGameObject.GetComponent<BattleUIEffect>();
            _resultUI = _ResultUIGameObject.GetComponent<ResultUIControllor>();
            _battleSceneOptionController = GetComponent<BattleSceneOptionController>();
            
            _timer = _introStateTime;
            
            _fighterDataList[0].DictionaryInit();
            
            _fighter1View = _player1.GetComponent<FighterView>();
            _fighter2View = _player2.GetComponent<FighterView>();
        
            _fighter1 = new Fighter();
            _fighter2 = new Fighter();
            
            fighters.Add(_fighter1);
            fighters.Add(_fighter2);
            
            
            _fighter1View.Initialize(_fighter1);
            _fighter2View.Initialize(_fighter2); 
            _fighter1.BattleSetup(_fighterDataList[0], new Vector2(-2, 0), true);
            _fighter2.BattleSetup(_fighterDataList[0], new Vector2(2, 0), false);
        }

        void Start()
        {
            _battleUIEffect.PlayReady(_roundCount);
        }

        void OnEnable()
        {
            _resultUI.OnPlayAgainClicked += ResetBattle;
        }

        void OnDisabe()
        {
            _resultUI.OnPlayAgainClicked -= ResetBattle;
        }
        
        private void FixedUpdate()
        {
            switch (_battleState)
            {
                case BattleState.Intro:
                    IntroState();
                    
                    _timer -= Time.deltaTime;
                    if (_timer <= 0) ChangeBattleState(BattleState.Battle);
                    break;
                
                case BattleState.Battle:

                    FightState();

                    Fighter deadFighter = fighters.Find(f => f.IsDead);
                    if (deadFighter != null)
                    {
                        ChangeBattleState(BattleState.KO);
                    }
                    
                    break;
                
                case BattleState.KO:
                    
                    _koTimer -= Time.deltaTime;
                    if (_koTimer > 0) return;
                        
                    KoState();
                    
                    _timer -= Time.deltaTime;
                    if(_timer <= 0) ChangeBattleState(BattleState.End);
                    
                    break;
                
                case BattleState.End:

                    EndState();
                    _timer -= Time.deltaTime;

                    if (_timer <= 0)
                    {
                        if (Fighter1RoundWinCount >= maxRoundWin || Fighter2RoundWinCount >= maxRoundWin)
                        {
                            _isWinnerPlayerOne = Fighter1RoundWinCount >= maxRoundWin ?  true : false;
                            OpenResultUI(_isWinnerPlayerOne);
                        }
                        else
                        {
                            ChangeBattleState(BattleState.Intro);
                        }
                    }
                    
                    break;
            }
            
        }

        void ChangeBattleState(BattleState state)
        {
            _battleState = state;
            
            switch (state)
            {
                case BattleState.Intro:
                    _battleUIEffect.HideWinner();
                    
                    if (maxRoundWin - 1 == Fighter1RoundWinCount && maxRoundWin - 1 == Fighter2RoundWinCount)
                    {
                        _isFinalRound = true;
                    }
                    
                    _battleUIEffect.PlayReady(_roundCount, _isFinalRound);
                    
                    OnFightStateIntro?.Invoke();
                    
                    _fighter1.BattleSetup(_fighterDataList[0], new Vector2(-2, 0), true);
                    _fighter2.BattleSetup(_fighterDataList[0], new Vector2(2, 0), false);
                    
                    _timer = _introStateTime;
                    
                    break;
                case BattleState.Battle:
                    
                    break;
                case BattleState.KO:
                    _battleUIEffect.PlayKO();
                    _roundCount++;
                    _timer = _koStateTime;
                    _koTimer = _waitKoTime;
                    
                    _fighter1.ClearInput();
                    _fighter2.ClearInput();
                    
                    break;
                case BattleState.End:
                    _timer = _endStateTime;
                    
                    List<Fighter> deadFighter = fighters.FindAll(f => f.IsDead);
                    if (deadFighter.Count >= 1)
                    {
                        if (deadFighter[0] == _fighter1)
                        {
                            _isWinnerPlayerOne = false;
                            _battleUIEffect.ShowWinner(_isWinnerPlayerOne);
                            Fighter2RoundWinCount++;
                            OnSetWinMarker?.Invoke();
                            _fighter2.RequestWinAction();
                        }
                        else if (deadFighter[0] == _fighter2)
                        {
                            _isWinnerPlayerOne = true;
                            _battleUIEffect.ShowWinner(_isWinnerPlayerOne);
                            Fighter1RoundWinCount++;
                            OnSetWinMarker?.Invoke();
                            _fighter1.RequestWinAction();
                        }
                    }
                    
                    break;
            }
        }
        
        void IntroState()
        {
            _fighter1.UpdateInput(_inputController.GetPlayer1InputData());
            _fighter2.UpdateInput(_inputController.GetPlayer2InputData());
            
            fighters.ForEach(f => f.IncrementActionFrame());
            
            _fighter1.UpdateFacingDirection(_fighter2.Position);
            _fighter2.UpdateFacingDirection(_fighter1.Position);
            
            fighters.ForEach(f => f.UpdateIntroAction());
            fighters.ForEach(f => f.UpdateBoxes());
            
            CheckPushBox();
            CheckHitAndHurtBox();
        }

        void FightState()
        {
            _fighter1.UpdateInput(_inputController.GetPlayer1InputData());
            _fighter2.UpdateInput(_inputController.GetPlayer2InputData());
            
            fighters.ForEach(f => f.IncrementActionFrame());
            
            _fighter1.UpdateFacingDirection(_fighter2.Position);
            _fighter2.UpdateFacingDirection(_fighter1.Position);
            
            fighters.ForEach(f => f.UpdateAction());
            fighters.ForEach(f => f.UpdateMovement());
            fighters.ForEach(f => f.UpdateBoxes());

            CheckPushBox();
            CheckOutMap();
            CheckHitAndHurtBox();
            fighters.ForEach(f => f.UpdateFighterSound());
        }

        void KoState()
        {
            fighters.ForEach(f => f.IncrementActionFrame());
            
            _fighter1.UpdateFacingDirection(_fighter2.Position);
            _fighter2.UpdateFacingDirection(_fighter1.Position);
            
            fighters.ForEach(f => f.UpdateAction());
            fighters.ForEach(f => f.UpdateMovement());
            fighters.ForEach(f => f.UpdateBoxes());

            CheckPushBox();
            CheckOutMap();
            fighters.ForEach(f => f.UpdateFighterSound());
        }

        void EndState()
        {
            fighters.ForEach(f => f.IncrementActionFrame());
            
            _fighter1.UpdateFacingDirection(_fighter2.Position);
            _fighter2.UpdateFacingDirection(_fighter1.Position);
            
            fighters.ForEach(f => f.UpdateAction());
            fighters.ForEach(f => f.UpdateMovement());
            fighters.ForEach(f => f.UpdateFighterSound());
        }
        
        private void CheckHitAndHurtBox()
        {
            foreach (Fighter attacker in fighters)
            {
                bool isHit = false; // 공격 성공시 true로 변환
                bool isPlayer1 = false; // 공격을 성공한 플레이어가 1p인지 2p인지 확인. true면 1p
                int hitAttackID = -1; // 성공한 공격의 AttackID를 임시 저장할 변수
                Vector2 damagePosition = new Vector2(); // 히트 이펙트를 어디서 실행시킬지
                
                foreach (Fighter defender in fighters)
                {
                    if (attacker == defender) continue;

                    foreach (HitBox hitBox in attacker.HitBoxes)
                    {
                        if (!attacker.CanAttackMore(hitBox.attackID)) continue;
                        
                        foreach (HurtBox hurtBox in defender.HurtBoxes)
                        {
                            if (hitBox.BoxOverlap(hurtBox))
                            {
                                isHit = true;
                                if(attacker == _fighter1) isPlayer1 = true;
                                
                                float x1 = Mathf.Min(hitBox.xMax, hurtBox.xMax);
                                float x2 = Mathf.Max(hitBox.xMin, hurtBox.xMin);
                                float y1 = Mathf.Min(hitBox.yMax, hurtBox.yMax);
                                float y2 = Mathf.Max(hitBox.yMin, hurtBox.yMin);
                                damagePosition.x = (x1 + x2) / 2;
                                damagePosition.y = (y1 + y2) / 2;
                                
                                hitAttackID = hitBox.attackID;
                            }
                        }
                        
                        if (isHit) break;
                    }
                    
                    if (isHit) // 히트에 성공했다면
                    {
                        attacker.SuccessfullyAttack(defender.Position);
                        
                        DamageResult damageresult = defender.DamagedAction(attacker.GetAttackData(hitAttackID), attacker.Position);
                        
                        int hitStopFrame = attacker.GetHitStopFrame(damageresult, hitAttackID);
                        int hitStunFrame = attacker.GetHitStunFrame(damageresult, hitAttackID);
                        int shakePower = attacker.GetShakeSpritePower(damageresult, hitAttackID);
                        List<MoveSpeed> moveSpeed = attacker.GetMoveSpeeds(damageresult, hitAttackID);
                        AudioClip audioClip = attacker.GetHitSound(damageresult, hitAttackID);
                        EffectType effectType = attacker.GetEffectType(hitAttackID);
                        
                        EffectManager.Instance.PlayEffect(effectType, damageresult, damagePosition, isPlayer1);
                        
                        if (isPlayer1)
                        {
                            _fighter1View.UpdateLayer(_playerUpLayer);
                            _fighter2View.UpdateLayer(_playerDownLayer);
                        }
                        else
                        {
                            _fighter1View.UpdateLayer(_playerDownLayer);
                            _fighter2View.UpdateLayer(_playerUpLayer);
                        }
                        
                        defender.SetShakeSpritePower(shakePower);
                        defender.SetHitStunFrame(hitStunFrame);
                        defender.SetMoveSpeeds(moveSpeed);
                        defender.SetHitsound(audioClip);
                        defender.SetHitStopFrame(hitStopFrame);
                        
                        attacker.SetHitStopFrame(hitStopFrame);
                    }
                }
            }
            
        }
        
        private void CheckPushBox()
        {
            if (_fighter1.IsIgnorePushBox)
            {
                _fighter1View.UpdateLayer(_playerDownLayer);
                _fighter2View.UpdateLayer(_playerUpLayer);
                return;
            }

            if (_fighter2.IsIgnorePushBox)
            {
                _fighter1View.UpdateLayer(_playerUpLayer);
                _fighter2View.UpdateLayer(_playerDownLayer);
                return;
            }

            if (!_fighter1.PushBox.BoxOverlap(_fighter2.PushBox))
                return;

            if (_fighter1.Position.x < _fighter2.Position.x)
            {
                float overlap = _fighter1.PushBox.xMax - _fighter2.PushBox.xMin;

                _fighter1.ChangePosition(-overlap * 0.5f, 0);
                _fighter2.ChangePosition( overlap * 0.5f, 0);
            }
            else
            {
                float overlap = _fighter2.PushBox.xMax - _fighter1.PushBox.xMin;

                _fighter1.ChangePosition( overlap * 0.5f, 0);
                _fighter2.ChangePosition(-overlap * 0.5f, 0);
            }
        }

        private void CheckOutMap()
        {
            if (_fighter1.WallPushBox == null || _fighter2.WallPushBox == null) return;
            
            fighters.ForEach(f =>
            {
                if (f.WallPushBox.xMin < _mapMinX)
                {
                    f.ChangePosition(_mapMinX - f.WallPushBox.xMin, 0);
                }
                else if (f.WallPushBox.xMax > _mapMaxX)
                {
                    f.ChangePosition(_mapMaxX - f.WallPushBox.xMax, 0);
                }
            });
        }

        private void OpenResultUI(bool isFinalWinnerPlayerOne)
        {
            _battleSceneOptionController.SetBattleEnd(true);
            _ResultUIGameObject.SetActive(true);
            _resultUI.OpenResultUI(isFinalWinnerPlayerOne);
        }

        private void ResetBattle()
        {
            OnResetBattle?.Invoke();
            
            Fighter1RoundWinCount = 0;
            Fighter2RoundWinCount = 0;
            _roundCount = 1;
            _isFinalRound = false;
            _battleSceneOptionController.SetBattleEnd(false);
            Time.timeScale = 1;

            ChangeBattleState(BattleState.Intro);
        }
    }
    
}
