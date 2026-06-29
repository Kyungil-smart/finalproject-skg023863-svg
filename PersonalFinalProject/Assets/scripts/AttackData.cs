using UnityEngine;
using System;
using System.Collections.Generic;


namespace MyGame
{
    [Serializable]
    public class AttackData
    {
        public int attackID; // 공격 ID
        public string attackName;
        public int hitCount; // 최대 히트 수

        public int damageActionID;
        public int guardActionID;
        public int deadActionID;

        public int damage;
        
        public int hitStopFrame; // 공격에 적중 시 히트 스톱
        public int guardHitStopFrame; // 공격을 가드 시 히트 스톱
        public int hitStunFrame;
        public int guardHitStunFrame;
        
        public List<MoveSpeed> hitMoveSpeeds;
        public List<MoveSpeed> guradMoveSpeeds;
    }
}

