using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Zenject;

namespace Pipes
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class CellView : MonoBehaviour, IPointerClickHandler
    {
        [FormerlySerializedAs("spriteRenderer")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private CellModel _model;
        private SignalBus _signalBus;
        private GameAnimationSetup _animationSetup;
        private float _cellSize = 1f;
        private Vector3 _gridOrigin;
        private Vector3 _visualBaseScale = Vector3.one;
        private Tween _rotatePunchTween;
        private int _fallGeneration;

        public CellModel Model => _model;

        private Transform VisualTransform =>
            _spriteRenderer != null ? _spriteRenderer.transform : transform;

        [Inject]
        public void Construct(SignalBus signalBus, GameAnimationSetup animationSetup)
        {
            _signalBus = signalBus;
            _animationSetup = animationSetup;
        }

        public void Init(CellModel model)
        {
            _model = model;
            _visualBaseScale = VisualTransform.localScale;
            UpdateVisual();
        }

        public void PrepareForPool()
        {
            _fallGeneration++;

            _rotatePunchTween?.Kill();
            _rotatePunchTween = null;

            Transform visual = VisualTransform;
            visual.DOKill();
            visual.localScale = _visualBaseScale;
            visual.localRotation = Quaternion.identity;
            transform.localRotation = Quaternion.identity;

            _model = null;
        }

        // LeanPool SendMessage hooks.
        private void OnSpawn()
        {
        }

        private void OnDespawn()
        {
            PrepareForPool();
        }

        public void ConfigureLayout(float cellSize, Vector3 gridOrigin)
        {
            _cellSize = cellSize;
            _gridOrigin = gridOrigin;
        }

        public void UpdateVisual()
        {
            if (_model == null)
            {
                return;
            }

            // Keep root upright (collider / bg); rotate only the pipe sprite.
            transform.localRotation = Quaternion.identity;
            VisualTransform.localRotation = Quaternion.Euler(0f, 0f, -_model.CurrentRotation * 90f);

            if (_spriteRenderer != null && _animationSetup != null)
            {
                if (_model.IsWaterFilled)
                {
                    _spriteRenderer.color = _animationSetup.FilledColor;
                }
                else if (_model.IsConnectedFromLeft)
                {
                    _spriteRenderer.color = _animationSetup.ConnectedColor;
                }
                else
                {
                    _spriteRenderer.color = _animationSetup.NormalColor;
                }
            }
        }

        public void PlayRotatePunch()
        {
            if (_animationSetup == null)
            {
                return;
            }

            Transform visual = VisualTransform;
            _rotatePunchTween?.Kill();
            visual.localScale = _visualBaseScale;

            float halfDuration = _animationSetup.RotatePunchDuration * 0.5f;
            Vector3 peakScale = _visualBaseScale * _animationSetup.RotatePunchScale;

            _rotatePunchTween = DOTween.Sequence()
                .SetTarget(visual)
                .SetLink(gameObject)
                .Append(visual.DOScale(peakScale, halfDuration).SetEase(Ease.OutQuad))
                .Append(visual.DOScale(_visualBaseScale, halfDuration).SetEase(Ease.InQuad))
                .OnKill(() => visual.localScale = _visualBaseScale);
        }

        public async UniTask FallTo(
            int targetY,
            float duration,
            CancellationToken cancellationToken = default)
        {
            int generation = ++_fallGeneration;

            Vector3 target = _gridOrigin + new Vector3(_model.X * _cellSize, targetY * _cellSize, 0f);
            Vector3 start = transform.localPosition;
            float safeDuration = Mathf.Max(0.01f, duration);
            float elapsed = 0f;

            while (elapsed < safeDuration)
            {
                if (generation != _fallGeneration)
                {
                    throw new System.OperationCanceledException();
                }

                cancellationToken.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / safeDuration);
                t = t * t * (3f - 2f * t);
                transform.localPosition = Vector3.Lerp(start, target, t);

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            if (generation != _fallGeneration)
            {
                throw new System.OperationCanceledException();
            }

            transform.localPosition = target;
        }

        public void SetVisualRow(int y)
        {
            transform.localPosition = _gridOrigin + new Vector3(_model.X * _cellSize, y * _cellSize, 0f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_model == null || _signalBus == null)
            {
                return;
            }

            _signalBus.Fire(new CellClickedSignal(_model));
        }

        private void OnDestroy()
        {
            _fallGeneration++;
            _rotatePunchTween?.Kill();
            VisualTransform.DOKill();
        }
    }
}
