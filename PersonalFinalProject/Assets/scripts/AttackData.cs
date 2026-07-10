using UnityEngine;
using System;
using System.Collections.Generic;


namespace MyGame
{
    [Serializable]
    public class AttackData
    {
        public int attackID; // 공격 ID
        public string attackName; // 공격 이름
        public int hitCount; // 최대 히트 수

        public int damageActionID; // 공격이 성공했을 시 상대가 취할 액션 ID
        public int guardActionID; // 상대가 가드 했을 시 상대가 취할 액션 ID
        public int guardBreakActionID; // 상대가 가드 브레이크 됐을 시 취할 액션 ID
        public int deadActionID; // 상대가 해당 공격을 맞고 죽었을 경우 취할 액션 ID

        public int damage; // 해당 공격의 공격력. 피격 받은 파이터의 체력에 영향을 줌
        public int gaurdDamage; // 가드 브레이크 게이지에 영향을 주는 공격력. 
        
        public int hitShakePower; // 공격이 성공했을 시 상대 스프라이트의 흔들림(타격감) 수치 
        public int guardShakePower; // 상대방이 가드했을 시 상대 스프라이트의 흔들림(타격감) 수치
        public int guardBreakShakePower; // 상대방이 가드 브레이크가 됐을 시 상대 스프라이트의 흔들림(타격감) 수치
        
        public int hitStopFrame; // 공격에 적중 시 히트 스톱
        public int guardHitStopFrame; // 공격을 가드 시 히트 스톱
        public int guardBreakHitStopFrame; // 가드 브레이크 시 히트 스톱
        
        public int hitStunFrame; // 공격에 적중 당했을 시 스턴 프레임(히트 당했을 시 해당 히트 액션의 총 프레임 수)
        public int guardHitStunFrame; // 공격을 가드 했을 시 스턴 프레임(가드 했을 시 해당 가드 액션의 총 프레임 수)

        public bool isUseBaseHitSE;
        public bool isUseBaseGuardSE;
        public bool isUseBaseGuardBreakSE;
        
        public AudioClip hitSE;
        public AudioClip guardSE;
        public AudioClip guardBreakSE;
        
        public List<MoveSpeed> hitMoveSpeeds; // 상대가 히트 당햇을 시 넉백 속도 
        public List<MoveSpeed> guradMoveSpeeds; // 상대가 가드 했을 시 넉백 속도
    }
}

