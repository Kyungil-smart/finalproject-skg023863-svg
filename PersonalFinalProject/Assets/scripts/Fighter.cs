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
        public float xMin { get { return rect.x - rect.width / 2; }} // 박스의 중심점으로부터 왼쪽
        public float xMax { get { return rect.x + rect.width / 2; }} // 박스의 중심점으로부터 오른쪽
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

    public class HitBox : BoxBase
    {
        public int attackID;
    }

    public class HurtBox : BoxBase
    {
        
    }

    public class PushBox : BoxBase
    {
        
    }
    
    // 캐릭터들의 행동을 열거형으로 정리
    public enum FighterActionID
    {
        Idle,
        Forward,
        Backward,
        NAttack,
        Damaged,
        CrouchGuard,
    }

    public enum DamageResult
    {
        Damage,
        Guard,
        GuradBreak,
    }
    
    // 대전에서 사용할 캐릭터(Fighter)의 로직
    public class Fighter
    {
        private Vector2 _position;
        public Vector2 Position => _position;
        // public FighterActionID CurrentActionID { get; private set; } // 현재 액션

        private float _velocityX;
        
        public int CurrentActionID { get; private set; }

        private bool _isFaceRight = true;
        public bool IsFaceRight => _isFaceRight;
        
        public int Sign { get { return _isFaceRight ? 1 : -1; } }
        
        private InputData _currentInput;
        
        private int _currentActionFrame; // 현재 액션의 핸재 프레임 
        public int CurrentActionFrame => _currentActionFrame;
        
        public bool isActionEnd { get { return _currentActionFrame >= _fighterData.ActionDatas[CurrentActionID].frameCount; } }
        
        public int loopStartFrame { get { return _fighterData.ActionDatas[CurrentActionID].loopFromFrame; } }

        private int _currentHitStopFrame; // 현재 공격의 남아있는 히트 스탑 프레임 수, 프레임 마다 -- 됨
        
        public bool isHitStopEnd { get { return _currentHitStopFrame <= 0; } }

        public int ShakeSpritePower { get; private set; }

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
            
            if (!isHitStopEnd)
            {
                _currentHitStopFrame--;
                return;
            }
            
            _currentActionFrame++;
            
        }
        private void RequestAction(int actionID, int startFrame = 0)
        {
            if (isActionEnd)
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
                if (!isActionEnd) return;
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
            if (!isHitStopEnd) return;
            
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
            
            if (_fighterData.ActionDatas[CurrentActionID].actionType == ActionType.Guard ||
                _fighterData.ActionDatas[CurrentActionID].actionType == ActionType.Damaged)
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

        public void SuccessfullAttack()
        {
            _currentAttackhitCount++;
        }

        public void SetHitStopFrame(int hitStopFrame)
        {
            _currentHitStopFrame = hitStopFrame;
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
                RequestAction(attackData.guardActionID);
                return DamageResult.Guard;
            }
            else
            {
                RequestAction(attackData.damageActionID);
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


