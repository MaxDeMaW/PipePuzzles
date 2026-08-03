using System.Collections.Generic;
using NUnit.Framework;

namespace Pipes.Tests
{
    public sealed class GridModelTests
    {
        [Test]
        public void DestroyAndCollapse_DropsCellsDownAndRefills()
        {
            DifficultyProfile profile = TestDifficultyProfiles.Create(columns: 1, rows: 3, seed: 7);
            var generator = new InitialGridGenerator(profile);
            var grid = new GridModel(profile, generator);

            CellModel bottom = new CellModel(0, 0, PipeType.I, 0);
            CellModel middle = new CellModel(0, 1, PipeType.L, 0);
            CellModel top = new CellModel(0, 2, PipeType.T, 0);
            grid.Cells[0, 0] = bottom;
            grid.Cells[0, 1] = middle;
            grid.Cells[0, 2] = top;

            List<DropData> drops = grid.DestroyAndCollapse(new List<CellModel> { bottom });

            Assert.AreSame(middle, grid.GetCell(0, 0));
            Assert.AreEqual(0, middle.Y);
            Assert.AreSame(top, grid.GetCell(0, 1));
            Assert.AreEqual(1, top.Y);
            Assert.IsNotNull(grid.GetCell(0, 2));
            Assert.AreNotSame(bottom, grid.GetCell(0, 2));

            Assert.That(drops.Exists(d => d.Model == middle && d.FromY == 1 && d.ToY == 0));
            Assert.That(drops.Exists(d => d.Model == top && d.FromY == 2 && d.ToY == 1));
            Assert.That(drops.Exists(d => d.FromY >= 3 && d.ToY == 2));
        }

        [Test]
        public void DestroyAndCollapse_EmptyList_ReturnsNoDrops()
        {
            DifficultyProfile profile = TestDifficultyProfiles.Create(columns: 2, rows: 2, seed: 1);
            var grid = new GridModel(profile, new InitialGridGenerator(profile));
            grid.Cells[0, 0] = new CellModel(0, 0, PipeType.X, 0);

            List<DropData> drops = grid.DestroyAndCollapse(new List<CellModel>());

            Assert.AreEqual(0, drops.Count);
            Assert.IsNotNull(grid.GetCell(0, 0));
        }

        [Test]
        public void DestroyAndCollapse_RemovesMultipleAndKeepsColumnFull()
        {
            DifficultyProfile profile = TestDifficultyProfiles.Create(columns: 1, rows: 4, seed: 2);
            var grid = new GridModel(profile, new InitialGridGenerator(profile));

            CellModel c0 = new CellModel(0, 0, PipeType.I, 0);
            CellModel c1 = new CellModel(0, 1, PipeType.L, 0);
            CellModel c2 = new CellModel(0, 2, PipeType.T, 0);
            CellModel c3 = new CellModel(0, 3, PipeType.X, 0);
            grid.Cells[0, 0] = c0;
            grid.Cells[0, 1] = c1;
            grid.Cells[0, 2] = c2;
            grid.Cells[0, 3] = c3;

            grid.DestroyAndCollapse(new List<CellModel> { c1, c2 });

            Assert.AreSame(c0, grid.GetCell(0, 0));
            Assert.AreSame(c3, grid.GetCell(0, 1));
            Assert.AreEqual(1, c3.Y);
            Assert.IsNotNull(grid.GetCell(0, 2));
            Assert.IsNotNull(grid.GetCell(0, 3));
            Assert.AreNotSame(c1, grid.GetCell(0, 2));
            Assert.AreNotSame(c2, grid.GetCell(0, 3));
        }

        [Test]
        public void RefreshFlowFromLeft_MarksReachableCells()
        {
            DifficultyProfile profile = TestDifficultyProfiles.Create(columns: 3, rows: 1, seed: 3);
            var grid = new GridModel(profile, new InitialGridGenerator(profile));

            grid.Cells[0, 0] = new CellModel(0, 0, PipeType.I, 1);
            grid.Cells[1, 0] = new CellModel(1, 0, PipeType.I, 1);
            grid.Cells[2, 0] = new CellModel(2, 0, PipeType.I, 1);

            List<CellModel> matched = grid.RefreshFlowFromLeft();

            Assert.AreEqual(3, matched.Count);
            Assert.IsTrue(grid.GetCell(0, 0).IsConnectedFromLeft);
            Assert.IsTrue(grid.GetCell(1, 0).IsConnectedFromLeft);
            Assert.IsTrue(grid.GetCell(2, 0).IsConnectedFromLeft);
        }

        [Test]
        public void ClearFlowVisuals_ResetsWaterAndConnectionFlags()
        {
            DifficultyProfile profile = TestDifficultyProfiles.Create(columns: 2, rows: 1, seed: 4);
            var grid = new GridModel(profile, new InitialGridGenerator(profile));
            grid.Cells[0, 0] = new CellModel(0, 0, PipeType.I, 1);
            grid.Cells[1, 0] = new CellModel(1, 0, PipeType.I, 1);

            grid.RefreshFlowFromLeft();
            grid.MarkWaterFilled(grid.GetCell(0, 0));
            Assert.IsTrue(grid.GetCell(0, 0).IsWaterFilled);
            Assert.IsTrue(grid.GetCell(0, 0).IsConnectedFromLeft);

            grid.ClearFlowVisuals();

            Assert.IsFalse(grid.GetCell(0, 0).IsWaterFilled);
            Assert.IsFalse(grid.GetCell(0, 0).IsConnectedFromLeft);
            Assert.IsFalse(grid.GetCell(1, 0).IsConnectedFromLeft);
        }

        [Test]
        public void GetCell_OutOfBounds_ReturnsNull()
        {
            DifficultyProfile profile = TestDifficultyProfiles.Create(columns: 2, rows: 2, seed: 5);
            var grid = new GridModel(profile, new InitialGridGenerator(profile));

            Assert.IsNull(grid.GetCell(-1, 0));
            Assert.IsNull(grid.GetCell(0, -1));
            Assert.IsNull(grid.GetCell(2, 0));
            Assert.IsNull(grid.GetCell(0, 2));
        }

        [Test]
        public void InitializeGrid_WithFixedSeed_HasNoLeftToRightMatch()
        {
            DifficultyProfile profile = TestDifficultyProfiles.Create(columns: 5, rows: 6, seed: 1001);
            var grid = new GridModel(profile, new InitialGridGenerator(profile));

            grid.InitializeGrid();

            Assert.IsFalse(PipeConnectivity.HasLeftToRightMatch(grid.Cells, grid.Columns, grid.Rows));
        }

        [Test]
        public void MarkWaterFilled_Null_DoesNothing()
        {
            DifficultyProfile profile = TestDifficultyProfiles.Create(columns: 1, rows: 1, seed: 6);
            var grid = new GridModel(profile, new InitialGridGenerator(profile));

            Assert.DoesNotThrow(() => grid.MarkWaterFilled(null));
        }
    }
}
