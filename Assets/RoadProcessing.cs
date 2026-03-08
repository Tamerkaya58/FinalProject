using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadProcessing : MonoBehaviour
{
public GameObject RoadChunk;
public LevelData levelData;

public RoadChunkQueue _roadQueue;
public GameObject RoadChunkObject;

}

public class RoadChunkQueue
{
    private Queue<GameObject> RoadQueue;
    private int _maxCapacity;
    public RoadChunkQueue(int MaxCapacity)
    {
        _maxCapacity = MaxCapacity;
        RoadQueue = new Queue<GameObject>();
    }
    public void EnqueueChunk(GameObject newChunk)
    {
        RoadQueue.Enqueue(newChunk);
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
        return RoadQueue.Count >= _maxCapacity;
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
