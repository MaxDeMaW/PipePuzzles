using System.Collections.Generic;

namespace Pipes
{
    public sealed class CellClickedSignal
    {
        public CellModel Model { get; }

        public CellClickedSignal(CellModel model)
        {
            Model = model;
        }
    }

    public sealed class CellsDestroyedSignal
    {
        public List<CellModel> DestroyedCells { get; }

        public CellsDestroyedSignal(List<CellModel> destroyedCells)
        {
            DestroyedCells = destroyedCells;
        }
    }

    public sealed class ScoreChangedSignal
    {
        public int Total { get; }
        public int Delta { get; }

        public ScoreChangedSignal(int total, int delta)
        {
            Total = total;
            Delta = delta;
        }
    }
}
