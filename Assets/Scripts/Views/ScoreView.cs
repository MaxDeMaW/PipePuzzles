using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Pipes
{
    public sealed class ScoreView : MonoBehaviour
    {
        [FormerlySerializedAs("scoreText")]
        [SerializeField] private TMP_Text _scoreText;

        private SignalBus _signalBus;
        private GameAnimationSetup _animationSetup;
        private int _displayedScore;
        private Vector3 _baseScale = Vector3.one;
        private Tween _countTween;
        private Tween _punchTween;

        [Inject]
        public void Construct(
            SignalBus signalBus,
            IScoreService scoreService,
            GameAnimationSetup animationSetup)
        {
            _signalBus = signalBus;
            _animationSetup = animationSetup;
            SetScoreInstant(scoreService != null ? scoreService.Total : 0);
        }

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void Start()
        {
            if (_signalBus != null)
            {
                _signalBus.Subscribe<ScoreChangedSignal>(OnScoreChanged);
            }
        }

        private void OnDestroy()
        {
            _countTween?.Kill();
            _punchTween?.Kill();
            transform.DOKill();

            if (_signalBus != null)
            {
                _signalBus.TryUnsubscribe<ScoreChangedSignal>(OnScoreChanged);
            }
        }

        private void OnScoreChanged(ScoreChangedSignal signal)
        {
            if (signal == null)
            {
                return;
            }

            if (signal.Delta == 0 || signal.Total == _displayedScore)
            {
                SetScoreInstant(signal.Total);
                return;
            }

            AnimateScoreChange(signal.Total);
        }

        private void AnimateScoreChange(int targetTotal)
        {
            float countDuration = _animationSetup != null ? _animationSetup.ScoreCountDuration : 0.45f;
            float punchDuration = _animationSetup != null ? _animationSetup.ScorePunchDuration : 0.22f;
            float punchScale = _animationSetup != null ? _animationSetup.ScorePunchScale : 1.35f;

            _countTween?.Kill();
            float from = _displayedScore;

            _countTween = DOTween.To(
                    () => from,
                    value =>
                    {
                        from = value;
                        _displayedScore = Mathf.RoundToInt(value);
                        if (_scoreText != null)
                        {
                            _scoreText.text = _displayedScore.ToString();
                        }
                    },
                    targetTotal,
                    countDuration)
                .SetEase(Ease.OutCubic)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    _displayedScore = targetTotal;
                    if (_scoreText != null)
                    {
                        _scoreText.text = targetTotal.ToString();
                    }
                });

            _punchTween?.Kill();
            transform.localScale = _baseScale;

            float half = punchDuration * 0.5f;
            _punchTween = DOTween.Sequence()
                .SetTarget(transform)
                .SetLink(gameObject)
                .Append(transform.DOScale(_baseScale * punchScale, half).SetEase(Ease.OutQuad))
                .Append(transform.DOScale(_baseScale, half).SetEase(Ease.InQuad))
                .OnKill(() => transform.localScale = _baseScale);
        }

        private void SetScoreInstant(int total)
        {
            _countTween?.Kill();
            _punchTween?.Kill();

            _displayedScore = total;
            transform.localScale = _baseScale;

            if (_scoreText != null)
            {
                _scoreText.text = total.ToString();
            }
        }
    }
}
