using System;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame
{
    public class RoundWinController : MonoBehaviour
    {
        [SerializeField] private BattleManager _battleManager;
        
        [SerializeField] private Sprite _winSprite;
        [SerializeField] private Sprite _loseSprite;
        
        [SerializeField] private GameObject[] _playerOneWinMarker;
        [SerializeField] private GameObject[] _playerTwoWinMarker;

        private Image[] _playerOneWinMarkerImage;
        private Image[] _playerTwoWinMarkerImage;

        void Start()
        {
            _playerOneWinMarkerImage =  new Image[_playerOneWinMarker.Length];
            _playerTwoWinMarkerImage =  new Image[_playerTwoWinMarker.Length];
            
            for (int i = 0; i < _battleManager.maxRoundWin; i++)
            {
                _playerOneWinMarker[i].SetActive(true);
                _playerTwoWinMarker[i].SetActive(true);
                _playerOneWinMarkerImage[i] = _playerOneWinMarker[i].GetComponent<Image>();
                _playerTwoWinMarkerImage[i] = _playerTwoWinMarker[i].GetComponent<Image>();
            }
            
            _battleManager.OnSetWinMarker += SetWinMarker;
            _battleManager.OnResetBattle += ResetWinMarker;
        }

        void OnDestroy()
        {
            _battleManager.OnSetWinMarker -= SetWinMarker;
            _battleManager.OnResetBattle -= ResetWinMarker;
        }

        void SetWinMarker()
        {
            int index;
            
            index = _battleManager.Fighter1RoundWinCount - 1;
            
            if (_battleManager.Fighter1RoundWinCount - 1 >= 0)
            {
                _playerOneWinMarkerImage[index].sprite = _winSprite;
            }
            
            index =  _battleManager.Fighter2RoundWinCount - 1;
            
            if (_battleManager.Fighter2RoundWinCount - 1 >= 0)
            {
                _playerTwoWinMarkerImage[index].sprite = _winSprite;
            }
        }

        void ResetWinMarker()
        {
            for (int i = 0; i < _battleManager.maxRoundWin; i++)
            {
                _playerOneWinMarkerImage[i].sprite = _loseSprite;
                _playerTwoWinMarkerImage[i].sprite = _loseSprite;
            }
        }
        
    }
}

