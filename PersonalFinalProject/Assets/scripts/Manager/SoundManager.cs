using UnityEngine;
using UnityEngine.Audio;

namespace MyGame
{
    public enum VolumeType
    {
        MasterVolume,
        BgmVolume,
        SEVolume
    }
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioMixer _audioMixer;
        
        [SerializeField] private AudioSource _BgmSource;
        [SerializeField] private AudioSource _player1SESource;
        [SerializeField] private AudioSource _player2SESource;

        [SerializeField] private AudioClip _baseHitSE;
        [SerializeField] private AudioClip _baseGaurdSE;
        [SerializeField] private AudioClip _baseGuardBreakSE;
        
        public AudioClip BaseHitSE => _baseHitSE;
        public AudioClip BaseGaurdSE => _baseGaurdSE;
        public AudioClip BaseGuardBreakSE => _baseGuardBreakSE;
        
        public int masterVolume;
        public int bgmVolume;
        public int seVolume;

        [SerializeField] private AudioClip _TestBgm;
        
        public static SoundManager Instance;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            LoadVolume();
            DontDestroyOnLoad(gameObject);
            PlayBGM(_TestBgm);
        }
        
        public void PlayBGM(AudioClip clip)
        {
            _BgmSource.clip = clip;
            _BgmSource.Play();
        }
        
        public void StopBGM()
        {
            _BgmSource.Stop();
        }
        
        public void PlaySE(AudioClip clip)
        {
            _player1SESource.PlayOneShot(clip);
        }
        
        public void PlayFighterSE(AudioClip clip, bool isPlayer1, Vector2 position)
        {
            AudioSource source = isPlayer1 ? _player1SESource : _player2SESource;

            if (clip == null) return;

            source.panStereo = CalculatePan(position.x);
            source.PlayOneShot(clip);
        }
        
        private float CalculatePan(float positionX)
        {
            return Mathf.Clamp(positionX / GameManager.Instance.MapMaxX, -1f, 1f);
        }
        public void PauseFighterSE()
        {
            _player1SESource.Pause();
            _player2SESource.Pause();
        }

        public void ResumeFighterSE()
        {
            _player1SESource.UnPause();
            _player2SESource.UnPause();
        }
        
        public void SetMasterVolume(int volume)
        {
            masterVolume = volume;
            SetVolume(VolumeType.MasterVolume, volume);
        }

        public void SetBGMVolume(int volume)
        {
            bgmVolume = volume;
            SetVolume(VolumeType.BgmVolume, volume);
        }

        public void SetSEVolume(int volume)
        {
            seVolume = volume;
            SetVolume(VolumeType.SEVolume, volume);
        }
        
        private void SetVolume(VolumeType type, int volume)
        {
            float value = Mathf.Max(volume / 100f, 0.0001f);
            _audioMixer.SetFloat(type.ToString(), Mathf.Log10(value) * 20);
        }

        public void LoadVolume()
        {
            masterVolume = PlayerPrefs.GetInt(VolumeType.MasterVolume.ToString(), 50);
            bgmVolume = PlayerPrefs.GetInt(VolumeType.BgmVolume.ToString(), 50);
            seVolume = PlayerPrefs.GetInt(VolumeType.SEVolume.ToString(), 50);
            
            SetMasterVolume(masterVolume);
            SetBGMVolume(bgmVolume);
            SetSEVolume(seVolume);
        }
    }
}

