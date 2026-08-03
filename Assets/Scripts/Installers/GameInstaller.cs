using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Pipes
{
    public sealed class GameInstaller : MonoInstaller
    {
        [Header("Catalogs")]
        [SerializeField] private PipePrefabCatalog _pipePrefabCatalog;
        [SerializeField] private ParticleVfxCatalog _particleVfxCatalog;

        [Header("Setup")]
        [Tooltip("Shared infra settings: pooling.")]
        [FormerlySerializedAs("gameBalance")]
        [SerializeField] private GameBalance _gameBalance;
        [Tooltip("Active difficulty tier (Easy / Normal / Hard).")]
        [FormerlySerializedAs("difficultyProfile")]
        [SerializeField] private DifficultyProfile _difficultyProfile;
        [FormerlySerializedAs("gameAnimationSetup")]
        [SerializeField] private GameAnimationSetup _gameAnimationSetup;
        [FormerlySerializedAs("gridRoot")]
        [SerializeField] private Transform _gridRoot;
        [FormerlySerializedAs("cellSize")]
        [SerializeField] private float _cellSize = 1f;

        public override void InstallBindings()
        {
            ValidateSetup();

            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<CellClickedSignal>();
            Container.DeclareSignal<CellsDestroyedSignal>();
            Container.DeclareSignal<ScoreChangedSignal>();

            Container.BindInstance(_gridRoot).WithId("GridRoot");
            Container.BindInstance(_cellSize).WithId("CellSize");
            Container.BindInstance(_pipePrefabCatalog);
            Container.BindInstance(_particleVfxCatalog);
            Container.BindInstance(_gameBalance);
            Container.BindInstance(_difficultyProfile);
            Container.BindInstance(_gameAnimationSetup);

            Container.Bind<InitialGridGenerator>().AsSingle();
            Container.BindInterfacesAndSelfTo<GridModel>().AsSingle();
            Container.Bind<BoardLayout>().AsSingle();
            Container.BindInterfacesAndSelfTo<BoardCellCollection>().AsSingle();
            Container.BindInterfacesAndSelfTo<BoardVfxPresenter>().AsSingle();
            Container.Bind<BoardMatchAnimator>().AsSingle();
            Container.Bind<IBoardView>().To<BoardView>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameController>().AsSingle();
            Container.BindInterfacesTo<ScoreService>().AsSingle();
            Container.BindInterfacesTo<Bootstrapper>().AsSingle();
            Container.Bind<CellViewFactory>().AsSingle();
            Container.Bind<ParticleVfxFactory>().AsSingle();
            Container.Bind<ScoreView>().FromComponentInHierarchy().AsSingle().NonLazy();
        }

        private void ValidateSetup()
        {
            if (_pipePrefabCatalog == null)
            {
                Debug.LogError("GameInstaller: PipePrefabCatalog is not assigned.", this);
            }
            else
            {
                ValidatePipeCatalog(_pipePrefabCatalog);
            }

            if (_particleVfxCatalog == null)
            {
                Debug.LogError("GameInstaller: ParticleVfxCatalog is not assigned.", this);
            }
            else
            {
                ValidateVfxCatalog(_particleVfxCatalog);
            }

            if (_gameBalance == null)
            {
                Debug.LogError("GameInstaller: GameBalance is not assigned.", this);
            }

            if (_difficultyProfile == null)
            {
                Debug.LogError("GameInstaller: DifficultyProfile is not assigned.", this);
            }
            else if (_pipePrefabCatalog != null)
            {
                ValidateSpawnCoverage(_difficultyProfile, _pipePrefabCatalog);
            }

            if (_gameAnimationSetup == null)
            {
                Debug.LogError("GameInstaller: GameAnimationSetup is not assigned.", this);
            }

            if (_gridRoot == null)
            {
                Debug.LogError("GameInstaller: GridRoot is not assigned.", this);
            }

            if (_cellSize <= 0f)
            {
                Debug.LogError("GameInstaller: CellSize must be > 0.", this);
            }
        }

        private static void ValidatePipeCatalog(PipePrefabCatalog catalog)
        {
            IReadOnlyList<PipePrefabEntry> entries = catalog.Entries;
            if (entries == null || entries.Count == 0)
            {
                Debug.LogError($"GameInstaller: PipePrefabCatalog '{catalog.name}' has no entries.", catalog);
                return;
            }

            var seen = new HashSet<PipeType>();
            for (int i = 0; i < entries.Count; i++)
            {
                PipePrefabEntry entry = entries[i];
                if (entry.Prefab == null)
                {
                    Debug.LogError(
                        $"GameInstaller: PipePrefabCatalog '{catalog.name}' entry[{i}] ({entry.Type}) has no prefab.",
                        catalog);
                    continue;
                }

                if (!seen.Add(entry.Type))
                {
                    Debug.LogError(
                        $"GameInstaller: PipePrefabCatalog '{catalog.name}' has duplicate type '{entry.Type}'.",
                        catalog);
                }
            }
        }

        private static void ValidateVfxCatalog(ParticleVfxCatalog catalog)
        {
            IReadOnlyList<ParticleVfxEntry> entries = catalog.Entries;
            if (entries == null || entries.Count == 0)
            {
                Debug.LogError($"GameInstaller: ParticleVfxCatalog '{catalog.name}' has no entries.", catalog);
                return;
            }

            var seen = new HashSet<ParticleVfxId>();
            for (int i = 0; i < entries.Count; i++)
            {
                ParticleVfxEntry entry = entries[i];
                if (entry.Prefab == null)
                {
                    Debug.LogError(
                        $"GameInstaller: ParticleVfxCatalog '{catalog.name}' entry[{i}] ({entry.Id}) has no prefab.",
                        catalog);
                    continue;
                }

                if (!seen.Add(entry.Id))
                {
                    Debug.LogError(
                        $"GameInstaller: ParticleVfxCatalog '{catalog.name}' has duplicate id '{entry.Id}'.",
                        catalog);
                }
            }
        }

        private static void ValidateSpawnCoverage(DifficultyProfile difficulty, PipePrefabCatalog catalog)
        {
            IReadOnlyList<PipeSpawnWeight> weights = difficulty.SpawnWeights;
            if (weights == null || weights.Count == 0)
            {
                Debug.LogError(
                    $"GameInstaller: DifficultyProfile '{difficulty.name}' has empty spawnWeights.",
                    difficulty);
                return;
            }

            for (int i = 0; i < weights.Count; i++)
            {
                PipeType type = weights[i].Type;
                if (!catalog.TryGetPrefab(type, out _))
                {
                    Debug.LogError(
                        $"GameInstaller: DifficultyProfile '{difficulty.name}' spawns '{type}', " +
                        $"but PipePrefabCatalog '{catalog.name}' has no prefab for it.",
                        difficulty);
                }
            }
        }
    }
}
