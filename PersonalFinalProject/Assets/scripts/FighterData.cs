using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    [CreateAssetMenu]
    public class FighterData : ScriptableObject
    {
        public int healthGauge;
        public int guardBreakGauge;
        
        public float forwardSpeed;
        public float backwardSpeed;

        public Rect baseHurtBox;
        public Rect basePushBox;
        public Rect baseWallPushBox;

        private Dictionary<int, ActionData> _actionDatas = new();
        public Dictionary<int, ActionData> ActionDatas => _actionDatas;
        
        private Dictionary<int, AttackData> _attackDatas = new();
        public Dictionary<int, AttackData> AttackDatas => _attackDatas;
        
        private Dictionary<CommandType, CommandData> _commandDatas = new();
        public Dictionary<CommandType, CommandData> CommandDatas => _commandDatas;
        
        [SerializeField] private ActionDataContainer _actionDataContainer;
        [SerializeField] private AttackDataContainer _attackDataContainer;
        [SerializeField] private CommandDataContainer _commandDataContainer;

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

            if (_commandDataContainer == null)
            {
                Debug.LogError("No CommandDatacontainer");
                return;
            }

            foreach (var action in _actionDataContainer.actions)
            {
                _actionDatas.Add(action.actionID, action);
            }
            
            foreach (var attack in _attackDataContainer.attackDatas)
            {
                _attackDatas.Add(attack.attackID, attack);
            }

            foreach (var command in _commandDataContainer.commandDatas)
            {
                _commandDatas.Add(command.commandType, command);
            }
        }
    }
}

