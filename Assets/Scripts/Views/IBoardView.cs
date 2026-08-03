using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Pipes
{
    public interface IBoardView
    {
        void Build();
        void Refresh();
        void PlayRotatePunch(CellModel cell);

        UniTask AnimatePathFillAsync(
            IReadOnlyList<CellModel> path,
            Action<CellModel> onStep,
            CancellationToken cancellationToken);

        UniTask AnimateDropsAsync(
            IReadOnlyList<DropData> drops,
            CancellationToken cancellationToken);
    }
}
