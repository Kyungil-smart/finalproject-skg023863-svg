using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    [CreateAssetMenu]
    public class FighterData : ScriptableObject
    {
        public float forwardSpeed;
        public float backwardSpeed;

        public Rect baseHurtBox;
        public Rect basePushBox;

        private Dictionary<int, ActionData> _actionDatas = new();
        public Dictionary<int, ActionData> ActionDatas => _actionDatas;
        
        private Dictionary<int, AttackData> _attackDatas = new();
        public Dictionary<int, AttackData> AttackDatas => _attackDatas;
        
        [SerializeField] private ActionDataContainer _actionDataContainer;
        [SerializeField] private AttackDataContainer _attackDataContainer;

        public void DictionaryInit()
        {
            if (_actionDataContainer == null)
            {
                Debug.LogError("No ActionDatacontainer");
                return;
            }
            
            if (_attackDataContainer == null)
            {
                Debug.LogError("No AttackDatacontainer");
                return;
            }

            foreach (var action in _actionDataContainer.actions)
            {
                _actionDatas.Add(action.actionID, action);
            }
            
            foreach (var attack in _attackDataContainer.attackDataList)
            {
                _attackDatas.Add(attack.attackID, attack);
            }
        }
    }
}

