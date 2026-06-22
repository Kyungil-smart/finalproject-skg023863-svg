using System.Collections.Generic;
using System.Net;
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
    
    // Fighter들의 행동을 열거형으로 정리
    public enum FighterActionID
    {
        Idle,
        Forward,
        Backward,
        NAttack,
        Damaged,
        CrouchGuard,
    }

    // Fighter가 공격 받았을 때 어떤 상황인지 열거형으로 정리
    public enum DamageResult
    {
        Damage, 
        Guard,  
        GuradBreak, 
    }
    
    // 대전에서 사용할 캐릭터(Fighter)의 로직
    public class Fighter
    {
        private Vector2 _position; // Fighter의 위치
        public Vector2 Position => _position;

        // Fighter의 속도. 이동속도를 제외하고 특정 액션에서 속도가 필요할 경우 이 변수에 적용해서 사용. 예)가드 시 밀려 날 때, 전진성 있는 공격 등
        private float _velocityX; 
        
        public int CurrentActionID { get; private set; } // 현재의 액션 ID를 저장

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
        
        // 만약 IsLoop
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
        
        private List<PushBox> _pushBoxes = new();
        public List<PushBox> PushBoxes => _pushBoxes;
        
        private List<MoveSpeed> _knockBackMoveSpeeds;

        private int _currentKnockBackFrame;
        
        private FighterData _fighterData;
        
        public FighterData FighterData => _fighterData;

        public void BattleSetup(FighterData fighterData, Vector2 position, bool isFaceRight)
        {
            _fighterData = fighterData;
            _position = position;
            _isFaceRight = isFaceRight;
            
            SetCurrentAction((int)FighterActionID.Idle);
        }

        public void UpdateInput(InputData input)
        {
            _currentInput = input;
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
        private void RequestAction(int actionID, int startFrame = 0)
        {
            if (IsActionEnd)
            {
                SetCurrentAction(actionID, startFrame);
                return;
            }
            
            if(CurrentActionID == actionID) return;
            
            
            if (_fighterData.ActionDatas[CurrentActionID].isAlwayscancelable)
            {
                SetCurrentAction(actionID, startFrame);
            }
        }

        public void UpdateAction()
        {
            if (!_fighterData.ActionDatas[CurrentActionID].isAlwayscancelable)
            {
                if (!IsActionEnd) return;
            }

            if(_currentInput.Attack)
            {
                RequestAction((int)FighterActionID.NAttack);
                return;
            }

            bool isForward;
            bool isBackward;

            if (_isFaceRight)
            {
                isForward = _currentInput.MoveX > 0;
                isBackward = _currentInput.MoveX < 0;
            }
            else
            {
                isForward = _currentInput.MoveX < 0;
                isBackward = _currentInput.MoveX > 0;
            }

            if (isForward)
            {
                RequestAction((int)FighterActionID.Forward);
            }
            else if (isBackward)
            {
                RequestAction((int)FighterActionID.Backward);
            }
            else
            {
                RequestAction((int)FighterActionID.Idle);
            }
        }
        
        public void UpdateMovement()
        {
            if (!IsHitStopEnd) return;
            
            if (CurrentActionID == (int)FighterActionID.Forward)
            {
                _position.x += _fighterData.forwardSpeed * Sign * Time.fixedDeltaTime;
                return;
            }
            if (CurrentActionID == (int)FighterActionID.Backward)
            {
                _position.x -= _fighterData.backwardSpeed * Sign * Time.fixedDeltaTime;
                return;
            }

            MoveSpeed moveSpeed;
            
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
                _velocityX = moveSpeed.speed;
                _position.x += moveSpeed.speed * Sign * Time.fixedDeltaTime;
            }
            _velocityX = 0;
            
            
        }
        
        private void SetCurrentAction(int actionID, int startFrame = 0)
        {
            CurrentActionID = actionID;
            _currentActionFrame = startFrame;
            
            _currentAttackhitCount = 0;
            ShakeSpritePower = 0;
            HitStunFrame = 0;
        }

        public void UpdateFacingDirection(Fighter opponent)
        {
            _isFaceRight = _position.x < opponent.Position.x;
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
            
            return 0;
        }

        public void SetHitStopFrame(int hitStopFrame)
        {
            _currentHitStopFrame = hitStopFrame;
        }
        
        public void SetShakeSpritePower(int shakePower)
        {
            ShakeSpritePower = shakePower * Sign;
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

        public int GetHitStunFrame(DamageResult damageResult, int attackID)
        {
            AttackData attackData =  _fighterData.AttackDatas[attackID];

            if (damageResult == DamageResult.Damage)
                return attackData.hitStunFrame;
            
            if (damageResult == DamageResult.Guard)
                return attackData.guardHitStunFrame;

            return 0;
        }

        public void SetHitStunFrame(int hitStunFrame)
        {
            HitStunFrame = hitStunFrame;
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

        public DamageResult DamagedAction(AttackData attackData)
        {
            if (CurrentActionID == (int)FighterActionID.Backward)
            {
                SetCurrentAction(attackData.guardActionID);
                return DamageResult.Guard;
            }
            else
            {
                SetCurrentAction(attackData.damageActionID);
                return DamageResult.Damage;
            }
        }

        public AttackData GetAttackData(int attackID)
        {
            AttackData attackData = _fighterData.AttackDatas[attackID];
            return attackData;
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
                // hurtBox.rect = MoveBoxes(hurtBoxData.rect, _position);
                _hurtBoxes.Add(hurtBox);
            }
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


