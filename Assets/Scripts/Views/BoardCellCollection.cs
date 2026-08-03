using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Pipes
{
    public sealed class BoardCellCollection : IInitializable, IDisposable
    {
        private readonly GridModel _gridModel;
        private readonly SignalBus _signalBus;
        private readonly CellViewFactory _cellViewFactory;
        private readonly BoardLayout _layout;
        private readonly Dictionary<CellModel, CellView> _views = new Dictionary<CellModel, CellView>();
        private bool _bound;

        public BoardCellCollection(
            GridModel gridModel,
            SignalBus signalBus,
            CellViewFactory cellViewFactory,
            BoardLayout layout)
        {
            _gridModel = gridModel;
            _signalBus = signalBus;
            _cellViewFactory = cellViewFactory;
            _layout = layout;
        }

        public void Initialize()
        {
            if (_bound)
            {
                return;
            }

            _signalBus.Subscribe<CellsDestroyedSignal>(OnCellsDestroyed);
            _bound = true;
        }

        public void Dispose()
        {
            ClearViews();

            if (!_bound)
            {
                return;
            }

            _signalBus.TryUnsubscribe<CellsDestroyedSignal>(OnCellsDestroyed);
            _bound = false;
        }

        public void BuildFromModel()
        {
            _cellViewFactory.Warmup();

            for (int x = 0; x < _gridModel.Columns; x++)
            {
                for (int y = 0; y < _gridModel.Rows; y++)
                {
                    Create(_gridModel.GetCell(x, y), y);
                }
            }

            Refresh();
        }

        public void Refresh()
        {
            foreach (KeyValuePair<CellModel, CellView> pair in _views)
            {
                pair.Value.UpdateVisual();
            }
        }

        public bool TryGet(CellModel model, out CellView view)
        {
            return _views.TryGetValue(model, out view);
        }

        public CellView Create(CellModel model, int visualY)
        {
            if (model == null)
            {
                return null;
            }

            CellView view = _cellViewFactory.Create(model);
            if (view == null)
            {
                Debug.LogError(
                    $"BoardCellCollection: failed to create CellView for {model.Type} at ({model.X},{model.Y}).");
                return null;
            }

            view.ConfigureLayout(_layout.CellSize, _layout.GetGridOrigin());
            view.SetVisualRow(visualY);
            view.UpdateVisual();
            _views[model] = view;
            return view;
        }

        public CellView GetOrCreate(CellModel model, int visualY)
        {
            if (TryGet(model, out CellView view))
            {
                view.SetVisualRow(visualY);
                return view;
            }

            return Create(model, visualY);
        }

        private void OnCellsDestroyed(CellsDestroyedSignal signal)
        {
            if (signal?.DestroyedCells == null)
            {
                return;
            }

            for (int i = 0; i < signal.DestroyedCells.Count; i++)
            {
                CellModel model = signal.DestroyedCells[i];
                if (!_views.TryGetValue(model, out CellView view))
                {
                    continue;
                }

                _views.Remove(model);
                _cellViewFactory.Despawn(view);
            }
        }

        private void ClearViews()
        {
            foreach (KeyValuePair<CellModel, CellView> pair in _views)
            {
                if (pair.Value != null)
                {
                    _cellViewFactory.Despawn(pair.Value);
                }
            }

            _views.Clear();
        }
    }
}
