using Lean;
using UnityEngine;

namespace Pipes
{
    public static class LeanPoolUtility
    {
        public static LeanPool EnsureExpandablePool(GameObject prefab, int preload)
        {
            LeanPool pool = LeanPool.AllPools.Find(p => p != null && p.Prefab == prefab);
            if (pool == null)
            {
                // Keep pools out of scene teardown (same idea as LeanPool.Spawn's container).
                var go = new GameObject(prefab.name + " Pool");
                Object.DontDestroyOnLoad(go);
                pool = go.AddComponent<LeanPool>();
                pool.Prefab = prefab;
            }

            pool.Capacity = 0;
            pool.Preload = preload;
            pool.Notification = LeanPool.NotificationType.SendMessage;

            while (pool.Total < preload)
            {
                pool.FastPreload();
            }

            return pool;
        }
    }
}
