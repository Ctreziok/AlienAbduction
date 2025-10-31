using System.Collections.Generic;
using UnityEngine;

public static class ObjectPool
{
    static readonly Dictionary<GameObject, Stack<GameObject>> pools = new();

    public static void Warm(GameObject prefab, int count)
    {
        if (!pools.ContainsKey(prefab)) pools[prefab] = new Stack<GameObject>();
        for (int i = 0; i < count; i++)
        {
            var go = Object.Instantiate(prefab);
            EnsurePooledRef(go).sourcePrefab = prefab;   // <-- set correctly
            go.SetActive(false);
            pools[prefab].Push(go);
        }
    }

    public static GameObject Spawn(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out var stack))
        {
            stack = new Stack<GameObject>();
            pools[prefab] = stack;
        }

        GameObject go = stack.Count > 0 ? stack.Pop() : Object.Instantiate(prefab);
        EnsurePooledRef(go).sourcePrefab = prefab;       // <-- set correctly
        go.SetActive(true);
        return go;
    }

    public static void Despawn(GameObject instance)
    {
        instance.SetActive(false);
        var pooled = instance.GetComponent<PooledRef>();
        if (pooled != null && pooled.sourcePrefab != null && pools.ContainsKey(pooled.sourcePrefab))
            pools[pooled.sourcePrefab].Push(instance);
        else
            Object.Destroy(instance); // safety fallback
    }

    static PooledRef EnsurePooledRef(GameObject go)
    {
        var pr = go.GetComponent<PooledRef>();
        if (pr == null) pr = go.AddComponent<PooledRef>();
        return pr;
    }
}

