using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MyGame
{
    public class OptionMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject _optionUI;
        
        [SerializeField] private Button _masterVolumeButton;
        [SerializeField] private Button _BgmVolumeButton;
        [SerializeField] private Button _SeVolumeButton;
        
        [SerializeField] private TMP_Text _masterVolumeText;
        [SerializeField] private TMP_Text _bgmVolumeText;
        [SerializeField] private TMP_Text _seVolumeText;
        
        private GameObject _previousSelectedObject;

        private int _masterVolume;
        private int _bgmVolume;
        private int _seVolume;
        
        [SerializeField] private RectTransform _cursor;
        public OptionType cursorIndex;
        
        void Start()
        {
            Debug.Log("OptionMenuController Start 실행");
            
            _masterVolume = SoundManager.Instance.masterVolume;
            _bgmVolume =  SoundManager.Instance.bgmVolume;
            _seVolume = SoundManager.Instance.seVolume;
            
            Debug.Log($"초기화 Master : {_masterVolume}");
            
            _masterVolumeText.text = _masterVolume.ToString();
            _bgmVolumeText.text = _bgmVolume.ToString();
            _seVolumeText.text = _seVolume.ToString();
        }
        
        private void RefreshVolume()
        {
            _masterVolume = SoundManager.Instance.masterVolume;
            _bgmVolume = SoundManager.Instance.bgmVolume;
            _seVolume = SoundManager.Instance.seVolume;

            _masterVolumeText.text = _masterVolume.ToString();
            _bgmVolumeText.text = _bgmVolume.ToString();
            _seVolumeText.text = _seVolume.ToString();
        }

        public void MoveCursor(RectTransform _cursorPoint)
        {
            _cursor.position = _cursorPoint.position;
        }

        public void SetCursorIndex(OptionType optionType)
        { 
            cursorIndex = optionType;
        }
        
        public void MasterVolumeUp()
        {
            Debug.Log($"변경 전 Master : {_masterVolume}");
            ChangeVolume(VolumeType.MasterVolume, 5);
            _masterVolumeButton.Select();
        }

        public void MasterVolumeDown()
        {
            ChangeVolume(VolumeType.MasterVolume, -5);
            _masterVolumeButton.Select();
        }

        public void BgmVolumeUp()
        {
            ChangeVolume(VolumeType.BgmVolume, 5);
            _BgmVolumeButton.Select();
        }

        public void BgmVolumeDown()
        {
            ChangeVolume(VolumeType.BgmVolume, -5);
            _BgmVolumeButton.Select();
        }

        public void SeVolumeUp()
        {
            ChangeVolume(VolumeType.SEVolume, 5);
            _SeVolumeButton.Select();
        }

        public void SeVolumeDown()
        {
            ChangeVolume(VolumeType.SEVolume, -5);
            _SeVolumeButton.Select();
        }
        
        private void ChangeVolume(VolumeType type, int amount)
        {
            switch (type)
            {
                case VolumeType.MasterVolume:
                    _masterVolume += amount;
                    _masterVolume = Mathf.Clamp(_masterVolume, 0, 100);

                    SoundManager.Instance.SetMasterVolume(_masterVolume);
                    PlayerPrefs.SetInt(type.ToString(), _masterVolume);
                    _masterVolumeText.text = _masterVolume.ToString();
                    break;

                case VolumeType.BgmVolume:
                    _bgmVolume += amount;
                    _bgmVolume = Mathf.Clamp(_bgmVolume, 0, 100);

                    SoundManager.Instance.SetBGMVolume(_bgmVolume);
                    PlayerPrefs.SetInt(type.ToString(), _bgmVolume);
                    _bgmVolumeText.text = _bgmVolume.ToString();
                    break;

                case VolumeType.SEVolume:
                    _seVolume += amount;
                    _seVolume = Mathf.Clamp(_seVolume, 0, 100);

                    SoundManager.Instance.SetSEVolume(_seVolume);
                    PlayerPrefs.SetInt(type.ToString(), _seVolume);
                    _seVolumeText.text = _seVolume.ToString();
                    break;
            }
        }
        public void OpenOptionUI()
        {
            Time.timeScale = 0;
            _previousSelectedObject = EventSystem.current.currentSelectedGameObject;

            _optionUI.SetActive(true);
            EventSystem.current.SetSelectedGameObject(_masterVolumeButton.gameObject);
        }
        
        public void CloseOptionUI()
        {
            Time.timeScale = 1;
            PlayerPrefs.Save();

            _optionUI.SetActive(false);

            if (_previousSelectedObject != null)
            {
                EventSystem.current.SetSelectedGameObject(_previousSelectedObject);
            }
        }
        
        public void GoToTitle()
        {
            Time.timeScale = 1f;
            CloseOptionUI();

            SceneManager.LoadScene(SceneName.TitleScene.ToString());
            
            OptionManager.Instance.SetOptionUI();
        }
    }
}

