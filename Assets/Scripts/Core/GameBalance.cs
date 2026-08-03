using UnityEngine;
using UnityEngine.Serialization;

namespace Pipes
{
    [CreateAssetMenu(fileName = "GameBalance", menuName = "Pipes/Game Balance")]
    public sealed class GameBalance : ScriptableObject
    {
        [Header("Pool")]
        [Tooltip("Clones preloaded per pipe prefab. Pool expands beyond this when Capacity is 0.")]
        [FormerlySerializedAs("poolPreloadPerPipeType")]
        [SerializeField] private int _poolPreloadPerPipeType = 16;

        public int PoolPreloadPerPipeType => Mathf.Max(0, _poolPreloadPerPipeType);
    }
}
