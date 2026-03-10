using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager2 : MonoBehaviour
{
    public static GameManager instance { get; private set; }
// Inspector üzerinden bağlayacağın LevelData
    [SerializeField] private LevelData CurrentLevel;

    // RoadProcessing referansını burada tutuyoruz
    private RoadProcessing RoadProcessor;

    private void Awake()
    {
        // Sahne ilk açıldığında çalışan yer burasıdır.
        if (CurrentLevel != null)
        {
            // İşçiyi (RoadProcessing) yaratıyoruz ve LevelData'yı ona teslim ediyoruz.
            // Bu işlem sahne boyunca sadece 1 kez yapılır.
            RoadProcessor = new RoadProcessing(CurrentLevel);
        }
        else
        {
            Debug.LogError("CurrentLevel is missing on GameManager! Drag your ScriptableObject.");
        }
    }
}
