using System.Collections.Generic;
using UnityEngine;

public class BuildingSpawner : MonoBehaviour
{
    [Header("Scroll")]
    public float scrollSpeed = 6f;
    public float speedAccelPerMinute = 0.6f;

    [Header("Layout")]
    public float buildingWidth = 6f;                 // keep equal to your prefabs’ width
    public Vector2 gapRange = new(1.6f, 3.2f);
    public Vector2 heightDeltaRange = new(-1.1f, 1.1f);
    public float minTopY = -1f, maxTopY = 3.5f;

    [Header("Pool & Prefabs")]
    public List<GameObject> buildingPrefabs;
    public int warmCountPerPrefab = 6;

    readonly List<Transform> active = new();
    float rightEdgeX, leftCullX;
    float lastTopY = 0f;                              // remember last roof height

    void OnValidate()
    {
        if (gapRange.y < gapRange.x) gapRange.y = gapRange.x + 0.1f;
        if (buildingWidth < 0.1f) buildingWidth = 6f;
    }

    void Start()
    {
        if (buildingPrefabs == null || buildingPrefabs.Count == 0)
        { Debug.LogError("[BuildingSpawner] No buildingPrefabs assigned."); enabled = false; return; }

        if (buildingWidth <= 0.01f)
        { Debug.LogError("[BuildingSpawner] Building Width must be > 0 (e.g., 6)."); enabled = false; return; }

        foreach (var p in buildingPrefabs) ObjectPool.Warm(p, warmCountPerPrefab);

        UpdateCameraEdges();
        // Initial seed
        float xEdge = leftCullX - buildingWidth;     // start a bit off-screen left
        lastTopY = 0f;
        for (int i = 0; i < 10; i++)
            SpawnChunk(ref xEdge, ref lastTopY, i == 0 ? 0f : RandomGap(), i == 0);
    }

    void Update()
    {
        UpdateCameraEdges();

        // Difficulty ramp
        scrollSpeed += speedAccelPerMinute * (Time.deltaTime / 60f);

        // Move & cull
        for (int i = active.Count - 1; i >= 0; i--)
        {
            var t = active[i];
            t.position += Vector3.left * scrollSpeed * Time.deltaTime;
            if (t.position.x < leftCullX)
            {
                ObjectPool.Despawn(t.gameObject);
                active.RemoveAt(i);
            }
        }

        // If everything got culled, reseed from left so we never “run out”
        if (active.Count == 0)
        {
            float xEdge = leftCullX - buildingWidth;
            // keep lastTopY; or clamp to 0 for stability
            lastTopY = Mathf.Clamp(lastTopY, minTopY, maxTopY);
            int seedGuard = 0;
            while (xEdge < rightEdgeX && seedGuard++ < 32)
                SpawnChunk(ref xEdge, ref lastTopY, RandomGap(), seedGuard == 1);
            if (seedGuard >= 32) Debug.LogWarning("[BuildingSpawner] Seed guard hit. Check widths/gaps.");
            return; // we just reseeded; let the next frame continue
        }

        // Refill to the right edge
        float currentRight = GetRightEdgeOfLast();
        float yTop = GetLastTopY();

        int guard = 0, guardMax = 64;
        while (currentRight < rightEdgeX && guard++ < guardMax)
            SpawnChunk(ref currentRight, ref yTop, RandomGap(), false);

        if (guard >= guardMax)
            Debug.LogWarning("[BuildingSpawner] Fill guard hit. Check Building Width, gapRange, and prefab setup.");
    }

    void UpdateCameraEdges()
    {
        var cam = Camera.main;
        float half = cam.orthographicSize * cam.aspect;
        rightEdgeX = cam.transform.position.x + half + 2f;
        leftCullX  = cam.transform.position.x - half - 8f;
    }

    float RandomGap() => Random.Range(gapRange.x, gapRange.y);
    float ClampTop(float v) => Mathf.Clamp(v, minTopY, maxTopY);

    void SpawnChunk(ref float rightMostEdge, ref float lastTop, float gap, bool first)
    {
        if (!first) rightMostEdge += gap;

        var prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Count)];
        var go = ObjectPool.Spawn(prefab);
        var chunk = go.GetComponent<BuildingChunk>();
        if (chunk == null)
        {
            Debug.LogError("[BuildingSpawner] Prefab missing BuildingChunk: " + prefab.name);
            ObjectPool.Despawn(go);
            return;
        }

        // If you want per-chunk width, uncomment the next line and replace uses of buildingWidth with w
        float w = buildingWidth; // float w = Mathf.Max(0.1f, chunk.width);

        float nextTop = ClampTop(lastTop + Random.Range(heightDeltaRange.x, heightDeltaRange.y));

        float worldY = nextTop - chunk.topLocalY;
        float worldX = rightMostEdge + w * 0.5f;

        go.transform.position = new Vector3(worldX, worldY, 0f);
        active.Add(go.transform);

        rightMostEdge = worldX + w * 0.5f;
        lastTop = nextTop;
        lastTopY = nextTop;
    }

    float GetRightEdgeOfLast()
    {
        if (active.Count == 0)
            return leftCullX - buildingWidth; // ensures refill happens next
        var last = active[active.Count - 1];
        return last.position.x + buildingWidth * 0.5f;
    }

    float GetLastTopY()
    {
        if (active.Count == 0) return lastTopY;
        var last = active[active.Count - 1];
        var chunk = last.GetComponent<BuildingChunk>();
        return last.position.y + chunk.topLocalY;
    }
}
