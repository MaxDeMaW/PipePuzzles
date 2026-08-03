using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace Pipes
{
    [Serializable]
    public struct PipeSpawnWeight
    {
        public PipeType Type;
        [Min(0f)] public float Weight;
    }

    [CreateAssetMenu(fileName = "DifficultyProfile", menuName = "Pipes/Difficulty Profile")]
    public sealed class DifficultyProfile : ScriptableObject
    {
        [Header("Grid")]
        [FormerlySerializedAs("columns")]
        [SerializeField] private int _columns = 5;
        [FormerlySerializedAs("rows")]
        [SerializeField] private int _rows = 8;

        [Header("Generation Seed")]
        [Tooltip("If enabled, the same seed always produces the same starting board.")]
        [FormerlySerializedAs("useFixedSeed")]
        [SerializeField] private bool _useFixedSeed;
        [FormerlySerializedAs("seed")]
        [SerializeField] private int _seed = 12345;

        [Header("Pipe Spawn Weights")]
        [Tooltip("Relative chances for each pipe type (initial board + refill).")]
        [SerializeField] private PipeSpawnWeight[] _spawnWeights =
        {
            new PipeSpawnWeight { Type = PipeType.I, Weight = 1f },
            new PipeSpawnWeight { Type = PipeType.L, Weight = 1f },
            new PipeSpawnWeight { Type = PipeType.T, Weight = 0.85f },
            new PipeSpawnWeight { Type = PipeType.X, Weight = 0.35f }
        };

        [Header("Score")]
        [FormerlySerializedAs("scorePerPipe")]
        [SerializeField] private int _scorePerPipe = 5;

        public int Columns => Mathf.Max(1, _columns);
        public int Rows => Mathf.Max(1, _rows);

        public bool UseFixedSeed => _useFixedSeed;
        public int Seed => _seed;

        public int ScorePerPipe => Mathf.Max(0, _scorePerPipe);

        public IReadOnlyList<PipeSpawnWeight> SpawnWeights => _spawnWeights;

        public int ResolveSeed()
        {
            return _useFixedSeed ? _seed : Environment.TickCount;
        }

        public PipeType RollPipeType(Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (_spawnWeights == null || _spawnWeights.Length == 0)
            {
                return PipeType.I;
            }

            float total = 0f;
            for (int i = 0; i < _spawnWeights.Length; i++)
            {
                total += Mathf.Max(0f, _spawnWeights[i].Weight);
            }

            if (total <= 0f)
            {
                return _spawnWeights[random.Next(0, _spawnWeights.Length)].Type;
            }

            float roll = (float)(random.NextDouble() * total);
            for (int i = 0; i < _spawnWeights.Length; i++)
            {
                float weight = Mathf.Max(0f, _spawnWeights[i].Weight);
                if (roll < weight)
                {
                    return _spawnWeights[i].Type;
                }

                roll -= weight;
            }

            return _spawnWeights[_spawnWeights.Length - 1].Type;
        }
    }
}
