using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Pipes
{
    public sealed class BoardView : IBoardView
    {
        private readonly BoardCellCollection _cells;
        private readonly BoardVfxPresenter _vfx;
        private readonly BoardMatchAnimator _animator;

        public BoardView(
            BoardCellCollection cells,
            BoardVfxPresenter vfx,
            BoardMatchAnimator animator)
        {
            _cells = cells;
            _vfx = vfx;
            _animator = animator;
        }

        public void Build()
        {
            _cells.BuildFromModel();
        }

        public void Refresh()
        {
            _cells.Refresh();
        }

        public void PlayRotatePunch(CellModel cell)
        {
            if (cell == null || !_cells.TryGet(cell, out CellView view))
            {
                return;
            }

            view.PlayRotatePunch();
            _vfx.PlaySpin(cell);
        }

        public UniTask AnimatePathFillAsync(
            IReadOnlyList<CellModel> path,
            Action<CellModel> onStep,
            CancellationToken cancellationToken)
        {
            return _animator.AnimatePathFillAsync(path, onStep, cancellationToken);
        }

        public UniTask AnimateDropsAsync(
            IReadOnlyList<DropData> drops,
            CancellationToken cancellationToken)
        {
            return _animator.AnimateDropsAsync(drops, cancellationToken);
        }
    }
}
