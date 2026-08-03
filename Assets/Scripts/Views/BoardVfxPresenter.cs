using System;
using Zenject;

namespace Pipes
{
    public sealed class BoardVfxPresenter : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly ParticleVfxFactory _particleVfxFactory;
        private readonly BoardLayout _layout;
        private bool _bound;

        public BoardVfxPresenter(
            SignalBus signalBus,
            ParticleVfxFactory particleVfxFactory,
            BoardLayout layout)
        {
            _signalBus = signalBus;
            _particleVfxFactory = particleVfxFactory;
            _layout = layout;
        }

        public void Initialize()
        {
            if (_bound)
            {
                return;
            }

            _particleVfxFactory.Warmup();
            _signalBus.Subscribe<CellsDestroyedSignal>(OnCellsDestroyed);
            _bound = true;
        }

        public void Dispose()
        {
            if (!_bound)
            {
                return;
            }

            _signalBus.TryUnsubscribe<CellsDestroyedSignal>(OnCellsDestroyed);
            _bound = false;
        }

        public void PlaySpin(CellModel cell)
        {
            if (cell == null)
            {
                return;
            }

            _particleVfxFactory.Spawn(ParticleVfxId.Spin, _layout.GetWorldPosition(cell));
        }

        private void OnCellsDestroyed(CellsDestroyedSignal signal)
        {
            if (signal?.DestroyedCells == null)
            {
                return;
            }

            for (int i = 0; i < signal.DestroyedCells.Count; i++)
            {
                CellModel model = signal.DestroyedCells[i];
                if (model == null)
                {
                    continue;
                }

                _particleVfxFactory.Spawn(ParticleVfxId.Explosion, _layout.GetWorldPosition(model));
            }
        }
    }
}
