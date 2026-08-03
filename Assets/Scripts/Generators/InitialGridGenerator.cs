using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Random = System.Random;

namespace Pipes
{
    public sealed class InitialGridGenerator
    {
        private const int MaxFullRegenerateAttempts = 40;
        private const int MaxBreakAttempts = 64;

        private readonly DifficultyProfile _difficulty;
        private readonly Random _random;

        public int UsedSeed { get; }

        [Inject]
        public InitialGridGenerator(DifficultyProfile difficulty)
        {
            _difficulty = difficulty;
            UsedSeed = _difficulty.ResolveSeed();
            _random = new Random(UsedSeed);
        }

        public CellModel CreateRandomCell(int x, int y)
        {
            PipeType type = _difficulty.RollPipeType(_random);
            int rotation = _random.Next(0, 4);
            return new CellModel(x, y, type, rotation);
        }

        public void FillWithoutMatches(CellModel[,] cells, int columns, int rows)
        {
            if (cells == null)
            {
                Debug.LogError("InitialGridGenerator: cells array is null.");
                return;
            }

            for (int attempt = 0; attempt < MaxFullRegenerateAttempts; attempt++)
            {
                FillRandom(cells, columns, rows);
                if (!PipeConnectivity.HasLeftToRightMatch(cells, columns, rows))
                {
                    return;
                }
            }

            FillRandom(cells, columns, rows);
            BreakLeftToRightMatches(cells, columns, rows);

            if (PipeConnectivity.HasLeftToRightMatch(cells, columns, rows))
            {
                Debug.LogError(
                    "InitialGridGenerator: could not clear left-to-right match after " +
                    $"{MaxFullRegenerateAttempts} regenerations and {MaxBreakAttempts} break attempts " +
                    $"(seed={UsedSeed}, size={columns}x{rows}).");
            }
        }

        private void FillRandom(CellModel[,] cells, int columns, int rows)
        {
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    cells[x, y] = CreateRandomCell(x, y);
                }
            }
        }

        private void BreakLeftToRightMatches(CellModel[,] cells, int columns, int rows)
        {
            for (int attempt = 0; attempt < MaxBreakAttempts; attempt++)
            {
                List<CellModel> matched = PipeConnectivity.CollectMatchedChains(cells, columns, rows);
                if (matched.Count == 0)
                {
                    return;
                }

                CellModel target = matched[_random.Next(matched.Count)];
                cells[target.X, target.Y] = CreateRandomCell(target.X, target.Y);
            }
        }
    }
}
