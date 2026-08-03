using System.Collections.Generic;
using NUnit.Framework;
using Random = System.Random;

namespace Pipes.Tests
{
    public sealed class InitialGridGeneratorTests
    {
        [Test]
        public void FillWithoutMatches_LeavesNoLeftToRightMatch()
        {
            DifficultyProfile profile = TestDifficultyProfiles.Create(5, 6, seed: 4242);
            var generator = new InitialGridGenerator(profile);
            var cells = new CellModel[profile.Columns, profile.Rows];

            generator.FillWithoutMatches(cells, profile.Columns, profile.Rows);

            Assert.IsFalse(PipeConnectivity.HasLeftToRightMatch(cells, profile.Columns, profile.Rows));
            Assert.AreEqual(4242, generator.UsedSeed);
        }

        [Test]
        public void FillWithoutMatches_FillsEveryCell()
        {
            DifficultyProfile profile = TestDifficultyProfiles.Create(4, 4, seed: 11);
            var generator = new InitialGridGenerator(profile);
            var cells = new CellModel[4, 4];

            generator.FillWithoutMatches(cells, 4, 4);

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    Assert.IsNotNull(cells[x, y]);
                    Assert.AreEqual(x, cells[x, y].X);
                    Assert.AreEqual(y, cells[x, y].Y);
                }
            }
        }

        [Test]
        public void FixedSeed_ProducesDeterministicBoard()
        {
            DifficultyProfile profileA = TestDifficultyProfiles.Create(5, 5, seed: 777);
            DifficultyProfile profileB = TestDifficultyProfiles.Create(5, 5, seed: 777);
            var generatorA = new InitialGridGenerator(profileA);
            var generatorB = new InitialGridGenerator(profileB);
            var cellsA = new CellModel[5, 5];
            var cellsB = new CellModel[5, 5];

            generatorA.FillWithoutMatches(cellsA, 5, 5);
            generatorB.FillWithoutMatches(cellsB, 5, 5);

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    Assert.AreEqual(cellsA[x, y].Type, cellsB[x, y].Type);
                    Assert.AreEqual(cellsA[x, y].CurrentRotation, cellsB[x, y].CurrentRotation);
                }
            }
        }

        [Test]
        public void CreateRandomCell_UsesOnlyWeightedType()
        {
            DifficultyProfile profile = TestDifficultyProfiles.CreateWithWeights(
                3,
                3,
                seed: 9,
                new PipeSpawnWeight { Type = PipeType.T, Weight = 1f });
            var generator = new InitialGridGenerator(profile);

            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(PipeType.T, generator.CreateRandomCell(0, 0).Type);
            }
        }
    }

    public sealed class DifficultyProfileTests
    {
        [Test]
        public void ResolveSeed_UsesFixedSeedWhenEnabled()
        {
            DifficultyProfile profile = TestDifficultyProfiles.Create(3, 3, seed: 55);

            Assert.AreEqual(55, profile.ResolveSeed());
        }

        [Test]
        public void ColumnsAndRows_ClampToAtLeastOne()
        {
            DifficultyProfile profile = TestDifficultyProfiles.Create(0, -2, seed: 1);

            Assert.AreEqual(1, profile.Columns);
            Assert.AreEqual(1, profile.Rows);
        }

        [Test]
        public void RollPipeType_RespectsSingleNonZeroWeight()
        {
            DifficultyProfile profile = TestDifficultyProfiles.CreateWithWeights(
                2,
                2,
                seed: 1,
                new PipeSpawnWeight { Type = PipeType.I, Weight = 0f },
                new PipeSpawnWeight { Type = PipeType.L, Weight = 1f },
                new PipeSpawnWeight { Type = PipeType.T, Weight = 0f },
                new PipeSpawnWeight { Type = PipeType.X, Weight = 0f });

            var random = new Random(123);
            for (int i = 0; i < 30; i++)
            {
                Assert.AreEqual(PipeType.L, profile.RollPipeType(random));
            }
        }
    }
}
