using UnityEngine;

public class GameManager2 : MonoBehaviour
{
    [SerializeField] private LevelData CurrentLevel;

    private RoadProcessing RoadProcessor;

    private void Awake()
    {
        if (CurrentLevel != null)
        {
            RoadProcessor = new RoadProcessing(CurrentLevel);
        }
        else
        {
            Debug.LogError("CurrentLevel is missing on GameManager2! Drag your LevelData ScriptableObject.");
        }
    }

    private void OnDestroy()
    {
        RoadProcessor?.Cleanup();
    }
}