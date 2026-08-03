using System;
using Zenject;

namespace Pipes
{
    public interface IScoreService
    {
        int Total { get; }
    }

    public sealed class ScoreService : IScoreService, IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly DifficultyProfile _difficulty;

        public int Total { get; private set; }

        public ScoreService(SignalBus signalBus, DifficultyProfile difficulty)
        {
            _signalBus = signalBus;
            _difficulty = difficulty;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<CellsDestroyedSignal>(OnCellsDestroyed);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<CellsDestroyedSignal>(OnCellsDestroyed);
        }

        private void OnCellsDestroyed(CellsDestroyedSignal signal)
        {
            if (signal?.DestroyedCells == null || signal.DestroyedCells.Count == 0)
            {
                return;
            }

            int scorePerPipe = _difficulty != null ? _difficulty.ScorePerPipe : 0;
            int delta = signal.DestroyedCells.Count * scorePerPipe;
            if (delta == 0)
            {
                return;
            }

            Total += delta;
            _signalBus.Fire(new ScoreChangedSignal(Total, delta));
        }
    }
}
