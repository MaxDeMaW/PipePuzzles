using System.Collections.Generic;
using Zenject;

namespace Pipes
{
    public sealed class GridModel
    {
        private readonly DifficultyProfile _difficulty;
        private readonly InitialGridGenerator _gridGenerator;
        private readonly CellModel[,] _cells;

        public int Columns => _difficulty.Columns;
        public int Rows => _difficulty.Rows;
        public CellModel[,] Cells => _cells;

        [Inject]
        public GridModel(DifficultyProfile difficulty, InitialGridGenerator gridGenerator)
        {
            _difficulty = difficulty;
            _gridGenerator = gridGenerator;
            _cells = new CellModel[Columns, Rows];
        }

        public void InitializeGrid()
        {
            _gridGenerator.FillWithoutMatches(_cells, Columns, Rows);
        }

        public CellModel GetCell(int x, int y)
        {
            if (!IsInside(x, y))
            {
                return null;
            }

            return _cells[x, y];
        }

        public List<CellModel> RefreshFlowFromLeft()
        {
            ClearFlowVisuals();

            PipeConnectivity.FlowAnalysis flow = PipeConnectivity.AnalyzeFromLeft(_cells, Columns, Rows);
            SetConnectedFromLeft(flow.ReachableFromLeft, true);
            return flow.MatchedLeftToRight;
        }

        public List<CellModel> CollectMatchedChainsOrdered()
        {
            return PipeConnectivity.CollectMatchedChains(_cells, Columns, Rows);
        }

        public void MarkWaterFilled(CellModel cell)
        {
            if (cell == null)
            {
                return;
            }

            cell.IsWaterFilled = true;
        }

        public List<DropData> DestroyAndCollapse(List<CellModel> matched)
        {
            if (matched == null || matched.Count == 0)
            {
                return new List<DropData>();
            }

            RemoveCells(matched);
            return CollapseAndRefill();
        }

        public void ClearFlowVisuals()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    CellModel cell = _cells[x, y];
                    if (cell != null)
                    {
                        cell.IsConnectedFromLeft = false;
                        cell.IsWaterFilled = false;
                    }
                }
            }
        }

        private static void SetConnectedFromLeft(List<CellModel> cells, bool connected)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].IsConnectedFromLeft = connected;
            }
        }

        private void RemoveCells(List<CellModel> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                CellModel cell = cells[i];
                if (IsInside(cell.X, cell.Y) && _cells[cell.X, cell.Y] == cell)
                {
                    _cells[cell.X, cell.Y] = null;
                }
            }
        }

        private List<DropData> CollapseAndRefill()
        {
            List<DropData> drops = new List<DropData>();
            List<CellModel> columnBuffer = new List<CellModel>(Rows);

            for (int x = 0; x < Columns; x++)
            {
                columnBuffer.Clear();

                for (int y = 0; y < Rows; y++)
                {
                    CellModel cell = _cells[x, y];
                    if (cell != null)
                    {
                        columnBuffer.Add(cell);
                    }

                    _cells[x, y] = null;
                }

                for (int i = 0; i < columnBuffer.Count; i++)
                {
                    CellModel cell = columnBuffer[i];
                    int fromY = cell.Y;
                    cell.Y = i;
                    _cells[x, i] = cell;

                    if (fromY != i)
                    {
                        drops.Add(new DropData(cell, fromY, i, x));
                    }
                }

                int spawnOffset = 0;
                for (int y = columnBuffer.Count; y < Rows; y++)
                {
                    int fromY = Rows + spawnOffset;
                    CellModel cell = CreateRandomCell(x, y);
                    _cells[x, y] = cell;
                    drops.Add(new DropData(cell, fromY, y, x));
                    spawnOffset++;
                }
            }

            return drops;
        }

        private CellModel CreateRandomCell(int x, int y)
        {
            return _gridGenerator.CreateRandomCell(x, y);
        }

        private bool IsInside(int x, int y)
        {
            return x >= 0 && x < Columns && y >= 0 && y < Rows;
        }
    }
}
