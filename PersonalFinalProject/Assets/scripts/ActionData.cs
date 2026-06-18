using UnityEngine;
using System;
using System.Collections.Generic;

namespace MyGame
{
    public enum ActionType // 액션의 타입을 큰 범위로 나누기위해 사용
    {
        Movement,
        Attack,
        Damaged,
        Guard
    }
    // 모든 행동의 프레임의 기본 골자. x가 시작 프레임, y가 끝 프레임을 뜻 함.
    public abstract class FrameBase
    {
        public Vector2Int startEndFrame;
    }

    // 히트박스. 허트 박스에 겹쳤을 시 공격 판정을 내는 박스
    [Serializable]
    public class HitBoxData : FrameBase
    {
        public Rect rect;
        public int attackID;
    }
    
    // 허트박스. 히트 박스에 겹쳤을 시 피격 판정을 내는 박스
    [Serializable]
    public class HurtBoxData :  FrameBase
    {
        public Rect rect;
        public bool useBaseRect;
    }
    
    // 푸시박스. 캐릭터 끼리 서로 밀쳐내기 위해 필요한 박스
    [Serializable]
    public class PushBoxData : FrameBase
    {
        public Rect rect;
        public bool useBaseRect;
    }

    // 특정 프레임에서 캐릭터의 속도를 나타내기 위해 사용.
    [Serializable]
    public class MoveSpeed : FrameBase
    {
        public float speed;
    }
    
    // 하나의 액션에 필요한 정보들을 담고 있는 Data
    [CreateAssetMenu]
    public class ActionData : ScriptableObject
    {
        public int actionID; // 액션을 구분하기 위한 ID
        public string actionName; // 액션의 이름
        public ActionType actionType; // 액션의 타입
        public int frameCount; // 액션의 총 프레임 수
        public bool isLoop; // 루프를 하는 액션인지
        public int loopFromFrame; // 루프를 시작하면 어느 프레임부터 시작하는지
        // public MoveSpeed[] moveSpeeds;
        public List<MoveSpeed> moveSpeeds;
        public HitBoxData[] hitboxDatas;
        public HurtBoxData[] hurtboxDatas;
        public PushBoxData[] pushBoxDatas;
        public bool isAlwayscancelable; // 언제든지 캔슬할 수 있는 액션인지

        public List<HitBoxData> GetHitBoxData(int frame)
        {
            List<HitBoxData> hitbox = new List<HitBoxData>(); 
            foreach (var hitboxData in hitboxDatas)
            {
                if (frame >= hitboxData.startEndFrame.x && frame <= hitboxData.startEndFrame.y)
                {
                    hitbox.Add(hitboxData);
                }
            }

            return hitbox;
        }

        public List<HurtBoxData> GetHurtBoxData(int frame)
        {
            List<HurtBoxData> hurtbox = new List<HurtBoxData>();
            foreach (var hurtboxData in hurtboxDatas)
            {
                if (frame >= hurtboxData.startEndFrame.x && frame <= hurtboxData.startEndFrame.y)
                {
                    hurtbox.Add(hurtboxData);
                }
            }
            
            return hurtbox;
        }

        public MoveSpeed GetMoveSpeed(int frame)
        {
            foreach (var moveSpeed in moveSpeeds)
            {
                if (frame >= moveSpeed.startEndFrame.x && frame <= moveSpeed.startEndFrame.y)
                {
                    return moveSpeed;
                }
            }
            return null;
        }
        
    }
    
}
