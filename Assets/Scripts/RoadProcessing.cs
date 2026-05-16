using System.Collections.Generic;
using UnityEngine;

public class RoadProcessing
{
    private LevelData CurrentLevelData;
    private GameObject RoadChunkPrefab;
    private float NextSpawnZPosition = 0f;
    private int ChunkMaxCapacity = 3;
    private int ChunkHowManyWhenTriggered = 1;
    
    private float ChunkLength = 0f; 
    private float RoadBoundaryX = 0f; 
    
    private Queue<GameObject> ActiveChunksQueue;

    public RoadProcessing(LevelData LevelDataInput)
    {
        CurrentLevelData = LevelDataInput;
        RoadChunkPrefab = CurrentLevelData.RoadChunk;

        ActiveChunksQueue = new Queue<GameObject>();

        CalculateRoadBoundaries();
        InitialChunkSpawner();
        SubscribeToTrigger();
    }
    
    private void SubscribeToTrigger()
    {
        RoadTrigger.OnCarPassedTrigger += ChunkChangePlacesWhenTriggered;
    }

    public void Cleanup()
    {
        RoadTrigger.OnCarPassedTrigger -= ChunkChangePlacesWhenTriggered;
    }

    private void CalculateRoadBoundaries()
    {
        if (RoadChunkPrefab == null)
        {
            Debug.LogError("FATAL ERROR: RoadChunkPrefab is null! Falling back to default values.");
            ChunkLength = 100f;
            RoadBoundaryX = 3.2f;
            return;
        }

        // Bounds hesaplaması için özel bir obje verildiyse onu, verilmediyse orijinal prefabi kullan
        GameObject BoundsTarget = CurrentLevelData.BoundsReferencePrefab != null ? CurrentLevelData.BoundsReferencePrefab : RoadChunkPrefab;

        Bounds RoadBounds = new Bounds(BoundsTarget.transform.position, Vector3.zero);
        bool initialized = false;

        Debug.Log($"--- Calculating Bounds for: {BoundsTarget.name} ---");
        // Hesaplamayı yaparken Trigger objelerini dışarıda bırakıyoruz
        foreach (Renderer rend in BoundsTarget.GetComponentsInChildren<Renderer>())
        {
            string logMsg = $"Renderer found: {rend.gameObject.name} | Bounds Size: {rend.bounds.size} | Bounds Center: {rend.bounds.center}";

            // Eğer objede veya ebeveyninde RoadTrigger varsa, isminde "trigger" geçiyorsa veya Tag'i "Trigger" ise yoksay
            if (rend.GetComponent<RoadTrigger>() != null || 
                rend.GetComponentInParent<RoadTrigger>() != null ||
                rend.gameObject.name.ToLower().Contains("trigger") ||
                rend.gameObject.CompareTag("Trigger"))
            {
                Debug.Log($"{logMsg} -> SKIPPED (Trigger component, name match, or Trigger tag)");
                continue;
            }

            // Ayrıca üzerinde isTrigger olan bir Collider varsa yine yoksay
            Collider col = rend.GetComponent<Collider>();
            if (col != null && col.isTrigger)
            {
                Debug.Log($"{logMsg} -> SKIPPED (isTrigger == true)");
                continue;
            }

            Debug.Log($"{logMsg} -> INCLUDED in calculation");

            if (!initialized)
            {
                RoadBounds = rend.bounds;
                initialized = true;
            }
            else
            {
                RoadBounds.Encapsulate(rend.bounds);
            }
        }
        Debug.Log($"--- Final Encapsulated Bounds Size: {RoadBounds.size} | Final ChunkLength (Z): {RoadBounds.size.z} ---");

        if (!initialized || RoadBounds.size == Vector3.zero)
        {
            Debug.LogError("FATAL ERROR: RoadChunk Prefab lacks any valid Renderers to calculate bounds! Falling back to default values.");
            ChunkLength = 100f;
            RoadBoundaryX = 3.2f;
        }
        else
        {
            ChunkLength = RoadBounds.size.z;
            
            float ActualWidth = RoadBounds.size.x;
            
            RoadBoundaryX = (ActualWidth / 2f) * 0.975f;
            
            Debug.Log($"ChunkLength: {ChunkLength}, RoadBoundaryX: {RoadBoundaryX}");
        }
    }

    private void InitialChunkSpawner()
    {
        for (int Index = 0; Index < ChunkMaxCapacity; Index++)
        {
            SpawnAndEnqueueChunk();
        }
    }

    private void SpawnAndEnqueueChunk()
    {
        Vector3 SpawnPosition = new Vector3(0, 0, NextSpawnZPosition);
        
        GameObject NewChunkObject = GameObject.Instantiate(RoadChunkPrefab, SpawnPosition, Quaternion.identity);
        NewChunkObject.name = "RoadChunk_" + NextSpawnZPosition;
        
        RoadChunk ChunkComponent = NewChunkObject.GetComponent<RoadChunk>();
        if (ChunkComponent == null)
        {
            ChunkComponent = NewChunkObject.AddComponent<RoadChunk>();
        }

        ChunkComponent.InitializeChunk(CurrentLevelData, RoadBoundaryX, ChunkLength);
        
        ActiveChunksQueue.Enqueue(NewChunkObject);
        NextSpawnZPosition += ChunkLength;
    }

    public void ChunkChangePlacesWhenTriggered()
    {
        for (int Index = 0; Index < ChunkHowManyWhenTriggered; Index++)
        {
            if (ActiveChunksQueue.Count == 0)
            {
                Debug.LogWarning("RoadProcessing: Kuyrukta taşınacak chunk kalmadı!");
                return;
            }

            GameObject OldestChunkObject = ActiveChunksQueue.Dequeue();

            OldestChunkObject.transform.position = new Vector3(0, 0, NextSpawnZPosition);
            OldestChunkObject.name = "RoadChunk_" + NextSpawnZPosition;

            RoadChunk ChunkComponent = OldestChunkObject.GetComponent<RoadChunk>();
            if (ChunkComponent != null)
            {
                ChunkComponent.InitializeChunk(CurrentLevelData, RoadBoundaryX, ChunkLength);
            }

            ActiveChunksQueue.Enqueue(OldestChunkObject);
            NextSpawnZPosition += ChunkLength;
        }
    }
}

public class RoadChunkQueue
{
    private Queue<GameObject> RoadQueue;
    private int MaxCapacityValue;
    
    public RoadChunkQueue(int MaxCapacityInput)
    {
        MaxCapacityValue = MaxCapacityInput;
        RoadQueue = new Queue<GameObject>();
    }
    
    public void EnqueueChunk(GameObject NewChunk)
    {
        RoadQueue.Enqueue(NewChunk);
    }
    
    public void DequeueAndDestroyOldestChunk()
    {
        if (RoadQueue.Count > 0)
        {
            GameObject OldestChunk = RoadQueue.Dequeue();
            GameObject.Destroy(OldestChunk);
        }
    }
    
    public bool IsAtCapacity()
    {
        return RoadQueue.Count >= MaxCapacityValue;
    }
    
    public void ClearAndDestroyAll() 
    { 
        while (RoadQueue.Count > 0)
        {
            GameObject ChunkToDestroy = RoadQueue.Dequeue();
            GameObject.Destroy(ChunkToDestroy);
        }
    }
}
