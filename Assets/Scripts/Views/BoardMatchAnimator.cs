using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Pipes
{
    public sealed class BoardMatchAnimator
    {
        private readonly BoardCellCollection _cells;
        private readonly GameAnimationSetup _animationSetup;

        public BoardMatchAnimator(BoardCellCollection cells, GameAnimationSetup animationSetup)
        {
            _cells = cells;
            _animationSetup = animationSetup;
        }

        public async UniTask AnimatePathFillAsync(
            IReadOnlyList<CellModel> path,
            Action<CellModel> onStep,
            CancellationToken cancellationToken)
        {
            if (path == null || path.Count == 0)
            {
                return;
            }

            _cells.Refresh();

            for (int i = 0; i < path.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                CellModel cell = path[i];
                onStep?.Invoke(cell);
                _cells.Refresh();

                await UniTask.Delay(_animationSetup.PathFillStepMs, cancellationToken: cancellationToken);
            }
        }

        public async UniTask AnimateDropsAsync(
            IReadOnlyList<DropData> drops,
            CancellationToken cancellationToken)
        {
            if (drops == null || drops.Count == 0)
            {
                return;
            }

            float duration = _animationSetup.DropDurationSeconds;
            var fallTasks = new List<UniTask>(drops.Count);

            for (int i = 0; i < drops.Count; i++)
            {
                DropData drop = drops[i];
                if (drop.Model == null)
                {
                    continue;
                }

                CellView view = _cells.GetOrCreate(drop.Model, drop.FromY);
                if (view == null)
                {
                    continue;
                }

                fallTasks.Add(view.FallTo(drop.ToY, duration, cancellationToken));
            }

            if (fallTasks.Count == 0)
            {
                return;
            }

            await UniTask.WhenAll(fallTasks);
        }
    }
}
