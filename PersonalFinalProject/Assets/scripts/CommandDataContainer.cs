using UnityEngine;
using System;

namespace MyGame
{
    [Serializable]
    public class CommandData
    {
        public CommandType commandType;
        public int ActionID;
    }
    
    [CreateAssetMenu]
    public class CommandDataContainer : ScriptableObject
    {
        public CommandData[] commandDatas;
    }
}

