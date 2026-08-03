using System.Collections.Generic;
using Lean;
using UnityEngine;
using Zenject;

namespace Pipes
{
    public sealed class CellViewFactory
    {
        private readonly DiContainer _container;
        private readonly Transform _gridRoot;
        private readonly Dictionary<PipeType, GameObject> _prefabsByType;
        private readonly GameBalance _balance;
        private bool _warmedUp;

        public CellViewFactory(
            DiContainer container,
            [Inject(Id = "GridRoot")] Transform gridRoot,
            PipePrefabCatalog prefabs,
            GameBalance balance)
        {
            _container = container;
            _gridRoot = gridRoot;
            _prefabsByType = prefabs != null
                ? prefabs.ToDictionary()
                : new Dictionary<PipeType, GameObject>();
            _balance = balance;
        }

        public void Warmup()
        {
            if (_warmedUp)
            {
                return;
            }

            _warmedUp = true;

            foreach (KeyValuePair<PipeType, GameObject> pair in _prefabsByType)
            {
                EnsurePool(pair.Value, _balance.PoolPreloadPerPipeType);
            }
        }

        public CellView Create(CellModel model)
        {
            if (model == null)
            {
                return null;
            }

            Warmup();

            if (!_prefabsByType.TryGetValue(model.Type, out GameObject prefab) || prefab == null)
            {
                Debug.LogError($"No CellView prefab for PipeType {model.Type}. Add it to PipePrefabCatalog.");
                return null;
            }

            EnsurePool(prefab, _balance.PoolPreloadPerPipeType);

            GameObject clone = LeanPool.Spawn(prefab, Vector3.zero, Quaternion.identity, _gridRoot);
            if (clone == null)
            {
                Debug.LogError($"LeanPool failed to spawn {prefab.name}.");
                return null;
            }

            CellView view = clone.GetComponent<CellView>();
            if (view == null)
            {
                LeanPool.Despawn(clone);
                Debug.LogError($"Spawned object {clone.name} has no CellView.");
                return null;
            }

            _container.Inject(view);
            view.Init(model);
            return view;
        }

        public void Despawn(CellView view)
        {
            // Unity fake-null: clone or pool may already be destroyed during teardown.
            if (view == null)
            {
                return;
            }

            view.PrepareForPool();

            GameObject clone = view.gameObject;
            if (clone == null)
            {
                return;
            }

            if (LeanPool.AllLinks.TryGetValue(clone, out LeanPool pool) && pool == null)
            {
                LeanPool.AllLinks.Remove(clone);
                return;
            }

            LeanPool.Despawn(clone);
        }

        private static void EnsurePool(GameObject prefab, int preload)
        {
            LeanPoolUtility.EnsureExpandablePool(prefab, preload);
        }
    }
}
