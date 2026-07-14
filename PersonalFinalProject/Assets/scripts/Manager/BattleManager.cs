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

         private float _mapMaxX;
         private float _mapMinX;

        private Fighter _fighter1;
        public Fighter Fighter1 => _fighter1;
        
        private Fighter _fighter2;
        public Fighter Fighter2 => _fighter2;

        public List<Fighter> fighters = new();

        private FighterView _fighter1View;
        private FighterView _fighter2View;
        
        private BattleState _battleState = BattleState.Intro;

        public int maxRoundWin;
        public int Fighter1RoundWinCount { get; private set; }
        public int Fighter2RoundWinCount { get; private set; }

        private float _timer;

        [SerializeField]private float _introStateTime;
        [SerializeField]private float _koStateTime;
        [SerializeField]private float _endStateTime;
        [SerializeField]private float _waitKoTime;

        public event Action OnResetGuardBraekGauge;
        public event Action OnSetWinMarker;

        public event Action OnResetBattle;
        
        void Awake()
        {
        _mapMaxX = GameManager.Instance.MapMaxX;
        _mapMinX = GameManager.Instance.MapMinX;
            
            
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
                    
                    _waitKoTime -= Time.deltaTime;
                    if (_waitKoTime > 0) return;
                        
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
                            Fighter1RoundWinCount = 0;
                            Fighter2RoundWinCount = 0;
                            OnResetBattle?.Invoke();
                        }
                        
                        ChangeBattleState(BattleState.Intro);
                    }
                    
                    break;
            }
            
        }

        void ChangeBattleState(BattleState state)
        {
            Debug.Log($"지금 배틀 스테이트는 {state}");
            _battleState = state;
            
            switch (state)
            {
                case BattleState.Intro:
                    
                    OnResetGuardBraekGauge?.Invoke();
                    
                    _fighter1.BattleSetup(_fighterDataList[0], new Vector2(-2, 0), true);
                    _fighter2.BattleSetup(_fighterDataList[0], new Vector2(2, 0), false);
                    
                    _fighter1.ClearInput();
                    _fighter2.ClearInput();
                    
                    _timer = _introStateTime;
                    
                    break;
                case BattleState.Battle:
                    
                    break;
                case BattleState.KO:
                    _timer = _koStateTime;
                    _waitKoTime = _koStateTime;
                    
                    break;
                case BattleState.End:
                    _timer = _endStateTime;
                    
                    List<Fighter> deadFighter = fighters.FindAll(f => f.IsDead);
                    if (deadFighter.Count >= 1)
                    {
                        if (deadFighter[0] == _fighter1)
                        {
                            Fighter2RoundWinCount++;
                            OnSetWinMarker?.Invoke();
                            _fighter2.RequestWinAction();
                        }
                        else if (deadFighter[0] == _fighter2)
                        {
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
            
            _fighter1.UpdateFacingDirection(_fighter2);
            _fighter2.UpdateFacingDirection(_fighter1);
            
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
            
            _fighter1.UpdateFacingDirection(_fighter2);
            _fighter2.UpdateFacingDirection(_fighter1);
            
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
            
            _fighter1.UpdateFacingDirection(_fighter2);
            _fighter2.UpdateFacingDirection(_fighter1);
            
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
            
            _fighter1.UpdateFacingDirection(_fighter2);
            _fighter2.UpdateFacingDirection(_fighter1);
            
            fighters.ForEach(f => f.UpdateAction());
            fighters.ForEach(f => f.UpdateMovement());
            fighters.ForEach(f => f.UpdateFighterSound());
        }
        
        private void CheckHitAndHurtBox()
        {
            foreach (Fighter attacker in fighters)
            {
                bool isHit = false;
                bool isPlayer1 = false;
                int hitAttackID = -1;
                Vector2 damagePosition = new Vector2();
                
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
                    
                    if (isHit)
                    {
                        attacker.SuccessfullyAttack();
                        
                        DamageResult damageresult = defender.DamagedAction(attacker.GetAttackData(hitAttackID));
                        int hitStopFrame = attacker.GetHitStopFrame(damageresult, hitAttackID);
                        int hitStunFrame = attacker.GetHitStunFrame(damageresult, hitAttackID);
                        int shakePower = attacker.GetShakeSpritePower(damageresult, hitAttackID);
                        List<MoveSpeed> moveSpeed = attacker.GetMoveSpeeds(damageresult, hitAttackID);
                        AudioClip audioClip = attacker.GetHitSound(damageresult, hitAttackID);
                        EffectType effectType = attacker.GetEffectType(hitAttackID);
                        
                        EffectManager.Instance.PlayEffect(effectType, damageresult, damagePosition, isPlayer1);
                        
                        defender.SetShakeSpritePower(shakePower);
                        defender.SetHitStunFrame(hitStunFrame);
                        defender.SetMoveSpeeds(moveSpeed);
                        defender.SetHitsound(audioClip);
                        
                        attacker.SetHitStopFrame(hitStopFrame);
                        defender.SetHitStopFrame(hitStopFrame);
                    }
                }
            }
            
        }
        
        private void CheckPushBox()
        {
            if (_fighter1.PushBox == null || _fighter2.PushBox == null)
                return;

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
            if (_fighter1.PushBox == null || _fighter2.PushBox == null) return;
            
            fighters.ForEach(f =>
            {
                if (f.PushBox.xMin < _mapMinX)
                {
                    f.ChangePosition(_mapMinX - f.PushBox.xMin, 0);
                }
                else if (f.PushBox.xMax > _mapMaxX)
                {
                    f.ChangePosition(_mapMaxX - f.PushBox.xMax, 0);
                }
            });
        }
    }
    
}
