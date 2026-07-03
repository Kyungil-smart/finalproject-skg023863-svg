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
            _battleManager.OnResetGuardBraekGauge += SetGuardBreakGauge;

            SetGuardBreakGauge();
        }

        void OnDestroy()
        {
            _fighter.OnGuardBreakGaugeChanged -= UpdateGuardBreakGauge;
            _battleManager.OnResetGuardBraekGauge -= SetGuardBreakGauge;
        }

        void UpdateGuardBreakGauge(int gauge)
        {
            _guardBreakGaugeImage[gauge].SetActive(false);
        }

        void SetGuardBreakGauge()
        {
            for (int i = 0; i < _guardBreakGaugeImage.Length; i++)
            {
                _guardBreakGaugeImage[i].SetActive(true);
            }
        }
        
    }
}

