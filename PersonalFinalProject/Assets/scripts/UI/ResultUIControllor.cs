using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MyGame
{
    public class ResultUIControllor : CursorControllor
    {
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private TMP_Text _winnerText;

        public event Action OnPlayAgainClicked; // Play Again 버튼 눌렀을 시 발동하는 이벤트
        
        public void OpenResultUI(bool isFinalWinnerPlayerOne)
        {
            int playerNumber = isFinalWinnerPlayerOne ? 1 : 2;

            _winnerText.text = $"Player {playerNumber}";
            
            gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(_playAgainButton.gameObject);
        
            Time.timeScale = 0;
        }

        public void OnPlayAgain()
        {
            gameObject.SetActive(false);
            
            OnPlayAgainClicked?.Invoke();
            Time.timeScale = 1;
        }

        public void OnGoToTitle()
        {
            gameObject.SetActive(false);
            
            string titleName = SceneName.TitleScene.ToString();
            SceneManager.LoadScene(titleName);
            Time.timeScale = 1;
        }
    }
}

