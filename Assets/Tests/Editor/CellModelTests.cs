using NUnit.Framework;

namespace Pipes.Tests
{
    public sealed class CellModelTests
    {
        [Test]
        public void GetBaseConnections_MatchesPipeShapes()
        {
            Assert.AreEqual(Direction.North | Direction.South, CellModel.GetBaseConnections(PipeType.I));
            Assert.AreEqual(Direction.North | Direction.East, CellModel.GetBaseConnections(PipeType.L));
            Assert.AreEqual(
                Direction.North | Direction.East | Direction.South,
                CellModel.GetBaseConnections(PipeType.T));
            Assert.AreEqual(
                Direction.North | Direction.East | Direction.South | Direction.West,
                CellModel.GetBaseConnections(PipeType.X));
        }

        [Test]
        public void Constructor_MasksRotationToTwoBits()
        {
            var cell = new CellModel(0, 0, PipeType.I, rotation: 5);

            Assert.AreEqual(1, cell.CurrentRotation);
        }

        [Test]
        public void RotateClockwise_FourTimes_ReturnsOriginal()
        {
            Direction original = Direction.North | Direction.East;
            Direction rotated = original;

            for (int i = 0; i < 4; i++)
            {
                rotated = CellModel.RotateClockwise(rotated);
            }

            Assert.AreEqual(original, rotated);
        }

        [Test]
        public void GetOpposite_ReturnsCardinalPairs()
        {
            Assert.AreEqual(Direction.South, CellModel.GetOpposite(Direction.North));
            Assert.AreEqual(Direction.West, CellModel.GetOpposite(Direction.East));
            Assert.AreEqual(Direction.North, CellModel.GetOpposite(Direction.South));
            Assert.AreEqual(Direction.East, CellModel.GetOpposite(Direction.West));
            Assert.AreEqual(Direction.None, CellModel.GetOpposite(Direction.None));
        }

        [Test]
        public void LPipe_Rotations_CycleExpectedPorts()
        {
            Assert.AreEqual(
                Direction.North | Direction.East,
                new CellModel(0, 0, PipeType.L, 0).GetRotatedConnections());
            Assert.AreEqual(
                Direction.East | Direction.South,
                new CellModel(0, 0, PipeType.L, 1).GetRotatedConnections());
            Assert.AreEqual(
                Direction.South | Direction.West,
                new CellModel(0, 0, PipeType.L, 2).GetRotatedConnections());
            Assert.AreEqual(
                Direction.West | Direction.North,
                new CellModel(0, 0, PipeType.L, 3).GetRotatedConnections());
        }

        [Test]
        public void IPipe_Rotations_CycleExpectedPorts()
        {
            Assert.AreEqual(
                Direction.North | Direction.South,
                new CellModel(0, 0, PipeType.I, 0).GetRotatedConnections());
            Assert.AreEqual(
                Direction.East | Direction.West,
                new CellModel(0, 0, PipeType.I, 1).GetRotatedConnections());
            Assert.AreEqual(
                Direction.North | Direction.South,
                new CellModel(0, 0, PipeType.I, 2).GetRotatedConnections());
            Assert.AreEqual(
                Direction.East | Direction.West,
                new CellModel(0, 0, PipeType.I, 3).GetRotatedConnections());
        }

        [Test]
        public void XPipe_AllRotations_KeepFourPorts()
        {
            Direction all = Direction.North | Direction.East | Direction.South | Direction.West;
            for (int rotation = 0; rotation < 4; rotation++)
            {
                Assert.AreEqual(all, new CellModel(0, 0, PipeType.X, rotation).GetRotatedConnections());
            }
        }
    }
}
