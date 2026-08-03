namespace Pipes
{
    public sealed class CellModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public PipeType Type { get; set; }
        public int CurrentRotation { get; set; }
        public bool IsConnectedFromLeft { get; set; }
        public bool IsWaterFilled { get; set; }

        public CellModel(int x, int y, PipeType type, int rotation)
        {
            X = x;
            Y = y;
            Type = type;
            CurrentRotation = rotation & 3;
            IsConnectedFromLeft = false;
            IsWaterFilled = false;
        }

        public Direction GetRotatedConnections()
        {
            Direction connections = GetBaseConnections(Type);

            for (int i = 0; i < CurrentRotation; i++)
            {
                connections = RotateClockwise(connections);
            }

            return connections;
        }

        public static Direction GetBaseConnections(PipeType type)
        {
            switch (type)
            {
                // Straight vertical: opens at top (North) and bottom (South) of the cell.
                // Rotation 1/3 turns it horizontal (East-West).
                case PipeType.I:
                    return Direction.North | Direction.South;
                case PipeType.L:
                    return Direction.North | Direction.East;
                case PipeType.T:
                    return Direction.North | Direction.East | Direction.South;
                case PipeType.X:
                    return Direction.North | Direction.East | Direction.South | Direction.West;
                default:
                    return Direction.None;
            }
        }

        public static Direction RotateClockwise(Direction directions)
        {
            Direction result = Direction.None;

            if ((directions & Direction.North) != 0)
            {
                result |= Direction.East;
            }

            if ((directions & Direction.East) != 0)
            {
                result |= Direction.South;
            }

            if ((directions & Direction.South) != 0)
            {
                result |= Direction.West;
            }

            if ((directions & Direction.West) != 0)
            {
                result |= Direction.North;
            }

            return result;
        }

        public static Direction GetOpposite(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Direction.South;
                case Direction.East:
                    return Direction.West;
                case Direction.South:
                    return Direction.North;
                case Direction.West:
                    return Direction.East;
                default:
                    return Direction.None;
            }
        }
    }
}
