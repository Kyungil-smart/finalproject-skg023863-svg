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
    }
}

