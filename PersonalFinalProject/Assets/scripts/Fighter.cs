using System;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MyGame
{
    // hitbox, hurtbox, pushbox등의 공통 부모 클래스
    // 박스의 위치와 충돌 판정에 필요한 xMin, xMax, yMin, yMax 계산 기능을 제공
    public class BoxBase
    {
        public Rect rect;
        
        // 현재 Fighter들이 사용하는 스프라이트의 pivot은 x = 0.5, y = 0 이므로 Fighter들의 스프라이트는 중앙하단을 기준으로 그려지게 된다.
        // Rect의 x,y는 오브젝트의 pivot을 기준으로 삼고 Rect의 하단좌측을 뜻한다.
        // 박스들을 스프라이트의 기준점에 맞춰서 그리는게 편하므로
        // Rect의 좌표를 그대로 사용하지 않고 마치 pivot x = 0.5, y = 0인 것처럼 사용하기위해 박스 위치를 재정의 해서 박스의 충돌을 비교하는데 사용한다. 
        public float xMin { get { return rect.x - rect.width / 2; }} // 박스의 중심점으로부터 width/2 만큼 왼쪽
        public float xMax { get { return rect.x + rect.width / 2; }} // 박스의 중심점으로부터 width/2 만큼 오른쪽
        public float yMin { get { return rect.y; }}                  // 박스의 바닥 시작 점
        public float yMax { get { return rect.y + rect.height; }}    // 박스의 높이

        // 박스 끼리의 충돌 판정을 하기 위해 사용하는 메서드 opponentBox는 자신 이외의 상대 BoxBase가 들어간다.
        public bool BoxOverlap(BoxBase opponentBox)
        {
            // c = corner
            bool c1 = xMin <= opponentBox.xMax;
            bool c2 = xMax >= opponentBox.xMin;
            bool c3 = yMin <= opponentBox.yMax;
            bool c4 = yMax >= opponentBox.yMin;
            
            return c1 && c2 && c3 && c4;
        }
    }

    // 히트박스. attackID를 받아와서 어떤 공격의 히트박스인지 확인 가능
    public class HitBox : BoxBase
    {
        public int attackID;
    }

    // 허트박스. 히트박스에 닿으면 공격을 받은 것으로 간주 된다.
    public class HurtBox : BoxBase
    {
        
    }
    
    // 푸쉬박스. Fighter끼리 밀어내거나 맵 밖으로 나게가 될 경우를 방지.
    public class PushBox : BoxBase
    {
        
    }

    public class WallPushBox : BoxBase
    {
        
    }

    public static class CommandDataStorage
    {
        public static readonly int[] Command214 = { 2, 1, 4 };
        public static readonly int[] Command623 = { 6, 2, 3 };
        public static readonly int[] Command4 = { 4 };
        public static readonly int[] Command6 = { 6 };
    }
    
    // Fighter의 행동을 열거형으로 정리
    public enum FighterState
    {
        Idle,
        Forward,
        Backward,
        ForwardDash,
        BackwardDash,
        Damaged,
        CrouchGuard,
        StandGuard,
        GuardBreak,
        Win,
        Dead,
    }
    
    // Fighter의 커맨드 입력을 열거형으로 정리, 숫자패드로 커맨드 방향 표기 예) 236 = ↓↘→
    public enum CommandType
    {
        None,
        Command4,
        Command6,
        Command236,
        Command214,
        Command623,
    }

    // Fighter가 공격 받았을 때 어떤 상황인지 열거형으로 정리
    public enum DamageResult
    {
        Damage, 
        Guard,  
        GuradBreak, 
        Dead
    }

    
    
    // 대전에서 사용할 캐릭터(Fighter)의 로직
    public class Fighter
    {
        private Vector2 _position; // Fighter의 위치
        public Vector2 Position => _position;
        
        public event Action<int> OnGuardBreakGaugeChanged;

        private int _healthGage;
        
        public int GuardBreakGauge { get; private set; }

        private bool _isWin;

        public bool IsDead { get; private set; }
        
        public bool IsIgnorePushBox { get; private set; }
        
        public int CurrentActionID { get; private set; } // 현재의 액션 ID를 저장

        private int _executeActionID = -1;

        private int _bufferActionID = -1;

        private int _bufferActionStartFrame = 0;

        private int _reserveActionID = -1;
        
        public string CurrentActionName { get { return _fighterData.ActionDatas[CurrentActionID].actionName; } }

        private bool _isFaceRight = true; // 오른쪽을 바라 보고 있는지 확인하는 bool 변수, true면 오른쪽을 바라보고 있는 것.
        public bool IsFaceRight => _isFaceRight;
        
        // 어느 쪽을 바라보고 있는지에 따라 방향키의 입력이 전진, 후진이 되도록 하기 위해 사용하는 변수
        public int Sign { get { return _isFaceRight ? 1 : -1; } }
        
        private InputData _currentInput; // 현재 입력을 저장
        
        private int _currentActionFrame; // 현재 액션의 몇번째 프레임인지. 0부터 시작함.
        public int CurrentActionFrame => _currentActionFrame;
        
        // 현재 액션이 끝났는지 확인하는 bool 변수.
        // _currentActionFrame이 현재 액션의 총 프레임 수보다 크거나 같던지, HitStunFrame 프레임 수보다 크거나 같으면 true 반환
        public bool IsActionEnd 
        { get 
            { return _currentActionFrame >= _fighterData.ActionDatas[CurrentActionID].frameCount
                && _currentActionFrame >= HitStunFrame; } 
        }
        
        public int LoopStartFrame { get { return _fighterData.ActionDatas[CurrentActionID].loopFromFrame; } }

        private int _currentHitStopFrame; // 현재 공격의 남아있는 히트 스탑 프레임 수, 프레임 마다 -- 됨
        
        public bool IsHitStopEnd { get { return _currentHitStopFrame <= 0; } }

        public int ShakeSpritePower { get; private set; }
        
        public int HitStunFrame { get; private set; }
        
        public bool IsDamaged { get { return _fighterData.ActionDatas[CurrentActionID].actionType == ActionType.Damaged; } }
        public bool IsGuarded { get { return _fighterData.ActionDatas[CurrentActionID].actionType == ActionType.Guard; } }

        // 이 공격 이 이번 액션에서 이미 몇 번 적중했는가 확인용
        // 1히트 공격이 들어갔을 경우 1히트 보다 더 히트되면 안 되므로 비교하기 위해 사용되는 변수
        private int _currentAttackhitCount; 
        
        private List<HitBox> _hitBoxes = new();
        public  List<HitBox> HitBoxes => _hitBoxes;
        
        private List<HurtBox> _hurtBoxes = new();
        public List<HurtBox> HurtBoxes => _hurtBoxes;
        
        private PushBox _pushBox;
        public PushBox PushBox => _pushBox;

        private WallPushBox _wallPushBox;
        public WallPushBox WallPushBox => _wallPushBox;
        
        private List<MoveSpeed> _knockBackMoveSpeeds;

        private int _currentKnockBackFrame;
        
        private FighterData _fighterData;
        
        public FighterData FighterData => _fighterData;

        private static int inputRecordFrame = 15;

        private int[] input = new int[inputRecordFrame];
        private int[] inputDown = new int[inputRecordFrame];
        private int[] inputUp = new int[inputRecordFrame];

        public void BattleSetup(FighterData fighterData, Vector2 position, bool isFaceRight)
        {
            _healthGage = fighterData.healthGauge;
            GuardBreakGauge = fighterData.guardBreakGauge;
            
            _fighterData = fighterData;
            _position = position;
            _isFaceRight = isFaceRight;
            _isWin = false;
            IsDead = false;
            
            SetCurrentAction((int)FighterState.Idle);
        }

        public void UpdateInput(InputData inputData)
        {
            for (int i = input.Length - 1; i >= 1; i--)
            {
                input[i] = input[i - 1];
                inputDown[i] = inputDown[i - 1];
                inputUp[i] = inputUp[i - 1];
            }
            
            // ^(XOR)는 비트 연산자. 비트가 같으면 0 틀리면 1
            input[0] = inputData.Input;
            inputDown[0] = (input[0] ^ input[1]) & input[0];
            inputUp[0] = (input[0] ^ input[1]) & ~input[0];
        }

        public void ClearInput()
        {
            for (int i = 0; i < input.Length; i++)
            {
                input[i] = 0;
                inputDown[i] = 0;
                inputUp[i] = 0;
            }
        }
        
        public void IncrementActionFrame()
        {
            if (Mathf.Abs(ShakeSpritePower) > 0)
            {
                ShakeSpritePower *= -1;
                ShakeSpritePower += (ShakeSpritePower < 0 ? 1 : -1);
            }
            
            if (!IsHitStopEnd)
            {
                _currentHitStopFrame--;
                return;
            }
            
            _currentActionFrame++;
            
        }

        public void UpdateIntroAction()
        {
            RequestAction((int)FighterState.Idle);
        }
        
        public void UpdateAction()
        {
            if (_isWin)
            {
                RequestAction((int)FighterState.Win);
                return;
            }
            
            if (IsDead)
            {
                RequestAction((int)FighterState.Dead);
                return;
            }
            
            if (_reserveActionID != -1 && IsHitStopEnd)
            {
                SetCurrentAction(_reserveActionID);
                _reserveActionID = -1;
                return;
            }

            if (_bufferActionID != -1 && CanCancelAttack() && IsHitStopEnd)
            {
                if (CurrentActionFrame < _bufferActionStartFrame) return;
                Debug.Log($"발동 프레임{CurrentActionFrame}");
                SetCurrentAction(_bufferActionID);
                return;
            }

            if (_executeActionID != -1 && CanCancelAttack() && IsHitStopEnd)
            {
                Debug.Log($"발동 프레임{CurrentActionFrame}");
                SetCurrentAction(_executeActionID);
                return;
            }
            
            bool isForward = IsInputForward(input[0]);
            bool isBackward = IsInputBackward(input[0]);
            bool isAttack = IsInputAttack(inputDown[0]);
            
            if(isAttack)
            {
                TryCommand();
                return;
            }

            if (CheckForwardDash())
            {
                RequestAction((int)FighterState.ForwardDash);
                
            }
            else if (CheckBackwardDash())
            {
                RequestAction((int)FighterState.BackwardDash);
                
            }
            else if (isForward)
                RequestAction((int)FighterState.Forward);
            
            else if (isBackward)
                RequestAction((int)FighterState.Backward);
            
            else
                RequestAction((int)FighterState.Idle);
            
        }
        
        public void UpdateMovement()
        {
            if (!IsHitStopEnd) return;
            
            if (CurrentActionID == (int)FighterState.Forward)
            {
                _position.x += _fighterData.forwardSpeed * Sign * Time.fixedDeltaTime;
                return;
            }
            if (CurrentActionID == (int)FighterState.Backward)
            {
                _position.x -= _fighterData.backwardSpeed * Sign * Time.fixedDeltaTime;
                return;
            }

            MoveSpeed moveSpeed;

            if (CurrentActionID == (int)FighterState.GuardBreak || IsDead)
            {
                moveSpeed = _fighterData.ActionDatas[CurrentActionID].GetMoveSpeed(CurrentActionFrame);
            
                if (moveSpeed != null)
                {
                    _position.x += moveSpeed.speed * Sign * Time.fixedDeltaTime;
                }

                return;
            }
            
            if (IsGuarded || IsDamaged)
            {
                moveSpeed = GetCurrentKnockBackMoveSpeed();
                
                if (moveSpeed != null)
                {
                    _position.x += moveSpeed.speed * Sign * Time.fixedDeltaTime;
                }
                    
                return;
            }
            
            moveSpeed = _fighterData.ActionDatas[CurrentActionID].GetMoveSpeed(CurrentActionFrame);
            
            if (moveSpeed != null)
            {
                _position.x += moveSpeed.speed * Sign * Time.fixedDeltaTime;
            }
        }
        
        private bool RequestAction(int actionID, int startFrame = 0)
        {
            if (IsActionEnd)
            {
                if (_fighterData.ActionDatas[actionID].isLoop)
                {
                    SetCurrentAction(actionID, _fighterData.ActionDatas[actionID].loopFromFrame);
                    return true;
                }
                
                SetCurrentAction(actionID, startFrame);
                return true;
            }
            
            if(CurrentActionID == actionID) return false;
            
            if (_fighterData.ActionDatas[CurrentActionID].isAlwayscancelable)
            {
                SetCurrentAction(actionID, startFrame);
                return true;
            }
            
            return false;
        }

        public void RequestWinAction()
        {
            _isWin = true;
        }
        
        private void SetCurrentAction(int actionID, int startFrame = 0)
        {
            CurrentActionID = actionID;
            _currentActionFrame = startFrame;
            
            _currentAttackhitCount = 0;
            ShakeSpritePower = 0;
            HitStunFrame = 0;
            _bufferActionStartFrame = 0;
            _bufferActionID = -1;
            _executeActionID = -1;
            IsIgnorePushBox = _fighterData.ActionDatas[CurrentActionID].isIgnorePushBox;
        }

        private bool RequestCommand(CommandType commandType)
        {
            if (!_fighterData.CommandDatas.TryGetValue(commandType, out CommandData commandData))
            {
                if (!_fighterData.CommandDatas.TryGetValue(CommandType.None, out commandData))
                    return false;
            }

            return RequestAction(commandData.ActionID);
        }
        
        private void TryCommand()
        {
            if (CheckCommand(CommandDataStorage.Command623, 15))
            {
                if (TryCancel(CommandType.Command623)) return;
                if (RequestCommand(CommandType.Command623)) return;
            }
            
            if (CheckCommand(CommandDataStorage.Command214, 15))
            {
                if (TryCancel(CommandType.Command214)) return;
                if (RequestCommand(CommandType.Command214)) return;
            }

            if (CheckCommand(CommandDataStorage.Command6, 1))
            {
                if (TryCancel(CommandType.Command6)) return;
                if (RequestCommand(CommandType.Command6)) return;
            }

            if (CheckCommand(CommandDataStorage.Command4, 1))
            {
                if (TryCancel(CommandType.Command4)) return;
                if (RequestCommand(CommandType.Command4)) return;
            }

            if (TryCancel(CommandType.None)) return;
            
            RequestCommand(CommandType.None);
        }

        private bool CheckForwardDash()
        {
            if (!IsInputForward(inputDown[0])) return false;

            bool isNeutral = false;
            
            for (int i = 1; i <= 10; i++)
            {
                if (IsInputForward(inputDown[i]))
                {
                    for (int j = 1; j < i; j++)
                    {
                        if (IsInputNeutral(inputDown[j]))
                        {
                            isNeutral = true;
                            continue;
                        }
                        
                        return false;
                    }
                }
            }
            
            return isNeutral;
        }
        
        private bool CheckBackwardDash()
        {
            if (!IsInputBackward(inputDown[0])) return false;

            bool isNeutral = false;
            
            for (int i = 1; i <= 10; i++)
            {
                if (IsInputBackward(inputDown[i]))
                {
                    for (int j = 1; j < i; j++)
                    {
                        if (IsInputNeutral(inputDown[j]))
                        {
                            isNeutral = true;
                            continue;
                        }
                        
                        return false;
                    }
                }
            }
            
            return isNeutral;
        }
        
        private bool TryCancel(CommandType commandType)
        {
            foreach(CancelData cancelData in _fighterData.ActionDatas[CurrentActionID].GetCancelData(CurrentActionFrame))
            {
                if (cancelData.commandType != commandType) continue;
                
                if(cancelData.execute)
                {
                    Debug.Log($"현재 액션 프레임{CurrentActionFrame}");
                    Debug.Log($"익스큐트 아이디는 {cancelData.nextActionID}");
                    _executeActionID = cancelData.nextActionID;
                    return true;
                }

                if(cancelData.buffer)
                {
                    Debug.Log($"커맨드 입력 프레임{CurrentActionFrame}");
                    Debug.Log($"버퍼 아이디는 {cancelData.nextActionID}");
                    _bufferActionID = cancelData.nextActionID;
                    _bufferActionStartFrame = cancelData.startEndFrame.y + 1;
                    return true;
                }
            }

            return false;
        }

        private bool CanCancelAttack()
        {
            if (_currentAttackhitCount > 0) return true;
            
            return false;
        }

        public void UpdateFacingDirection(Vector2 opponentPosition)
        {
            ActionData currentAction = _fighterData.ActionDatas[CurrentActionID];

            if (!currentAction.isAlwayscancelable && !IsActionEnd) return;
            
            _isFaceRight = _position.x < opponentPosition.x;
        }

        public bool CanAttackMore(int attackID)
        {
            if (_currentAttackhitCount >= _fighterData.AttackDatas[attackID].hitCount)
            {
                return false;
            }

            return true;
        }

        public void SuccessfullyAttack()
        {
            _currentAttackhitCount++;
        }
        
        public int GetHitStopFrame(DamageResult damageResult, int attackID)
        {
            AttackData attackData = _fighterData.AttackDatas[attackID];
            
            if (damageResult == DamageResult.Guard)
                return attackData.guardHitStopFrame;
            
            if (damageResult == DamageResult.Damage)
                return attackData.hitStopFrame;

            if (damageResult == DamageResult.GuradBreak)
                return attackData.guardBreakHitStopFrame;
            
            return 0;
        }

        public void SetHitStopFrame(int hitStopFrame)
        {
            _currentHitStopFrame = hitStopFrame;
        }

        public int GetShakeSpritePower(DamageResult damageResult, int attackID)
        {
            AttackData attackData = _fighterData.AttackDatas[attackID];
            
            if (damageResult == DamageResult.Guard)
                return attackData.guardShakePower;
            
            if (damageResult == DamageResult.Damage)
                return attackData.hitShakePower;

            if (damageResult == DamageResult.GuradBreak)
                return attackData.guardBreakShakePower;
            
            return 0;
        }
        
        public void SetShakeSpritePower(int shakePower)
        {
            ShakeSpritePower = shakePower * Sign;
        }

        public EffectType GetEffectType(int attackID)
        {
            AttackData attackData = _fighterData.AttackDatas[attackID];
            return attackData.effectType;
        }

        public List<MoveSpeed> GetMoveSpeeds(DamageResult damageResult, int attackID)
        {
            AttackData attackData = _fighterData.AttackDatas[attackID];
            
            if (damageResult == DamageResult.Guard)
                return attackData.guradMoveSpeeds;
            
            if (damageResult == DamageResult.Damage)
                return attackData.hitMoveSpeeds;

            return null;
        }

        public void SetMoveSpeeds(List<MoveSpeed> moveSpeeds)
        {
            _knockBackMoveSpeeds = moveSpeeds;
            _currentKnockBackFrame = 0;
        }
        
        private MoveSpeed GetCurrentKnockBackMoveSpeed()
        {
            if (_knockBackMoveSpeeds == null) return null;

            foreach (MoveSpeed knockBackMoveSpeed in _knockBackMoveSpeeds)
            {
                if (_currentKnockBackFrame >= knockBackMoveSpeed.startEndFrame.x &&
                    _currentKnockBackFrame <= knockBackMoveSpeed.startEndFrame.y)
                {
                    _currentKnockBackFrame++;
                    return knockBackMoveSpeed;
                }
            }
            
            _knockBackMoveSpeeds = null;
            _currentKnockBackFrame = 0;
            return null;
        }

        public int GetHitStunFrame(DamageResult damageResult, int attackID)
        {
            AttackData attackData =  _fighterData.AttackDatas[attackID];

            if (damageResult == DamageResult.Damage)
                return attackData.hitStunFrame;
            
            if (damageResult == DamageResult.Guard)
                return attackData.guardHitStunFrame;
            
            if(damageResult == DamageResult.GuradBreak)
                return attackData.guardHitStunFrame;
            
            return 0;
        }

        public void SetHitStunFrame(int hitStunFrame)
        {
            HitStunFrame = hitStunFrame;
        }
        
        public DamageResult DamagedAction(AttackData attackData, Vector2 opponentPosition)
        {
            _isFaceRight = _position.x < opponentPosition.x;
            
            bool isGuardBreak = false;
            
            if (attackData.gaurdDamage > 0)
            {
                GuardBreakGauge -= attackData.gaurdDamage;
                
                if (GuardBreakGauge < 0)
                {
                    GuardBreakGauge = 0;
                    isGuardBreak = true;
                }
                OnGuardBreakGaugeChanged?.Invoke(GuardBreakGauge);
            }
            
            if (CurrentActionID == (int)FighterState.Backward || 
                _fighterData.ActionDatas[CurrentActionID].actionType == ActionType.Guard)
            {
                if (isGuardBreak)
                {
                    SetCurrentAction(attackData.guardActionID);
                    _reserveActionID = attackData.guardBreakActionID;
                    return DamageResult.GuradBreak;
                }
                SetCurrentAction(attackData.guardActionID);
                return DamageResult.Guard;
            }

            if (attackData.damage > 0) _healthGage -= attackData.damage;
            
            if(_healthGage > 0)
            {
                SetCurrentAction(attackData.damageActionID);
                return DamageResult.Damage;
            }
            else
            {
                SetCurrentAction(attackData.deadActionID);
                IsDead = true;
                return DamageResult.Dead;
            }
            
        }

        public AttackData GetAttackData(int attackID)
        {
            AttackData attackData = _fighterData.AttackDatas[attackID];
            return attackData;
        }

        private bool IsInputForward(int input)
        {
            if (_isFaceRight)
            {
                return (input & (int)InputDefine.Right) > 0;
            }
            else
            {
                return (input & (int)InputDefine.Left) > 0;
            }
        }

        private bool IsInputBackward(int input)
        {
            if (_isFaceRight)
            {
                return (input & (int)InputDefine.Left) > 0;
            }
            else
            {
                return (input & (int)InputDefine.Right) > 0;
            }
        }
        
        private bool IsInputDown(int input)
        {
                return (input & (int)InputDefine.Down) > 0;
        }
        
        private bool IsInputUp(int input)
        {
            return (input & (int)InputDefine.Up) > 0;
        }
        
        private bool IsInputNeutral(int input)
        {
            return !IsInputForward(input) &&
                   !IsInputBackward(input) &&
                   !IsInputUp(input) &&
                   !IsInputDown(input);
        }
        
        // 숫자키패드를 기준으로 어느 방향의 입력을 했는지 반환해주는 함수. 예: ↙(왼쪽 아래 입력)이라면 1을 반환 
        private int GetDirection(int inputValue) 
        {
            bool isUp = IsInputUp(inputValue);
            bool isDown = IsInputDown(inputValue);
            bool isForward = IsInputForward(inputValue);
            bool isBackward = IsInputBackward(inputValue);

            if (isDown && isBackward) return 1;
            if (isDown && isForward) return 3;
            if (isUp && isBackward) return 7;
            if (isUp && isForward) return 9;
            
            if (isDown) return 2;
            if (isBackward) return 4;
            if (isForward) return 6;
            if (isUp) return 8;
            
            return 5;
        }

        // 실제 입력 방향이 커맨드에서 요구하는 방향으로 인정될 수 있는지 확인.
        // 상하좌우 방향은 해당 방향 성분을 포함하는 대각선 입력도 허용. 예:
        // 커맨드 방향이 2(아래)라면 실제 입력 1, 2, 3을 모두 허용.
        // private bool IsCorrectDirection(int actualDirection, int commandDirection)
        // {
        //     switch (commandDirection)
        //     {
        //         case 1 : return actualDirection == 1;
        //         case 2 : return actualDirection == 1 || actualDirection == 2 || actualDirection == 3;
        //         case 3 : return actualDirection == 3;
        //         case 4 : return actualDirection == 1 || actualDirection == 4 || actualDirection == 7;
        //         case 5 : return actualDirection == 5;
        //         case 6 : return actualDirection == 3 || actualDirection == 6 || actualDirection == 9;
        //         case 7 : return actualDirection == 7;
        //         case 8 : return actualDirection == 7 || actualDirection == 8 || actualDirection == 9;
        //         case 9 : return actualDirection == 9;
        //         
        //         default : return false;
        //     }
        // }
        
        // 실제 입력 방향이 커맨드에서 요구하는 방향으로 인정될 수 있는지 확인.
        private bool IsCorrectDirection(int actualDirection, int commandDirection)
        {
            switch (commandDirection)
            {
                case 1 : return actualDirection == 1;
                case 2 : return actualDirection == 2;
                case 3 : return actualDirection == 3;
                case 4 : return actualDirection == 4;
                case 5 : return actualDirection == 5;
                case 6 : return actualDirection == 6;
                case 7 : return actualDirection == 7;
                case 8 : return actualDirection == 8;
                case 9 : return actualDirection == 9;
                
                default : return false;
            }
        }
        
        
        private bool CheckCommand(int[] command, int maxFrame) // 커맨드 체크 함수
        {
            int commandIndex = command.Length - 1; // command의 인덱스, 맨 마지막 커맨드부터 차례대로 입력 확인
                                                   // 214를 예로들면 4 -> 1 -> 2 순으로 확인 
            bool isFindLastCommand = false;       // 맨 마지막 커맨드를 찾았는지

            for (int frame = 0; frame < maxFrame; frame++)
            {
                int direction = GetDirection(input[frame]);

                if (!isFindLastCommand) // 맨 마지막 커맨드를 찾을때까지 다음 if문으로 갈 수 없음
                {
                    if (!IsCorrectDirection(direction, command[commandIndex])) continue; // 못 찾았으면 다음 프레임 검사
                    
                    Debug.Log($"{direction}방향 입력됨. 확인된 인덱스 {frame}");
                    
                    commandIndex--; // 맞으면 인덱스 감소. 다음 커맨드 검사
                    
                    isFindLastCommand = true;

                    if (commandIndex < 0) return true; // 하나짜리 커맨드도 있을 수 있으니 commandIndex가 0보다 작아지면 true 반환

                    continue;
                }

                if (IsCorrectDirection(direction, command[commandIndex])) // 입력 검사. 틀리면 다음 if문으로
                {
                    Debug.Log($"{direction}방향 입력됨. 확인된 인덱스 {frame}");
                    commandIndex--; // 맞으면 인덱스 감소. 다음 커맨드 검사
                    
                    // for문 조건 내에 commandIndex가 0보다 작아졌다면 해당 커맨드를 입력 한 것이므로 true 반환
                    if (commandIndex < 0)
                    {
                        Debug.Log($"command{command[0]}{command[1]}{command[2]} 확인 됨!");
                        return true;
                    } 
                    
                    continue; // 아직 검사할 인덱스가 남았다면 continue
                }
                
                // // 입력 방향이 중립이거나 앞의 입력과 같다면 continue
                // if (direction == 5 || direction == command[commandIndex + 1]) 
                // {
                //     continue;
                // }
                //
                // return false; // 엉뚱한 방향이 들어오면 false
            }
            
            return false; // for문에서 만족하는 커맨드 입력을 찾지 못 했다면 false 
        }
        
        private bool IsInputAttack(int input)
        {
            return (input & (int)InputDefine.Attack) > 0;
        }
        
        public void UpdateFighterSound()
        {
            SEData sound = _fighterData.ActionDatas[CurrentActionID].GetSEData(CurrentActionFrame);
            if (sound != null) SoundManager.Instance.PlayFighterSE(sound.audioClip, _isFaceRight, Position);
            
        }

        public AudioClip GetHitSound(DamageResult damageResult, int attackID)
        {
            AttackData attackData =  _fighterData.AttackDatas[attackID];
            AudioClip audioClip;

            if (damageResult == DamageResult.Damage)
            {
                audioClip = attackData.isUseBaseHitSE ? 
                    SoundManager.Instance.BaseHitSE : attackData.hitSE;
                return audioClip;
            }

            if (damageResult == DamageResult.Guard)
            {
                audioClip = attackData.isUseBaseGuardSE ? 
                    SoundManager.Instance.BaseGaurdSE : attackData.guardSE;
                return audioClip;
            }
            
            if (damageResult == DamageResult.GuradBreak)
            {
                audioClip = attackData.isUseBaseGuardBreakSE ? 
                    SoundManager.Instance.BaseGuardBreakSE : attackData.guardBreakSE;
                return audioClip;
            }
            return null;
        }

        public void SetHitsound(AudioClip audioClip)
        {
            SoundManager.Instance.PlayFighterSE(audioClip, _isFaceRight, _position);
        }

        public void ChangePosition(float x, float y)
        {
            _position.x += x;
            _position.y += y;

            foreach (HitBox hitBox in _hitBoxes)
            {
                hitBox.rect.x += x;
                hitBox.rect.y += y;
            }

            foreach (HurtBox hurtBox in _hurtBoxes)
            {
                hurtBox.rect.x += x;
                hurtBox.rect.y += y;
            }

            _pushBox.rect.x += x;
            _pushBox.rect.y += y;
            
            _wallPushBox.rect.x += x;
            _wallPushBox.rect.y += y;
        }

        public void UpdateBoxes()
        {
            _hitBoxes.Clear();
            _hurtBoxes.Clear();

            foreach (HitBoxData hitboxData in _fighterData.ActionDatas[CurrentActionID]
                         .GetHitBoxData(CurrentActionFrame))
            {
                HitBox box = new HitBox();
                box.rect = MoveBoxes(hitboxData.rect, _position);
                box.attackID = hitboxData.attackID;
                _hitBoxes.Add(box);
            }

            foreach (HurtBoxData hurtBoxData in _fighterData.ActionDatas[CurrentActionID]
                         .GetHurtBoxData(CurrentActionFrame))
            {
                HurtBox hurtBox = new HurtBox();
                Rect rect = hurtBoxData.useBaseRect ? _fighterData.baseHurtBox : hurtBoxData.rect;
                hurtBox.rect = MoveBoxes(rect, _position);
                _hurtBoxes.Add(hurtBox);
            }

            PushBoxData pushBoxData = _fighterData.ActionDatas[CurrentActionID].GetPushBoxData(CurrentActionFrame);
            
            if (pushBoxData == null)
            {
                _pushBox = null;
                return;
            }
            
            if (pushBoxData != null)
            {
                _pushBox = new PushBox();
                Rect pushRect = pushBoxData.useBaseRect ? _fighterData.basePushBox : pushBoxData.rect;
                _pushBox.rect = MoveBoxes(pushRect, _position);

            }
            
            WallPushBoxData wallPushBoxData = _fighterData.ActionDatas[CurrentActionID].GetWallPushBoxData(CurrentActionFrame);
            
            _wallPushBox = new WallPushBox();
            Rect wallPushRect = wallPushBoxData.useBaseRect ? _fighterData.baseWallPushBox : wallPushBoxData.rect;
            _wallPushBox.rect = MoveBoxes(wallPushRect, _position);
        }

        private Rect MoveBoxes(Rect boxData, Vector2 basePosition)
        {
            Rect rect = new Rect();
            rect.x = basePosition.x + (Sign * boxData.x);
            rect.y = basePosition.y + boxData.y;
            rect.width = boxData.width;
            rect.height = boxData.height;
            return rect;
        }
    }
}


