using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkSpawner
{
    // --- OBJECT POOL ---
    // Key: Prefab, Value: Queue of inactive pooled instances
    private static Dictionary<GameObject, Queue<GameObject>> Pool = new Dictionary<GameObject, Queue<GameObject>>();

    // --- COLLIDER CACHE ---
    // Key: Prefab, Value: cached collider bottom half-extent (bounds.extents.y)
    private static Dictionary<GameObject, float> ColliderCache = new Dictionary<GameObject, float>();

    // --- RIGIDBODY CACHE ---
    // Key: Prefab, Value: true if the prefab has a Rigidbody component
    private static Dictionary<GameObject, bool> RigidbodyCache = new Dictionary<GameObject, bool>();

    private static GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (!Pool.ContainsKey(prefab))
            Pool[prefab] = new Queue<GameObject>();

        GameObject instance;
        if (Pool[prefab].Count > 0)
        {
            instance = Pool[prefab].Dequeue();
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.transform.SetParent(parent, true);
            instance.SetActive(true);
        }
        else
        {
            instance = GameObject.Instantiate(prefab, position, rotation, parent);
        }
        return instance;
    }

    public static void ReturnToPool(GameObject instance)
    {
        if (instance == null) return;
        instance.SetActive(false);
        // Detach from parent so it survives parent recycling
        instance.transform.SetParent(null);
        // Reset Rigidbody if present
        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Find which prefab this instance belongs to (fallback: use name matching)
        // We store the prefab reference on spawn so we can look it up here
        var marker = instance.GetComponent<PooledObjectMarker>();
        if (marker != null && marker.SourcePrefab != null)
        {
            if (!Pool.ContainsKey(marker.SourcePrefab))
                Pool[marker.SourcePrefab] = new Queue<GameObject>();
            Pool[marker.SourcePrefab].Enqueue(instance);
        }
        else
        {
            // Fallback: add a marker and use the instance itself to find parent pool
            // This shouldn't happen in practice since all spawned objects get a marker
            GameObject.Destroy(instance); // Safety fallback
        }
    }

    public static void SpawnContent(LevelData TargetLevelData, Transform HolderTransform, float RoadBoundaryX, float ChunkLength, float ChunkStartZ, int ChunkMaxCapacity, MonoBehaviour CoroutineHost)
    {
        float TotalWeight = CalculateTotalWeight(TargetLevelData);
        PlaceObstacles(TargetLevelData, HolderTransform, RoadBoundaryX, ChunkLength, ChunkStartZ, ChunkMaxCapacity, TotalWeight, CoroutineHost);
        PlaceCoins(TargetLevelData, HolderTransform, RoadBoundaryX, ChunkLength, ChunkStartZ);
    }

    private static float CalculateTotalWeight(LevelData TargetLevelData)
    {
        float TotalWeightValue = 0f;
        if (TargetLevelData.SpawnableObjects == null) return 0f;

        for (int i = 0; i < TargetLevelData.SpawnableObjects.Count; i++)
        {
            TotalWeightValue += TargetLevelData.SpawnableObjects[i].Weight;
        }
        return TotalWeightValue;
    }

    private static void PlaceObstacles(LevelData TargetLevelData, Transform HolderTransform, float RoadBoundaryX, float ChunkLength, float ChunkStartZ, int ChunkMaxCapacity, float TotalWeight, MonoBehaviour CoroutineHost)
    {
        if (TargetLevelData.SpawnableObjects == null || TargetLevelData.SpawnableObjects.Count == 0)
            return;

        int ObstacleCount = Mathf.Max(1, (TargetLevelData.Difficulty * 20) / ChunkMaxCapacity);
        float ChunkEndZ = ChunkStartZ + ChunkLength;

        float SafeStartZ = ChunkStartZ + TargetLevelData.SpawnSafeZoneStart;
        float SafeEndZ = ChunkEndZ - TargetLevelData.SpawnSafeZoneEnd;

        if (SafeStartZ >= SafeEndZ) return;

        for (int Index = 0; Index < ObstacleCount; Index++)
        {
            var SelectedData = GetRandomWeightedPrefab(TargetLevelData, TotalWeight);
            if (SelectedData == null || SelectedData.Prefab == null) continue;

            float RandomXValue = Random.Range(-RoadBoundaryX, RoadBoundaryX);
            float RandomZValue = Random.Range(SafeStartZ, SafeEndZ);
            float BaseYPosition = SelectedData.YOffset;

            // Cached collider lookup
            float ColliderBottomOffset = GetCachedColliderOffset(SelectedData.Prefab);

            float FinalYPosition = BaseYPosition + ColliderBottomOffset;
            Vector3 TargetSpawnPosition = new Vector3(RandomXValue, FinalYPosition, RandomZValue);

            Quaternion TargetRotation = SelectedData.UsePrefabRotation ? SelectedData.Prefab.transform.rotation : Quaternion.Euler(0, 90, 0);

            // Use object pool instead of Instantiate
            GameObject SpawnedObstacle = GetFromPool(SelectedData.Prefab, TargetSpawnPosition, TargetRotation, HolderTransform);

            // Ensure marker exists for pool tracking
            var marker = SpawnedObstacle.GetComponent<PooledObjectMarker>();
            if (marker == null)
                marker = SpawnedObstacle.AddComponent<PooledObjectMarker>();
            marker.SourcePrefab = SelectedData.Prefab;

            // --- ANTI-GRAVITY SYSTEM INJECTION ---
            if (!SelectedData.DontModifyRigidbody)
            {
                Rigidbody ObstacleRigidbody = SpawnedObstacle.GetComponent<Rigidbody>();
                if (ObstacleRigidbody != null)
                {
                    ObstacleRigidbody.isKinematic = true;
                    ObstacleRigidbody.velocity = Vector3.zero;
                    ObstacleRigidbody.angularVelocity = Vector3.zero;

                    if (CoroutineHost != null && CoroutineHost.isActiveAndEnabled)
                    {
                        CoroutineHost.StartCoroutine(UnlockPhysicsRoutine(ObstacleRigidbody));
                    }
                }
            }

#if UNITY_EDITOR
            if (UnityEditor.EditorPrefs.GetBool("DevTool_SpawnLogging", false))
            {
                Debug.Log($"<b>[Spawn Log]</b> Spawned <b>{SelectedData.Prefab.name}</b> at position: <color=#3498db>{TargetSpawnPosition}</color>");
            }
#endif
        }
    }

    /// <summary>
    /// Returns the bottom half-extent of the first Collider found on the prefab (cached on first call).
    /// </summary>
    private static float GetCachedColliderOffset(GameObject prefab)
    {
        if (!ColliderCache.TryGetValue(prefab, out float offset))
        {
            Collider col = prefab.GetComponentInChildren<Collider>();
            offset = (col != null) ? col.bounds.extents.y : 0f;
            ColliderCache[prefab] = offset;
        }
        return offset;
    }

    private static void PlaceCoins(LevelData TargetLevelData, Transform HolderTransform, float RoadBoundaryX, float ChunkLength, float ChunkStartZ)
    {
        if (TargetLevelData.CoinPrefab == null) return;

        float ChunkEndZ = ChunkStartZ + ChunkLength;
        float CoinSpacingValue = 20f;

        for (float ZPosition = ChunkStartZ; ZPosition < ChunkEndZ; ZPosition += CoinSpacingValue)
        {
            float RandomXValue = Random.Range(-RoadBoundaryX, RoadBoundaryX);
            Vector3 TargetCoinPosition = new Vector3(RandomXValue, 1f, ZPosition);

            // Use object pool for coins too
            GameObject SpawnedCoin = GetFromPool(TargetLevelData.CoinPrefab, TargetCoinPosition, Quaternion.identity, HolderTransform);
            SpawnedCoin.transform.localEulerAngles = new Vector3(0, 90, 90);

            var marker = SpawnedCoin.GetComponent<PooledObjectMarker>();
            if (marker == null)
                marker = SpawnedCoin.AddComponent<PooledObjectMarker>();
            marker.SourcePrefab = TargetLevelData.CoinPrefab;
        }
    }

    private static WeightedSpawn GetRandomWeightedPrefab(LevelData TargetLevelData, float TotalWeight)
    {
        if (TotalWeight <= 0f) return null;

        float RandomValue = Random.Range(0f, TotalWeight);
        float WeightSumValue = 0f;

        for (int i = 0; i < TargetLevelData.SpawnableObjects.Count; i++)
        {
            WeightSumValue += TargetLevelData.SpawnableObjects[i].Weight;
            if (RandomValue <= WeightSumValue)
            {
                return TargetLevelData.SpawnableObjects[i];
            }
        }

        return TargetLevelData.SpawnableObjects[TargetLevelData.SpawnableObjects.Count - 1];
    }

    // --- COROUTINE-BASED PHYSICS UNLOCKER ---
    private static IEnumerator UnlockPhysicsRoutine(Rigidbody TargetRigidbody)
    {
        yield return new WaitForSeconds(0.15f);

        // Check that the object is still active (not returned to pool)
        if (TargetRigidbody != null && TargetRigidbody.gameObject != null && TargetRigidbody.gameObject.activeInHierarchy)
        {
            TargetRigidbody.isKinematic = false;
        }
    }
}

/// <summary>
/// Attached to pooled objects to track which prefab they originated from,
/// enabling correct return to the right pool queue.
/// </summary>
public class PooledObjectMarker : MonoBehaviour
{
    [HideInInspector] public GameObject SourcePrefab;
}
