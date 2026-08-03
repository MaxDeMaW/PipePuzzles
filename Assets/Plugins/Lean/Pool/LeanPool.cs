using UnityEngine;
using System.Collections.Generic;

namespace Lean
{
    // This component allows you to pool Unity objects for fast instantiation and destruction
    [AddComponentMenu("Lean/Pool")]
    public class LeanPool : MonoBehaviour
    {
        public class DelayedDestruction
        {
            public GameObject Clone;
            public float Life;
        }

        public enum NotificationType
        {
            None,
            SendMessage,
            BroadcastMessage
        }

        public static List<LeanPool> AllPools = new List<LeanPool>();
        public static Dictionary<GameObject, LeanPool> AllLinks = new Dictionary<GameObject, LeanPool>();

        [Tooltip("The prefab the clones will be based on")]
        public GameObject Prefab;

        [Tooltip("Should this pool preload some clones?")]
        public int Preload;

        [Tooltip("Should this pool have a maximum amount of spawnable clones?")]
        public int Capacity;

        [Tooltip("Should this pool send messages to the clones when they're spawned/despawned?")]
        public NotificationType Notification = NotificationType.SendMessage;

        private readonly List<GameObject> cache = new List<GameObject>();
        private readonly List<DelayedDestruction> delayedDestructions = new List<DelayedDestruction>();
        private int total;

        private static Transform Container
        {
            get
            {
                if (_container == null)
                {
                    var go = new GameObject("LeanPool Container");
                    Object.DontDestroyOnLoad(go);
                    _container = go.transform;
                }

                return _container;
            }
        }

        private static Transform _container;

        public static T Spawn<T>(T prefab) where T : Component
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity, null);
        }

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab, position, rotation, null);
        }

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent)
            where T : Component
        {
            GameObject gameObject = prefab != null ? prefab.gameObject : null;
            GameObject clone = Spawn(gameObject, position, rotation, parent);
            return clone != null ? clone.GetComponent<T>() : null;
        }

        public static GameObject Spawn(GameObject prefab)
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity, null);
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Spawn(prefab, position, rotation, null);
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (prefab == null)
            {
                Debug.LogError("Attempting to spawn a null prefab");
                return null;
            }

            LeanPool pool = AllPools.Find(p => p.Prefab == prefab);
            if (pool == null)
            {
                pool = new GameObject(prefab.name + " Pool").AddComponent<LeanPool>();
                pool.Prefab = prefab;
                pool.transform.SetParent(Container, false);
            }

            GameObject clone = pool.FastSpawn(position, rotation, parent);
            if (clone != null)
            {
                AllLinks[clone] = pool;
                return clone;
            }

            return null;
        }

        public static void Despawn(Component clone, float delay = 0.0f)
        {
            if (clone != null)
            {
                Despawn(clone.gameObject, delay);
            }
        }

        public static void Despawn(GameObject clone, float delay = 0.0f)
        {
            if (clone == null)
            {
                return;
            }

            if (AllLinks.TryGetValue(clone, out LeanPool pool))
            {
                AllLinks.Remove(clone);
                pool.FastDespawn(clone, delay);
            }
            else
            {
                Debug.LogError(
                    "Attempting to despawn " + clone.name +
                    ", but failed to find pool for it! Make sure you created it using LeanPool.Spawn!");
                Object.Destroy(clone);
            }
        }

        public int Total => total;
        public int Cached => cache.Count;

        public GameObject FastSpawn(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (Prefab == null)
            {
                Debug.LogError("Attempting to spawn null");
                return null;
            }

            while (cache.Count > 0)
            {
                int index = cache.Count - 1;
                GameObject clone = cache[index];
                cache.RemoveAt(index);

                if (clone != null)
                {
                    Transform cloneTransform = clone.transform;
                    if (parent == null)
                    {
                        cloneTransform.SetParent(null, false);
                        cloneTransform.position = position;
                        cloneTransform.rotation = rotation;
                    }
                    else
                    {
                        cloneTransform.SetParent(parent, false);
                        cloneTransform.localPosition = position;
                        cloneTransform.localRotation = rotation;
                    }

                    clone.SetActive(true);
                    SendNotification(clone, "OnSpawn");
                    return clone;
                }
            }

            if (Capacity <= 0 || total < Capacity)
            {
                GameObject clone = FastClone(position, rotation, parent);
                SendNotification(clone, "OnSpawn");
                return clone;
            }

            return null;
        }

        public void FastDespawn(GameObject clone, float delay = 0.0f)
        {
            if (clone == null)
            {
                return;
            }

            if (delay > 0.0f)
            {
                if (delayedDestructions.Exists(m => m.Clone == clone) == false)
                {
                    DelayedDestruction delayedDestruction =
                        LeanClassPool<DelayedDestruction>.Spawn() ?? new DelayedDestruction();
                    delayedDestruction.Clone = clone;
                    delayedDestruction.Life = delay;
                    delayedDestructions.Add(delayedDestruction);
                }
            }
            else
            {
                cache.Add(clone);
                SendNotification(clone, "OnDespawn");
                clone.SetActive(false);
                clone.transform.SetParent(transform, false);
            }
        }

        public void FastPreload()
        {
            if (Prefab == null)
            {
                return;
            }

            GameObject clone = FastClone(Vector3.zero, Quaternion.identity, null);
            cache.Add(clone);
            clone.SetActive(false);
            clone.transform.SetParent(transform, false);
        }

        protected virtual void Awake()
        {
            UpdatePreload();
        }

        protected virtual void OnEnable()
        {
            AllPools.Add(this);
        }

        protected virtual void OnDisable()
        {
            AllPools.Remove(this);
        }

        protected virtual void Update()
        {
            for (int i = delayedDestructions.Count - 1; i >= 0; i--)
            {
                DelayedDestruction markedObject = delayedDestructions[i];
                if (markedObject.Clone != null)
                {
                    markedObject.Life -= Time.deltaTime;
                    if (markedObject.Life <= 0.0f)
                    {
                        RemoveDelayedDestruction(i);
                        FastDespawn(markedObject.Clone);
                    }
                }
                else
                {
                    RemoveDelayedDestruction(i);
                }
            }
        }

        private void RemoveDelayedDestruction(int index)
        {
            DelayedDestruction delayedDestruction = delayedDestructions[index];
            delayedDestructions.RemoveAt(index);
            LeanClassPool<DelayedDestruction>.Despawn(delayedDestruction);
        }

        private void UpdatePreload()
        {
            if (Prefab == null)
            {
                return;
            }

            for (int i = total; i < Preload; i++)
            {
                FastPreload();
            }
        }

        private GameObject FastClone(Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject clone = Object.Instantiate(Prefab, position, rotation);
            total += 1;
            clone.name = Prefab.name + " " + total;
            clone.transform.SetParent(parent, false);
            return clone;
        }

        private void SendNotification(GameObject clone, string messageName)
        {
            switch (Notification)
            {
                case NotificationType.SendMessage:
                    clone.SendMessage(messageName, SendMessageOptions.DontRequireReceiver);
                    break;
                case NotificationType.BroadcastMessage:
                    clone.BroadcastMessage(messageName, SendMessageOptions.DontRequireReceiver);
                    break;
            }
        }
    }
}
