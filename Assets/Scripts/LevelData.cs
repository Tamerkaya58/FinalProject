using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int Difficulty = 1;
    public float DifficultyGrowth = 0.1f;
    public GameObject RoadChunk;
    public GameObject CoinPrefab;
    public GameObject BarrierPrefab;
    [Range(0f, 1f)] public float BarrierSpawnChance = 0.5f;
    // You can add more later: SpeedMultiplier, SkyboxMaterial, etc.
    public List<WeightedSpawn> SpawnableObjects;
}

[System.Serializable]
public class WeightedSpawn
{
    public string Name; 
    public GameObject Prefab;
    [Range(0f, 100f)] public float Weight;
    // Objenin yerden kaç birim yukarıda spawn edileceği (0 = zemin seviyesi)
    public float YOffset = 0f;
}