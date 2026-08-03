using UnityEngine;
using Zenject;

namespace Pipes
{
    public sealed class BoardLayout
    {
        private readonly GridModel _gridModel;
        private readonly Transform _gridRoot;
        private readonly float _cellSize;

        public float CellSize => _cellSize;

        public BoardLayout(
            GridModel gridModel,
            [Inject(Id = "GridRoot")] Transform gridRoot,
            [Inject(Id = "CellSize")] float cellSize)
        {
            _gridModel = gridModel;
            _gridRoot = gridRoot;
            _cellSize = cellSize;
        }

        public Vector3 GetGridOrigin()
        {
            float offsetX = -(_gridModel.Columns - 1) * _cellSize * 0.5f;
            float offsetY = -(_gridModel.Rows - 1) * _cellSize * 0.5f;
            return new Vector3(offsetX, offsetY, 0f);
        }

        public Vector3 GetLocalPosition(int x, int y)
        {
            Vector3 origin = GetGridOrigin();
            return origin + new Vector3(x * _cellSize, y * _cellSize, 0f);
        }

        public Vector3 GetWorldPosition(CellModel cell)
        {
            if (cell == null)
            {
                return _gridRoot.position;
            }

            return _gridRoot.TransformPoint(GetLocalPosition(cell.X, cell.Y));
        }
    }
}
