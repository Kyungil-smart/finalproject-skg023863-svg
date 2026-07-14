using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MyGame
{
    public class BattleSceneEscape : MonoBehaviour
    {
        private MyGameInputAction _inputAction;

        void Awake()
        {
            _inputAction = new MyGameInputAction();
        }

        void OnEnable()
        {
            _inputAction.asset.Enable();
            _inputAction.UI.Escape.performed += GoToTitleScene;
        }

        void OnDisable()
        {
            _inputAction.UI.Escape.performed -= GoToTitleScene;
            _inputAction.asset.Disable();
        }

        void GoToTitleScene(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            SceneManager.LoadScene(SceneName.TitleScene.ToString());
        }
    }
}

