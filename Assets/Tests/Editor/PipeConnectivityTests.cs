using System.Collections.Generic;
using NUnit.Framework;

namespace Pipes.Tests
{
    public sealed class PipeConnectivityTests
    {
        [Test]
        public void IPipe_Rotation1_OpensEastWest()
        {
            var cell = new CellModel(0, 0, PipeType.I, rotation: 1);

            Assert.AreEqual(Direction.East | Direction.West, cell.GetRotatedConnections());
        }

        [Test]
        public void HorizontalIPipes_FormLeftToRightMatch()
        {
            CellModel[,] cells = BuildRow(
                new CellModel(0, 0, PipeType.I, 1),
                new CellModel(1, 0, PipeType.I, 1),
                new CellModel(2, 0, PipeType.I, 1));

            Assert.IsTrue(PipeConnectivity.HasLeftToRightMatch(cells, 3, 1));
            Assert.AreEqual(3, PipeConnectivity.CollectMatchedChains(cells, 3, 1).Count);
        }

        [Test]
        public void VerticalIOnLeft_DoesNotTouchLeftWall()
        {
            var cell = new CellModel(0, 0, PipeType.I, rotation: 0);
            CellModel[,] cells = BuildRow(cell);

            Assert.IsFalse(PipeConnectivity.TouchesLeftWall(cell));
            Assert.IsFalse(PipeConnectivity.HasLeftToRightMatch(cells, 1, 1));
        }

        [Test]
        public void BrokenMiddleConnection_HasNoMatch()
        {
            CellModel[,] cells = BuildRow(
                new CellModel(0, 0, PipeType.I, 1),
                new CellModel(1, 0, PipeType.I, 0),
                new CellModel(2, 0, PipeType.I, 1));

            Assert.IsFalse(PipeConnectivity.HasLeftToRightMatch(cells, 3, 1));
        }

        [Test]
        public void ReachableFromLeft_ButNotToRight_IsNotMatch()
        {
            CellModel[,] cells = BuildRow(
                new CellModel(0, 0, PipeType.I, 1),
                new CellModel(1, 0, PipeType.L, 2));

            PipeConnectivity.FlowAnalysis flow = PipeConnectivity.AnalyzeFromLeft(cells, 2, 1);

            Assert.AreEqual(2, flow.ReachableFromLeft.Count);
            Assert.AreEqual(0, flow.MatchedLeftToRight.Count);
            Assert.IsFalse(PipeConnectivity.HasLeftToRightMatch(cells, 2, 1));
        }

        [Test]
        public void XPipes_FormLeftToRightMatch()
        {
            CellModel[,] cells = BuildRow(
                new CellModel(0, 0, PipeType.X, 0),
                new CellModel(1, 0, PipeType.X, 0),
                new CellModel(2, 0, PipeType.X, 0));

            Assert.IsTrue(PipeConnectivity.HasLeftToRightMatch(cells, 3, 1));
        }

        [Test]
        public void NullGap_BreaksConnection()
        {
            var cells = new CellModel[3, 1];
            cells[0, 0] = new CellModel(0, 0, PipeType.I, 1);
            cells[1, 0] = null;
            cells[2, 0] = new CellModel(2, 0, PipeType.I, 1);

            Assert.IsFalse(PipeConnectivity.HasLeftToRightMatch(cells, 3, 1));
        }

        [Test]
        public void OneWayPort_DoesNotConnectWithoutMutualOpening()
        {
            CellModel[,] cells = BuildRow(
                new CellModel(0, 0, PipeType.I, 1),
                new CellModel(1, 0, PipeType.L, 1));

            PipeConnectivity.FlowAnalysis flow = PipeConnectivity.AnalyzeFromLeft(cells, 2, 1);

            Assert.AreEqual(1, flow.ReachableFromLeft.Count);
            Assert.IsFalse(PipeConnectivity.HasLeftToRightMatch(cells, 2, 1));
        }

        [Test]
        public void TPipe_Rotated_CanBridgeLeftToRight()
        {
            // I (E|W) + T rot3 (W|N|E) → mutual West/East and right wall touch.
            CellModel[,] cells = BuildRow(
                new CellModel(0, 0, PipeType.I, 1),
                new CellModel(1, 0, PipeType.T, 3));

            Assert.IsTrue(PipeConnectivity.HasLeftToRightMatch(cells, 2, 1));
        }

        [Test]
        public void TwoRows_OnlyMatchingComponentIsCollected()
        {
            var cells = new CellModel[3, 2];
            cells[0, 0] = new CellModel(0, 0, PipeType.I, 1);
            cells[1, 0] = new CellModel(1, 0, PipeType.I, 1);
            cells[2, 0] = new CellModel(2, 0, PipeType.I, 1);
            cells[0, 1] = new CellModel(0, 1, PipeType.I, 0);
            cells[1, 1] = new CellModel(1, 1, PipeType.I, 0);
            cells[2, 1] = new CellModel(2, 1, PipeType.I, 0);

            List<CellModel> matched = PipeConnectivity.CollectMatchedChains(cells, 3, 2);

            Assert.AreEqual(3, matched.Count);
            Assert.IsTrue(matched.TrueForAll(c => c.Y == 0));
        }

        private static CellModel[,] BuildRow(params CellModel[] row)
        {
            var cells = new CellModel[row.Length, 1];
            for (int x = 0; x < row.Length; x++)
            {
                cells[x, 0] = row[x];
            }

            return cells;
        }
    }
}
