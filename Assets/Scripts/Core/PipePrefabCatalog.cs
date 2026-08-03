using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pipes
{
    [Serializable]
    public struct PipePrefabEntry
    {
        public PipeType Type;
        public GameObject Prefab;
    }

    [CreateAssetMenu(fileName = "PipePrefabCatalog", menuName = "Pipes/Pipe Prefab Catalog")]
    public sealed class PipePrefabCatalog : ScriptableObject
    {
        [SerializeField] private PipePrefabEntry[] _entries =
        {
            new PipePrefabEntry { Type = PipeType.I },
            new PipePrefabEntry { Type = PipeType.L },
            new PipePrefabEntry { Type = PipeType.T },
            new PipePrefabEntry { Type = PipeType.X }
        };

        public IReadOnlyList<PipePrefabEntry> Entries => _entries;

        public bool TryGetPrefab(PipeType type, out GameObject prefab)
        {
            if (_entries == null)
            {
                prefab = null;
                return false;
            }

            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].Type != type)
                {
                    continue;
                }

                prefab = _entries[i].Prefab;
                return prefab != null;
            }

            prefab = null;
            return false;
        }

        public Dictionary<PipeType, GameObject> ToDictionary()
        {
            var map = new Dictionary<PipeType, GameObject>();
            if (_entries == null)
            {
                return map;
            }

            for (int i = 0; i < _entries.Length; i++)
            {
                PipePrefabEntry entry = _entries[i];
                if (entry.Prefab == null)
                {
                    continue;
                }

                map[entry.Type] = entry.Prefab;
            }

            return map;
        }
    }
}
