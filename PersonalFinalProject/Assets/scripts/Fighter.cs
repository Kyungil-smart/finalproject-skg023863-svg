using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace MyGame
{
    public class BoxBase
    {
        public Rect rect;
        
        public float xMin { get { return rect.x - rect.width / 2; }}
        public float xMax { get { return rect.x + rect.width / 2; }}
        public float yMin { get { return rect.y; }}
        public float yMax { get { return rect.y + rect.height; }}

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
        public int CurrentActionID { get; private set; }

        private bool _isFaceRight = true;
        public bool IsFaceRight => _isFaceRight;
        
        public int Sign { get { return _isFaceRight ? 1 : -1; } }
        
        private InputData _currentInput;
        
        private int _currentActionFrame; // 현재 액션의 핸재 프레임 
        public int CurrentActionFrame => _currentActionFrame;
        
        public bool isActionEnd { get { return _currentActionFrame >= _fighterData.ActionDatas[CurrentActionID].frameCount; } }
        
        public int startFrame { get { return _fighterData.ActionDatas[CurrentActionID].loopFromFrame; } }

        private int _currentHitStopFrame; // 현재 공격의 남아있는 히트 스탑 프레임 수, 프레임 마다 -- 됨

        // 이 공격 이 이번 액션에서 이미 몇 번 적중했는가 확인용
        // 1히트 공격이 들어갔을 경우 1히트 보다 더 히트되면 안 되므로 비교하기 위해 사용되는 변수
        private int _currentActionhitCount; 
        
        private List<HitBox> _hitBoxes = new();
        public  List<HitBox> HitBoxes => _hitBoxes;
        
        private List<HurtBox> _hurtBoxes = new();
        public List<HurtBox> HurtBoxes => _hurtBoxes;
        
        private List<PushBox> _pushBoxes = new();
        public List<PushBox> PushBoxes => _pushBoxes;
        
        private FighterData _fighterData;

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
        
        public void UpdateMovement()
        {
            // int faceDir = _isFaceRight ? 1 : -1;
            
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
        }
        
        public void IncrementActionFrame()
        {
            if (_currentHitStopFrame > 0)
            {
                _currentHitStopFrame--;
                return;
            }
            
            _currentActionFrame++;

            if (isActionEnd)
            {
                if (_fighterData.ActionDatas[CurrentActionID].isLoop)
                {
                    SetCurrentAction(CurrentActionID, startFrame);
                }
            }
        }
        private void RequestAction(int actionID)
        {
            if(CurrentActionID == actionID) return;

            SetCurrentAction(actionID);
        }

        public void UpdateAction()
        {
            if(CurrentActionID == (int)FighterActionID.NAttack ||  CurrentActionID == (int)FighterActionID.Damaged)
            {
                if(CurrentActionFrame >= _fighterData.ActionDatas[CurrentActionID].frameCount)
                {
                    RequestAction((int)FighterActionID.Idle);
                }

                return;
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
        
        private void SetCurrentAction(int actionID, int startFrame = 0)
        {
            CurrentActionID = actionID;
            // Debug.Log($"현재 액션 : {(FighterActionID)CurrentActionID}");
            _currentActionFrame = startFrame;
            _currentActionhitCount = 0;
        }

        public void UpdateFacingDirection(Fighter opponent)
        {
            _isFaceRight = _position.x < opponent.Position.x;
        }

        public bool CanAttackMore(int attackID)
        {
            if (_currentActionhitCount >= _fighterData.AttackDatas[attackID].hitCount)
            {
                return false;
            }

            return true;
        }

        public void SuccessfulAttack()
        {
            _currentActionhitCount++;
        }

        public void SetHitStopFrame(int hitStopFrame)
        {
            _currentHitStopFrame = hitStopFrame;
        }

        public void DamagedToAttacker()
        {
            SetCurrentAction((int)FighterActionID.Damaged);
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
                Rect rect = new Rect();
                rect = hurtBoxData.useBaseRect ? _fighterData.baseHurtBox : hurtBoxData.rect;
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


