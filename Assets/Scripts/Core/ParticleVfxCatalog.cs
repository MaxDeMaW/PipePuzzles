using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pipes
{
    public enum ParticleVfxId
    {
        Spin = 0,
        Explosion = 1
    }

    [Serializable]
    public struct ParticleVfxEntry
    {
        public ParticleVfxId Id;
        public GameObject Prefab;
        [Min(0.05f)] public float Lifetime;
    }

    [CreateAssetMenu(fileName = "ParticleVfxCatalog", menuName = "Pipes/Particle VFX Catalog")]
    public sealed class ParticleVfxCatalog : ScriptableObject
    {
        [SerializeField] private ParticleVfxEntry[] _entries =
        {
            new ParticleVfxEntry { Id = ParticleVfxId.Spin, Lifetime = 1f },
            new ParticleVfxEntry { Id = ParticleVfxId.Explosion, Lifetime = 1f }
        };

        public IReadOnlyList<ParticleVfxEntry> Entries => _entries;

        public bool TryGet(ParticleVfxId id, out ParticleVfxEntry entry)
        {
            if (_entries == null)
            {
                entry = default;
                return false;
            }

            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].Id != id)
                {
                    continue;
                }

                entry = _entries[i];
                return entry.Prefab != null;
            }

            entry = default;
            return false;
        }

        public Dictionary<ParticleVfxId, ParticleVfxEntry> ToDictionary()
        {
            var map = new Dictionary<ParticleVfxId, ParticleVfxEntry>();
            if (_entries == null)
            {
                return map;
            }

            for (int i = 0; i < _entries.Length; i++)
            {
                ParticleVfxEntry entry = _entries[i];
                if (entry.Prefab == null)
                {
                    continue;
                }

                map[entry.Id] = entry;
            }

            return map;
        }
    }
}
