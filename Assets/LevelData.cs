using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int Difficulty = 1;
    // You can add more later: SpeedMultiplier, SkyboxMaterial, etc.
    public List<WeightedSpawn> SpawnableObjects;
}