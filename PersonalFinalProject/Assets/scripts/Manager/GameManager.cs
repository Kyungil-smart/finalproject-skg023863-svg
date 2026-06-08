using UnityEngine;

namespace MyGame
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance; // 싱글톤
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            
            Application.targetFrameRate = 60; // 60프레임 고정
        }
    }
}
