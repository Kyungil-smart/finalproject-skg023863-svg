using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace MyGame
{
    public enum SceneName
    {
        TitleScene,
        BattleScene
    }
    public class TitleMenuController : MonoBehaviour
    {
        private GameObject _lastSelected;
        
        [SerializeField] protected GameObject _optionUI;
        [SerializeField] protected GameObject _firstSelectedOption;
        
        void Update()
        { 
            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current != null)
            {
                _lastSelected = current;
            }
            else if (_lastSelected != null)
            {
                EventSystem.current.SetSelectedGameObject(_lastSelected);
            }
        }
        
        public void ChangeBattleScene()
        {
            ChangeScene(SceneName.BattleScene);
        }

        public void ChangeTitleScene()
        {
            ChangeScene(SceneName.TitleScene);    
        }

        public void OpenOptionUI()
        {
            _optionUI.SetActive(true);
            EventSystem.current.SetSelectedGameObject(_firstSelectedOption);
        }

        public void ExitGame()
        {
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
        }

        private void ChangeScene(SceneName scene)
        {
            string sceneName = scene.ToString();
            SceneManager.LoadScene(sceneName);
        }
    }
    
}
