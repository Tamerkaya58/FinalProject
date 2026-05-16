using System.Collections.Generic;
using UnityEngine;

public class RoadChunk : MonoBehaviour
{
    private Transform SpawnedObjectsHolder;
    private LevelData CurrentLevelData;
    private float RoadBoundaryX;
    private float ChunkLength;
    private int ChunkMaxCapacity = 3;
    private float TotalWeight = 0f;

    private void Awake()
    {
        GameObject HolderObject = new GameObject("SpawnedObjectsHolder");
        HolderObject.transform.SetParent(this.transform);
        HolderObject.transform.localPosition = Vector3.zero;
        SpawnedObjectsHolder = HolderObject.transform;
    }

    public void InitializeChunk(LevelData LevelDataInput, float BoundaryX, float Length)
    {
        CurrentLevelData = LevelDataInput;
        RoadBoundaryX = BoundaryX;
        ChunkLength = Length;

        ClearChunk();

        // Delegate spawning logic to the new decoupled spawner class
        ChunkSpawner.SpawnContent(CurrentLevelData, SpawnedObjectsHolder, RoadBoundaryX, ChunkLength, this.transform.position.z, ChunkMaxCapacity);
    }

    public void ClearChunk()
    {
        if (SpawnedObjectsHolder != null)
        {
            foreach (Transform ChildTransform in SpawnedObjectsHolder)
            {
                Destroy(ChildTransform.gameObject);
            }
        }
    }
}
