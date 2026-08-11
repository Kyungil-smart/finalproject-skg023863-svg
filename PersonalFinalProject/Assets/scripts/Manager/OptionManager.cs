using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MyGame
{
    public class OptionManager : MonoBehaviour
    {
        public static OptionManager Instance { get; private set; }
        
        private OptionMenuController _optionMenuController;
        
        [SerializeField] private Button _seVolumeButton;
        [SerializeField] private Button _titleButton;
        [SerializeField] private Button _quitButton;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            _optionMenuController = GetComponent<OptionMenuController>();
        }
        
        public void SetOptionUI()
        {
            bool isBattleScene = SceneManager.GetActiveScene().name == SceneName.BattleScene.ToString();

            _titleButton.gameObject.SetActive(isBattleScene);

            SetNavigation(isBattleScene);
        }
        private void SetNavigation(bool isBattleScene)
        {
            Navigation seNavigation = _seVolumeButton.navigation;
            Navigation quitNavigation = _quitButton.navigation;

            if (isBattleScene)
            {
                seNavigation.selectOnDown = _titleButton;
                quitNavigation.selectOnUp = _titleButton;
            }
            else
            {
                seNavigation.selectOnDown = _quitButton;
                quitNavigation.selectOnUp = _seVolumeButton;
            }

            _seVolumeButton.navigation = seNavigation;
            _quitButton.navigation = quitNavigation;
        }
        
        public void OpenOptionUI()
        {
            SetOptionUI();
            _optionMenuController.OpenOptionUI();
        }

        public void CloseOptionUI()
        {
            _optionMenuController.CloseOptionUI();
        }
    }
}

