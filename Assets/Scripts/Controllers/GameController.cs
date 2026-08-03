using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Pipes
{
    public sealed class GameController : IDisposable
    {
        private enum TurnState
        {
            Idle,
            Filling,
            Collapsing
        }

        private readonly GridModel _gridModel;
        private readonly SignalBus _signalBus;
        private readonly IBoardView _boardView;
        private readonly SemaphoreSlim _flowLock = new SemaphoreSlim(1, 1);

        private CancellationTokenSource _flowCts;
        private int _clickGeneration;
        private bool _started;
        private TurnState _state = TurnState.Idle;

        private bool CanAcceptClick => _state != TurnState.Collapsing;

        public GameController(
            GridModel gridModel,
            SignalBus signalBus,
            IBoardView boardView)
        {
            _gridModel = gridModel;
            _signalBus = signalBus;
            _boardView = boardView;
        }

        public void StartGame()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            _signalBus.Subscribe<CellClickedSignal>(OnCellClicked);
            RefreshBoardFlow();
        }

        public void Dispose()
        {
            _flowCts?.Cancel();
            _flowCts?.Dispose();
            _flowCts = null;
            _flowLock.Dispose();

            if (_started)
            {
                _signalBus.TryUnsubscribe<CellClickedSignal>(OnCellClicked);
                _started = false;
            }
        }

        private void OnCellClicked(CellClickedSignal signal)
        {
            if (!CanAcceptClick || signal?.Model == null)
            {
                return;
            }

            HandleClickAsync(signal.Model).Forget();
        }

        private async UniTask HandleClickAsync(CellModel model)
        {
            int generation = ++_clickGeneration;
            _flowCts?.Cancel();

            await _flowLock.WaitAsync();
            try
            {
                // A newer click already superseded this one while we waited for the lock.
                if (generation != _clickGeneration || !CanAcceptClick)
                {
                    return;
                }

                model.CurrentRotation = (model.CurrentRotation + 1) % 4;
                RefreshBoardFlow();
                _boardView.PlayRotatePunch(model);

                _flowCts?.Dispose();
                _flowCts = new CancellationTokenSource();
                CancellationToken token = _flowCts.Token;

                try
                {
                    await RunMatchFlowAsync(token);
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    SetState(TurnState.Idle);
                }
            }
            finally
            {
                _flowLock.Release();
            }
        }

        private async UniTask RunMatchFlowAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<CellModel> matched = _gridModel.CollectMatchedChainsOrdered();
                if (matched.Count == 0)
                {
                    RefreshBoardFlow();
                    SetState(TurnState.Idle);
                    return;
                }

                SetState(TurnState.Filling);

                try
                {
                    await _boardView.AnimatePathFillAsync(
                        matched,
                        _gridModel.MarkWaterFilled,
                        cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    RefreshBoardFlow();
                    throw;
                }

                SetState(TurnState.Collapsing);
                await DestroyAndCollapseAsync(matched, cancellationToken);
            }
        }

        private async UniTask DestroyAndCollapseAsync(
            List<CellModel> matched,
            CancellationToken cancellationToken)
        {
            _signalBus.Fire(new CellsDestroyedSignal(matched));
            List<DropData> drops = _gridModel.DestroyAndCollapse(matched);
            await _boardView.AnimateDropsAsync(drops, cancellationToken);
        }

        private void RefreshBoardFlow()
        {
            _gridModel.RefreshFlowFromLeft();
            _boardView.Refresh();
        }

        private void SetState(TurnState next)
        {
            if (_state == next)
            {
                return;
            }

            if (!IsAllowedTransition(_state, next))
            {
                Debug.LogWarning(
                    $"GameController: unexpected turn transition {_state} -> {next}.");
            }

            _state = next;
        }

        private static bool IsAllowedTransition(TurnState from, TurnState to)
        {
            if (to == TurnState.Idle)
            {
                return true;
            }

            switch (from)
            {
                case TurnState.Idle:
                    return to == TurnState.Filling;
                case TurnState.Filling:
                    return to == TurnState.Collapsing;
                case TurnState.Collapsing:
                    return to == TurnState.Filling;
                default:
                    return false;
            }
        }
    }
}
