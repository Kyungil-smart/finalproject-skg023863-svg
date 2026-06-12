using UnityEngine;
using System;


namespace MyGame
{
    [Serializable]
    public class AttackData
    {
        public int attackID; // 공격 ID
        public int hitCount; // 최대 히트 수
        public int hitStopFrame; // 공격에 적중 시 히트 스톱
    }
}

