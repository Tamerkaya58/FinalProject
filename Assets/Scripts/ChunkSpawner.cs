using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChunkSpawner
{
    private static Dictionary<GameObject, Queue<GameObject>> Pool = new Dictionary<GameObject, Queue<GameObject>>();
    private static Dictionary<GameObject, float> ColliderCache = new Dictionary<GameObject, float>();

    public static void ClearPool()
    {
        Pool.Clear();
        ColliderCache.Clear();
    }

    private static GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null) return null;

        if (!Pool.ContainsKey(prefab))
            Pool[prefab] = new Queue<GameObject>();

        GameObject instance = null;

        while (Pool[prefab].Count > 0 && instance == null)
        {
            instance = Pool[prefab].Dequeue();
        }

        if (instance != null)
        {
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

        PooledObjectMarker marker = instance.GetComponent<PooledObjectMarker>();

        if (marker == null || marker.SourcePrefab == null)
        {
            GameObject.Destroy(instance);
            return;
        }

        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        instance.SetActive(false);
        instance.transform.SetParent(null);

        if (!Pool.ContainsKey(marker.SourcePrefab))
            Pool[marker.SourcePrefab] = new Queue<GameObject>();

        Pool[marker.SourcePrefab].Enqueue(instance);
    }

    public static void SpawnContent(LevelData TargetLevelData, Transform HolderTransform, float RoadBoundaryX, float ChunkLength, float ChunkStartZ, int ChunkMaxCapacity, MonoBehaviour CoroutineHost)
    {
        if (TargetLevelData == null || HolderTransform == null) return;

        float TotalWeight = CalculateTotalWeight(TargetLevelData);

        PlaceObstacles(TargetLevelData, HolderTransform, RoadBoundaryX, ChunkLength, ChunkStartZ, ChunkMaxCapacity, TotalWeight, CoroutineHost);
        PlaceCoins(TargetLevelData, HolderTransform, RoadBoundaryX, ChunkLength, ChunkStartZ);
    }

    private static float CalculateTotalWeight(LevelData TargetLevelData)
    {
        float TotalWeightValue = 0f;

        if (TargetLevelData.SpawnableObjects == null)
            return 0f;

        for (int i = 0; i < TargetLevelData.SpawnableObjects.Count; i++)
        {
            if (TargetLevelData.SpawnableObjects[i] != null && TargetLevelData.SpawnableObjects[i].Prefab != null)
            {
                TotalWeightValue += TargetLevelData.SpawnableObjects[i].Weight;
            }
        }

        return TotalWeightValue;
    }

    private static void PlaceObstacles(LevelData TargetLevelData, Transform HolderTransform, float RoadBoundaryX, float ChunkLength, float ChunkStartZ, int ChunkMaxCapacity, float TotalWeight, MonoBehaviour CoroutineHost)
    {
        if (TargetLevelData.SpawnableObjects == null || TargetLevelData.SpawnableObjects.Count == 0)
            return;

        if (TotalWeight <= 0f)
            return;

        int ObstacleCount = Mathf.Max(1, (TargetLevelData.Difficulty * 20) / ChunkMaxCapacity);

        float ChunkEndZ = ChunkStartZ + ChunkLength;
        float SafeStartZ = ChunkStartZ + TargetLevelData.SpawnSafeZoneStart;
        float SafeEndZ = ChunkEndZ - TargetLevelData.SpawnSafeZoneEnd;

        if (SafeStartZ >= SafeEndZ)
            return;

        for (int Index = 0; Index < ObstacleCount; Index++)
        {
            WeightedSpawn SelectedData = GetRandomWeightedPrefab(TargetLevelData, TotalWeight);

            if (SelectedData == null || SelectedData.Prefab == null)
                continue;

            float RandomXValue = Random.Range(-RoadBoundaryX, RoadBoundaryX);
            float RandomZValue = Random.Range(SafeStartZ, SafeEndZ);
            float BaseYPosition = SelectedData.YOffset;

            float ColliderBottomOffset = GetCachedColliderOffset(SelectedData.Prefab);
            float FinalYPosition = BaseYPosition + ColliderBottomOffset;

            Vector3 TargetSpawnPosition = new Vector3(RandomXValue, FinalYPosition, RandomZValue);

            Quaternion TargetRotation = SelectedData.UsePrefabRotation
                ? SelectedData.Prefab.transform.rotation
                : Quaternion.Euler(0, 90, 0);

            GameObject SpawnedObstacle = GetFromPool(SelectedData.Prefab, TargetSpawnPosition, TargetRotation, HolderTransform);

            if (SpawnedObstacle == null)
                continue;

            PooledObjectMarker marker = SpawnedObstacle.GetComponent<PooledObjectMarker>();

            if (marker == null)
                marker = SpawnedObstacle.AddComponent<PooledObjectMarker>();

            marker.SourcePrefab = SelectedData.Prefab;

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
        }
    }

    private static float GetCachedColliderOffset(GameObject prefab)
    {
        if (prefab == null) return 0f;

        if (!ColliderCache.TryGetValue(prefab, out float offset))
        {
            Collider col = prefab.GetComponentInChildren<Collider>();
            offset = col != null ? col.bounds.extents.y : 0f;
            ColliderCache[prefab] = offset;
        }

        return offset;
    }

    private static void PlaceCoins(LevelData TargetLevelData, Transform HolderTransform, float RoadBoundaryX, float ChunkLength, float ChunkStartZ)
    {
        if (TargetLevelData.CoinPrefab == null)
            return;

        float ChunkEndZ = ChunkStartZ + ChunkLength;
        float CoinSpacingValue = 20f;

        for (float ZPosition = ChunkStartZ; ZPosition < ChunkEndZ; ZPosition += CoinSpacingValue)
        {
            float RandomXValue = Random.Range(-RoadBoundaryX, RoadBoundaryX);
            Vector3 TargetCoinPosition = new Vector3(RandomXValue, 1f, ZPosition);

            GameObject SpawnedCoin = GetFromPool(TargetLevelData.CoinPrefab, TargetCoinPosition, Quaternion.identity, HolderTransform);

            if (SpawnedCoin == null)
                continue;

            SpawnedCoin.transform.localEulerAngles = new Vector3(0, 90, 90);

            PooledObjectMarker marker = SpawnedCoin.GetComponent<PooledObjectMarker>();

            if (marker == null)
                marker = SpawnedCoin.AddComponent<PooledObjectMarker>();

            marker.SourcePrefab = TargetLevelData.CoinPrefab;
        }
    }

    private static WeightedSpawn GetRandomWeightedPrefab(LevelData TargetLevelData, float TotalWeight)
    {
        if (TotalWeight <= 0f)
            return null;

        float RandomValue = Random.Range(0f, TotalWeight);
        float WeightSumValue = 0f;

        for (int i = 0; i < TargetLevelData.SpawnableObjects.Count; i++)
        {
            WeightedSpawn current = TargetLevelData.SpawnableObjects[i];

            if (current == null || current.Prefab == null)
                continue;

            WeightSumValue += current.Weight;

            if (RandomValue <= WeightSumValue)
                return current;
        }

        return null;
    }

    private static IEnumerator UnlockPhysicsRoutine(Rigidbody TargetRigidbody)
    {
        yield return new WaitForSeconds(0.15f);

        if (TargetRigidbody != null && TargetRigidbody.gameObject != null && TargetRigidbody.gameObject.activeInHierarchy)
        {
            TargetRigidbody.isKinematic = false;
        }
    }
}

public class PooledObjectMarker : MonoBehaviour
{
    [HideInInspector] public GameObject SourcePrefab;
}