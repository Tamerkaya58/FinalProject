using UnityEngine;

public class RoadChunk : MonoBehaviour
{
    private Transform SpawnedObjectsHolder;
    private LevelData CurrentLevelData;
    private float RoadBoundaryX;
    private float ChunkLength;
    private int ChunkMaxCapacity = 3;

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

        ChunkSpawner.SpawnContent(CurrentLevelData, SpawnedObjectsHolder, RoadBoundaryX, ChunkLength, this.transform.position.z, ChunkMaxCapacity, this);
    }

    public void ClearChunk()
    {
        if (SpawnedObjectsHolder == null) return;

        // Use for loop instead of foreach to avoid GC alloc from Transform enumerator
        int childCount = SpawnedObjectsHolder.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = SpawnedObjectsHolder.GetChild(i);
            ChunkSpawner.ReturnToPool(child.gameObject);
        }
    }
}
