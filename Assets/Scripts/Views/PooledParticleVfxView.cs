using Lean;
using UnityEngine;

namespace Pipes
{
    public sealed class PooledParticleVfxView : MonoBehaviour
    {
        private ParticleSystem[] _particleSystems;

        private void Awake()
        {
            CacheParticles();
        }

        public void Play(float lifetime)
        {
            CacheParticles();

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                ParticleSystem system = _particleSystems[i];
                if (system == null)
                {
                    continue;
                }

                system.Clear(true);
                system.Play(true);
            }

            LeanPool.Despawn(gameObject, Mathf.Max(0.05f, lifetime));
        }

        private void OnDespawn()
        {
            if (_particleSystems == null)
            {
                return;
            }

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                ParticleSystem system = _particleSystems[i];
                if (system == null)
                {
                    continue;
                }

                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void CacheParticles()
        {
            if (_particleSystems != null)
            {
                return;
            }

            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }
    }
}
