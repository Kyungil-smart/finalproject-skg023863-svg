using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.InputSystem;

namespace MyGame
{
    public class BattleUIEffect : MonoBehaviour
    {
        [Header("KO연출")] 
        [SerializeField] private TMP_Text _kText;
        [SerializeField] private TMP_Text _oText;
        [SerializeField] private float _KOStartScale;
        [SerializeField] private float _KOEndValue;
        [SerializeField] private float _KODuration;
        [SerializeField] private float _KOFadeDuration;

        [Header("Ready연출")] [SerializeField] private TMP_Text _roundText;
        [SerializeField] private TMP_Text _readyText;
        [SerializeField] private TMP_Text _fightText;
        [SerializeField] private float _roundDuration;
        [SerializeField] private float _roundInterval;
        [SerializeField] private float _blinkInterval;
        [SerializeField] private int _blinkLoop;
        [SerializeField] private float _fightScale;
        [SerializeField] private float _fightStartDuration;
        [SerializeField] private float _fightEndDuration;
        [SerializeField] private float _fightInterval;

        [Header("Winner 표시")] 
        [SerializeField] private TMP_Text _winnerText;
        
        public void PlayKO()
        {
            _kText.transform.localScale = Vector3.one * _KOStartScale;
            _oText.transform.localScale = Vector3.one * _KOStartScale;

            Sequence _sequence = DOTween.Sequence();

            _sequence.AppendCallback(() => _kText.alpha = 1f);
            _sequence.Append(_kText.transform.DOScale(_KOEndValue, _KODuration));

            _sequence.AppendCallback(() => _oText.alpha = 1f);
            _sequence.Append(_oText.transform.DOScale(_KOEndValue, _KODuration));

            _sequence.AppendInterval(3f - (_KODuration * 2) + _KOFadeDuration);

            _sequence.Append(_kText.DOFade(0f, _KOFadeDuration));

            _sequence.Join(_oText.DOFade(0f, _KOFadeDuration));
        }

        public void PlayReady(int round = 1, bool isFinalRound = false)
        {
            if (isFinalRound)
                _roundText.text = "Final Round";
            else
                _roundText.text = $"Round {round}";

            _roundText.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            _fightText.transform.localScale = Vector3.one * 0.2f;

            Sequence _sequence = DOTween.Sequence();

            _sequence.AppendCallback(() => _roundText.alpha = 1f);
            _sequence.Append(_roundText.transform.DOLocalRotate(Vector3.zero, _roundDuration));
            _sequence.AppendInterval(0.5f);
            _sequence.AppendCallback(() => _roundText.alpha = 0f);

            Sequence blink = DOTween.Sequence();

            blink.AppendCallback(() => _readyText.alpha = 1f);
            blink.AppendInterval(_blinkInterval);
            blink.AppendCallback(() => _readyText.alpha = 0f);
            blink.AppendInterval(_blinkInterval);

            blink.SetLoops(_blinkLoop);

            _sequence.Append(blink);

            _sequence.AppendCallback(() => _fightText.alpha = 1f);

            _sequence.Append(_fightText.transform.DOScale(_fightScale, _fightStartDuration)
                .SetEase(Ease.OutBack));

            _sequence.Append(_fightText.transform.DOScale(1f, _fightEndDuration));

            _sequence.AppendInterval(_fightInterval);

            _sequence.AppendCallback(() => _fightText.alpha = 0f);
        }

        public void ShowWinner(bool isWinnerPlayerOne)
        {
            int winPlayer = isWinnerPlayerOne ? 1 : 2;  
            
            _winnerText.text = $"PLAYER {winPlayer} WIN";

            _winnerText.gameObject.SetActive(true);
        }
        
        public void HideWinner()
        {
            _winnerText.gameObject.SetActive(false);
        }
    }
}