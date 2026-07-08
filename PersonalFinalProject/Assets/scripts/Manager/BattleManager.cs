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

        [SerializeField] private float _mapMaxX;
        [SerializeField] private float _mapMinX;

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

        public event Action OnResetGuardBraekGauge;
        public event Action OnSetWinMarker;

        public event Action OnResetBattle;
        
        void Awake()
        {
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
                            Debug.Log($"플레이어2 승 : 승리 점수 {Fighter2RoundWinCount}" );
                            _fighter2.RequestWinAction();
                        }
                        else if (deadFighter[0] == _fighter2)
                        {
                            Fighter1RoundWinCount++;
                            OnSetWinMarker?.Invoke();
                            Debug.Log($"플레이어2 승 : 승리 점수 {Fighter1RoundWinCount}" );
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
        }

        void KoState()
        {
            
        }

        void EndState()
        {
            fighters.ForEach(f => f.IncrementActionFrame());
            
            _fighter1.UpdateFacingDirection(_fighter2);
            _fighter2.UpdateFacingDirection(_fighter1);
            
            fighters.ForEach(f => f.UpdateAction());
            fighters.ForEach(f => f.UpdateMovement());
            fighters.ForEach(f => f.UpdateBoxes());

            CheckPushBox();
            CheckOutMap();
            CheckHitAndHurtBox();
        }
        
        private void CheckHitAndHurtBox()
        {
            foreach (Fighter attacker in fighters)
            {
                bool isHit = false;
                int hitAttackID = -1;
                
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
                        
                        defender.SetShakeSpritePower(shakePower);
                        defender.SetHitStunFrame(hitStunFrame);
                        defender.SetMoveSpeeds(moveSpeed);
                        
                        attacker.SetHitStopFrame(hitStopFrame);
                        defender.SetHitStopFrame(hitStopFrame);
                    }
                }
            }
            
        }

        private void CheckPushBox()
        {
            if (_fighter1.PushBox == null || _fighter2.PushBox == null) return;
            
            Rect rect1 = _fighter1.PushBox.rect;
            Rect rect2 = _fighter2.PushBox.rect;

            if (rect1.Overlaps(rect2))
            {
                if (_fighter1.Position.x < _fighter2.Position.x)
                {
                    _fighter1.ChangePosition((rect1.xMax - rect2.xMin) * -1 / 2 ,_fighter1.Position.y);
                    _fighter2.ChangePosition((rect1.xMax - rect2.xMin) * 1 / 2, _fighter2.Position.y);
                }
                else if (_fighter1.Position.x > _fighter2.Position.x)
                {
                    _fighter1.ChangePosition((rect2.xMax - rect1.xMin) * 1 / 2 , _fighter1.Position.y);
                    _fighter2.ChangePosition((rect2.xMax - rect1.xMin) * -1 / 2, _fighter2.Position.y);
                }
            }
        }

        private void CheckOutMap()
        {
            if (_fighter1.PushBox == null || _fighter2.PushBox == null) return;
            
            fighters.ForEach(f =>
            {
                if (f.PushBox.xMin < _mapMinX)
                {
                    f.ChangePosition(_mapMinX - f.PushBox.xMin, f.Position.y);
                }
                else if (f.PushBox.xMax > _mapMaxX)
                {
                    f.ChangePosition(_mapMaxX - f.PushBox.xMax, f.Position.y);
                }
            });
        }
    }
    
}
