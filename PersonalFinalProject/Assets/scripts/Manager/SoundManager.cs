using UnityEngine;

namespace MyGame
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _BGMSource;
        [SerializeField] private AudioSource _player1SESource;
        [SerializeField] private AudioSource _player2SESource;
        
        public static SoundManager instance;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            
            DontDestroyOnLoad(gameObject);
        }
    }
}

