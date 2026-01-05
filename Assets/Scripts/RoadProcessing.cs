using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadProcessing : MonoBehaviour
{
public GameObject RoadChunk;
public LevelData levelData;

public RoadChunkQueue _roadQueue;

private GameObject LinkHazards(LevelData data)
{
    return null;
}
public RoadChunkQueue InitializeRoadQueue(int amountToSpawn)
{
    _roadQueue = new RoadChunkQueue(amountToSpawn);

    for (int i = 0; i < amountToSpawn; i++)
    {
        var chunk = SpawnNewChunk(i);
        _roadQueue.EnqueueChunk(chunk);
    }
    return _roadQueue;
}
private GameObject SpawnNewChunk(int i)
{
    return Instantiate(RoadChunk, new Vector3(0,0,CalculateRoadZ(i)), Quaternion.identity);
}
private float CalculateRoadZ(int order)
{
    float ZScale = RoadChunk.transform.localScale.z;
    return ZScale * order;
}

}

public class RoadChunkQueue
{
    private Queue<GameObject> _roadQueue;
    private int _maxCapacity;
    public RoadChunkQueue(int MaxCapacity)
    {
        _maxCapacity = MaxCapacity;
        _roadQueue = new Queue<GameObject>();
    }
    public void EnqueueChunk(GameObject newChunk)
    {
        _roadQueue.Enqueue(newChunk);
    }
    public GameObject DequeueOldestChunk()
    {
        if (_roadQueue.Count >0)
        {
            return _roadQueue.Dequeue();
        }
        return null;
    }
    public bool IsAtCapacity()
    {
        return _roadQueue.Count >= _maxCapacity;
    }
    public GameObject[] queueToarray()
    {
        return _roadQueue.ToArray();
    }
    public Transform returntransform(int index)
    {
       GameObject[] chunklist = queueToarray();
        return chunklist[index].transform;
    }
    public void Clear() 
    { 
        _roadQueue.Clear(); 
    }
}
