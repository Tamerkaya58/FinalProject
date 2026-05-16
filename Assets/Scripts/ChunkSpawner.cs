using UnityEngine;
using System.Threading.Tasks;

public static class ChunkSpawner
{
    public static void SpawnContent(LevelData TargetLevelData, Transform HolderTransform, float RoadBoundaryX, float ChunkLength, float ChunkStartZ, int ChunkMaxCapacity)
    {
        float TotalWeight = CalculateTotalWeight(TargetLevelData);
        PlaceObstacles(TargetLevelData, HolderTransform, RoadBoundaryX, ChunkLength, ChunkStartZ, ChunkMaxCapacity, TotalWeight);
        PlaceCoins(TargetLevelData, HolderTransform, RoadBoundaryX, ChunkLength, ChunkStartZ);
    }

    private static float CalculateTotalWeight(LevelData TargetLevelData)
    {
        float TotalWeightValue = 0f;
        if (TargetLevelData.SpawnableObjects == null) return 0f;
        
        foreach (var SpawnItem in TargetLevelData.SpawnableObjects)
        {
            TotalWeightValue += SpawnItem.Weight;
        }
        return TotalWeightValue;
    }

    private static void PlaceObstacles(LevelData TargetLevelData, Transform HolderTransform, float RoadBoundaryX, float ChunkLength, float ChunkStartZ, int ChunkMaxCapacity, float TotalWeight)
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
            
            float ColliderBottomOffset = 0f;
            Collider ObjectCollider = SelectedData.Prefab.GetComponentInChildren<Collider>();
            
            if (ObjectCollider != null)
            {
                ColliderBottomOffset = ObjectCollider.bounds.extents.y;
            }

            float FinalYPosition = BaseYPosition + ColliderBottomOffset;
            Vector3 TargetSpawnPosition = new Vector3(RandomXValue, FinalYPosition, RandomZValue);

            Quaternion TargetRotation = SelectedData.UsePrefabRotation ? SelectedData.Prefab.transform.rotation : Quaternion.Euler(0, 90, 0);
            GameObject SpawnedObstacle = GameObject.Instantiate(SelectedData.Prefab, TargetSpawnPosition, TargetRotation);
            SpawnedObstacle.transform.SetParent(HolderTransform, true);

            // --- ANTI-GRAVITY SYSTEM INJECTION ---
            if (!SelectedData.DontModifyRigidbody)
            {
                Rigidbody ObstacleRigidbody = SpawnedObstacle.GetComponent<Rigidbody>();
                if (ObstacleRigidbody != null)
                {
                    // Freeze the object immediately upon instantiation to prevent clipping/falling
                    ObstacleRigidbody.isKinematic = true;
                    ObstacleRigidbody.velocity = Vector3.zero;
                    ObstacleRigidbody.angularVelocity = Vector3.zero;

                    // Unlock the physics shortly after it perfectly settles on the coordinate
                    UnlockPhysicsAsync(ObstacleRigidbody);
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

    private static void PlaceCoins(LevelData TargetLevelData, Transform HolderTransform, float RoadBoundaryX, float ChunkLength, float ChunkStartZ)
    {
        if (TargetLevelData.CoinPrefab == null) return;

        float ChunkEndZ = ChunkStartZ + ChunkLength;
        float CoinSpacingValue = 20f;

        for (float ZPosition = ChunkStartZ; ZPosition < ChunkEndZ; ZPosition += CoinSpacingValue)
        {
            float RandomXValue = Random.Range(-RoadBoundaryX, RoadBoundaryX);
            Vector3 TargetCoinPosition = new Vector3(RandomXValue, 1f, ZPosition);

            GameObject SpawnedCoin = GameObject.Instantiate(TargetLevelData.CoinPrefab, TargetCoinPosition, Quaternion.identity);
            SpawnedCoin.transform.localEulerAngles = new Vector3(0, 90, 90);
            SpawnedCoin.transform.SetParent(HolderTransform, true);
        }
    }

    private static WeightedSpawn GetRandomWeightedPrefab(LevelData TargetLevelData, float TotalWeight)
    {
        if (TotalWeight <= 0f) return null;

        float RandomValue = Random.Range(0f, TotalWeight);
        float WeightSumValue = 0f;

        foreach (var SpawnItem in TargetLevelData.SpawnableObjects)
        {
            WeightSumValue += SpawnItem.Weight;
            if (RandomValue <= WeightSumValue)
            {
                return SpawnItem;
            }
        }

        return TargetLevelData.SpawnableObjects[TargetLevelData.SpawnableObjects.Count - 1];
    }

    // --- ASYNCHRONOUS PHYSICS UNLOCKER ---
    private static async void UnlockPhysicsAsync(Rigidbody TargetRigidbody)
    {
        try
        {
            // Delay for 150 milliseconds. This gives Unity's internal physics loop 
            // enough time to register the colliders and static position.
            await Task.Delay(150);
            
            // Check if the object still exists (hasn't been destroyed or disabled during delay)
            if (TargetRigidbody != null && TargetRigidbody.gameObject != null)
            {
                TargetRigidbody.isKinematic = false; // Gravity takes control now.
            }
        }
        catch (System.Exception LoggedException)
        {
            Debug.LogWarning($"Physics unlock task interrupted: {LoggedException.Message}");
        }
    }
}