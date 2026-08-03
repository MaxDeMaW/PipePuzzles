using System.Collections.Generic;

namespace Pipes
{
    public static class PipeConnectivity
    {
        private static readonly Direction[] CardinalDirections =
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        };

        private static readonly (int dx, int dy)[] DirectionOffsets =
        {
            (0, 1),
            (1, 0),
            (0, -1),
            (-1, 0)
        };

        public readonly struct FlowAnalysis
        {
            public List<CellModel> ReachableFromLeft { get; }
            public List<CellModel> MatchedLeftToRight { get; }

            public FlowAnalysis(List<CellModel> reachableFromLeft, List<CellModel> matchedLeftToRight)
            {
                ReachableFromLeft = reachableFromLeft;
                MatchedLeftToRight = matchedLeftToRight;
            }
        }

        public static FlowAnalysis AnalyzeFromLeft(CellModel[,] cells, int columns, int rows)
        {
            bool[,] visited = new bool[columns, rows];
            List<CellModel> reachable = new List<CellModel>();
            List<CellModel> matched = new List<CellModel>();

            for (int y = 0; y < rows; y++)
            {
                CellModel seed = cells[0, y];
                if (seed == null || visited[0, y] || !TouchesLeftWall(seed))
                {
                    continue;
                }

                List<CellModel> component = CollectConnectedComponent(cells, columns, rows, 0, y, visited);
                reachable.AddRange(component);

                if (TouchesRightWall(component, columns))
                {
                    matched.AddRange(component);
                }
            }

            return new FlowAnalysis(reachable, matched);
        }

        public static List<CellModel> CollectMatchedChains(CellModel[,] cells, int columns, int rows)
        {
            return AnalyzeFromLeft(cells, columns, rows).MatchedLeftToRight;
        }

        public static bool HasLeftToRightMatch(CellModel[,] cells, int columns, int rows)
        {
            return CollectMatchedChains(cells, columns, rows).Count > 0;
        }

        public static bool TouchesLeftWall(CellModel cell)
        {
            return cell != null
                   && cell.X == 0
                   && (cell.GetRotatedConnections() & Direction.West) != 0;
        }

        public static bool TouchesRightWall(List<CellModel> component, int columns)
        {
            if (component == null || component.Count == 0)
            {
                return false;
            }

            int rightEdge = columns - 1;
            for (int i = 0; i < component.Count; i++)
            {
                CellModel cell = component[i];
                if (cell.X == rightEdge && (cell.GetRotatedConnections() & Direction.East) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static List<CellModel> CollectConnectedComponent(
            CellModel[,] cells,
            int columns,
            int rows,
            int startX,
            int startY,
            bool[,] visited)
        {
            List<CellModel> component = new List<CellModel>();
            Queue<CellModel> queue = new Queue<CellModel>();

            CellModel start = cells[startX, startY];
            visited[startX, startY] = true;
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                CellModel current = queue.Dequeue();
                component.Add(current);

                Direction connections = current.GetRotatedConnections();

                for (int i = 0; i < CardinalDirections.Length; i++)
                {
                    Direction dir = CardinalDirections[i];
                    if ((connections & dir) == 0)
                    {
                        continue;
                    }

                    int nx = current.X + DirectionOffsets[i].dx;
                    int ny = current.Y + DirectionOffsets[i].dy;

                    if (!IsInside(nx, ny, columns, rows) || visited[nx, ny])
                    {
                        continue;
                    }

                    CellModel neighbor = cells[nx, ny];
                    if (neighbor == null)
                    {
                        continue;
                    }

                    Direction opposite = CellModel.GetOpposite(dir);
                    if ((neighbor.GetRotatedConnections() & opposite) == 0)
                    {
                        continue;
                    }

                    visited[nx, ny] = true;
                    queue.Enqueue(neighbor);
                }
            }

            return component;
        }

        private static bool IsInside(int x, int y, int columns, int rows)
        {
            return x >= 0 && x < columns && y >= 0 && y < rows;
        }
    }
}
