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
            OptionManager.Instance.OpenOptionUI();
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
