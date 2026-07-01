using UnityEngine;
using System;

namespace MyGame
{
    public class GaurdBreakGaugeController : MonoBehaviour
    {
        [SerializeField] private BattleManager _battleManager;

        [SerializeField] private bool _isPlayerOne;

        [SerializeField] private GameObject[] _guardBreakGaugeImage;

        private Fighter _fighter;
        
        void Start()
        {
            _fighter = _isPlayerOne ? _battleManager.Fighter1 : _battleManager.Fighter2;
            _fighter.OnGuardBreakGaugeChanged += UpdateGuardBreakGauge;

            for (int i = 0; i < _guardBreakGaugeImage.Length; i++)
            {
                _guardBreakGaugeImage[i].SetActive(true);
            }
        }

        void OnDestroy()
        {
            if (_fighter != null) _fighter.OnGuardBreakGaugeChanged -= UpdateGuardBreakGauge;
        }

        void UpdateGuardBreakGauge(int gauge)
        {
            _guardBreakGaugeImage[gauge].SetActive(false);
        }
        
    }
}

