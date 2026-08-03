using System.Collections.Generic;
using Lean;
using UnityEngine;
using Zenject;

namespace Pipes
{
    public sealed class ParticleVfxFactory
    {
        private readonly Dictionary<ParticleVfxId, ParticleVfxEntry> _byId;
        private readonly GameBalance _balance;
        private readonly HashSet<GameObject> _warmedPrefabs = new HashSet<GameObject>();

        public ParticleVfxFactory(ParticleVfxCatalog catalog, GameBalance balance)
        {
            _byId = catalog != null
                ? catalog.ToDictionary()
                : new Dictionary<ParticleVfxId, ParticleVfxEntry>();
            _balance = balance;
        }

        public void Warmup()
        {
            foreach (KeyValuePair<ParticleVfxId, ParticleVfxEntry> pair in _byId)
            {
                WarmupPrefab(pair.Value.Prefab);
            }
        }

        public void Spawn(ParticleVfxId type, Vector3 worldPosition)
        {
            if (!_byId.TryGetValue(type, out ParticleVfxEntry entry))
            {
                Debug.LogWarning($"ParticleVfxFactory: no prefab registered for {type}.");
                return;
            }

            SpawnAt(entry.Prefab, worldPosition, Mathf.Max(0.05f, entry.Lifetime), type.ToString());
        }

        private void WarmupPrefab(GameObject prefab)
        {
            if (prefab == null || !_warmedPrefabs.Add(prefab))
            {
                return;
            }

            LeanPoolUtility.EnsureExpandablePool(prefab, _balance.PoolPreloadPerPipeType);
        }

        private void SpawnAt(GameObject prefab, Vector3 worldPosition, float lifetime, string debugName)
        {
            if (prefab == null)
            {
                return;
            }

            WarmupPrefab(prefab);
            LeanPoolUtility.EnsureExpandablePool(prefab, _balance.PoolPreloadPerPipeType);

            GameObject clone = LeanPool.Spawn(prefab, worldPosition, prefab.transform.rotation);
            if (clone == null)
            {
                Debug.LogError($"ParticleVfxFactory: LeanPool failed to spawn {debugName}.");
                return;
            }

            PooledParticleVfxView view = clone.GetComponent<PooledParticleVfxView>();
            if (view == null)
            {
                view = clone.AddComponent<PooledParticleVfxView>();
            }

            view.Play(lifetime);
        }
    }
}
