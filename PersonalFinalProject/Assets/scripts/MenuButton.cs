using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
    
namespace MyGame
{
    public enum SenecName
    {
        TitleScene,
        BattleScene
    }
    public class MenuButton : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void ChangeBattleScene()
        {
            ChangeScene(SenecName.BattleScene);
        }

        public void ChangeTitleScene()
        {
            ChangeScene(SenecName.TitleScene);    
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

        private void ChangeScene(SenecName scene)
        {
            string sceneName = scene.ToString();
            SceneManager.LoadScene(sceneName);
        }
    }
}

