using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;


public class RoadProcessing
{
    private LevelData CurrentLevelData;
    private GameObject RoadChunkPrefab;
    private float NextSpawnZPosition = 0f;
    private int ChunkMaxCapacity = 3;
    private int ChunkHowManyWhenTriggered = 1;
    
    // Unuttuğun ve benim eklediğim hayati değişkenler:
    private float ChunkLength = 0f; 
    private float RoadBoundaryX = 0f; 
    
    private Queue<GameObject> ActiveChunksQueue;
    // Constructor: GameManager bu sınıfı yaratırken verileri buraya kusar.
    public RoadProcessing(LevelData LevelDataInput)
    {
        CurrentLevelData = LevelDataInput;
        RoadChunkPrefab = CurrentLevelData.RoadChunk;

        ActiveChunksQueue = new Queue<GameObject>();

        // 1. ADIM: Önce sınırları ve uzunluğu kesin olarak hesapla!
        CalculateRoadBoundaries();
        
        // 2. ADIM: Artık elimizde ChunkLength olduğu için yolları dizebiliriz.
        InitialChunkSpawner();
    }
    private void CalculateRoadBoundaries()
    {
        // Yoldaki fiziksel çarpışma kutusunu (BoxCollider) çekiyoruz.
        BoxCollider ChunkCollider = RoadChunkPrefab.GetComponent<BoxCollider>();

        if (ChunkCollider != null)
        {
            // Z Ekseni: Uzunluğu Scale ve Collider Size çarparak bul.
            ChunkLength = ChunkCollider.size.z * RoadChunkPrefab.transform.localScale.z;
            
            // X Ekseni: Genişliği hesapla ve %10 güvenlik payı (margin) bırak.
            float ActualWidth = ChunkCollider.size.x * RoadChunkPrefab.transform.localScale.x;
            RoadBoundaryX = (ActualWidth / 2f) * 0.9f;
        }
        else
        {
            // Sistemin çökmesini engelleyen güvenlik duvarı (Failsafe)
            Debug.LogError("FATAL ERROR: RoadChunk Prefab lacks a BoxCollider! Falling back to default values.");
            ChunkLength = 100f;
            RoadBoundaryX = 3.2f;
        }
    }

    private void InitialChunkSpawner()
    {
        for (int Index = 0; Index < ChunkMaxCapacity; Index++)
        {
            // 1. Pozisyonu belirle
            Vector3 SpawnPosition = new Vector3(0, 0, NextSpawnZPosition);
            
            // 2. Objeyi yarat ve isimlendir
            GameObject NewChunk = GameObject.Instantiate(RoadChunkPrefab, SpawnPosition, Quaternion.identity);
            NewChunk.name = "RoadChunk_" + NextSpawnZPosition;
            
            // 3. Kuyruğa (hafızaya) kaydet
            ActiveChunksQueue.Enqueue(NewChunk);
            
            // 4. Bir sonraki yolun konumunu dinamik uzunluk kadar ileri it
            NextSpawnZPosition += ChunkLength;
        }
    }
    private void GetWeightedObjects()
    {
        
    }
    public void ChunkChangePlacesWhenTriggered()
    {
        //en arkadaki chunk ChunkHowmany when triggered'a göre kaç en arkadadan kaç tanesini en öne geçireceğini bulur
    }
    private void PlaceObjects()
    {
        //places the next with difficulty scalar and 
    }

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
