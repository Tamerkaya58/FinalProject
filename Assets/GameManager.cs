using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Rigidbody carRB;

    public GameObject[] obstacles;
    public GameObject coin;

    public GameObject mainMenu;
    public GameObject loseScreen;
    public GameObject winScreen;

    public LevelData CurrentLevel;
    private float TotalWeight;
    
    GameObject statics;

    public static int level = 1;
    int frequency;

    public int points;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

    }
    void Start()
    {
        if (CurrentLevel == null)
        {
            Debug.LogError("CurrentLevel is missing!");
            return;
        }

        statics = GameObject.Find("Statics");
        PlaceObstacles();
        PlaceCoins();
    }
    float CalculateTotalWeight()
    {
        TotalWeight = 0f;
        foreach (var spawn in CurrentLevel.SpawnableObjects)
        {
            TotalWeight += spawn.Weight;
        }
        return TotalWeight;
    }
    private (GameObject prefab, float yValue) GetRandomWeightedPrefab()
    {
        float randomValue = Random.Range(0f, CalculateTotalWeight());
        float WeightSum = 0f;

        foreach (var spawn in CurrentLevel.SpawnableObjects)
        {
            WeightSum += spawn.Weight;
            if (randomValue <= WeightSum)
            {
                return (spawn.Prefab, spawn.Prefab.transform.position.y);

            }
        }

        // Fallback in case of rounding errors
        Debug.LogError($"Weighted Random Failed! RandomValue: {randomValue}, TotalWeight: {TotalWeight}. Returning null.");
        return (null, 0f);
    }

    void PlaceObstacles()
    {
        float randomValue = Random.Range(0f, TotalWeight);
        
        for (int i = 0; i < 2000; i += 100/CurrentLevel.Difficulty)
        {
            var selectedData = GetRandomWeightedPrefab();
            Vector3 random = new Vector3(Random.Range(-3.2f, 3.2f), selectedData.yValue, RandomRoadZ());
            GameObject obs = Instantiate(selectedData.prefab, random, Quaternion.identity);
            obs.transform.localEulerAngles = new Vector3(0, 90, 0);
           // Debug.Log("Placed obstacle " + obs.name + " at: " + random.ToString());
            obs.transform.SetParent(statics.transform);
        }
    }

    // Generates a random coordinate on the road for coin placement
    Vector3 RandomRoadCoordinate()
    {
        Vector3 random = new Vector3(Random.Range(-4.5f, 4.5f), 1f, Random.Range (1 , 1000));
        return random;
    }

    void PlaceCoins()
    {
        for (int i = 0; i < 1000; i += 20)
        {
            GameObject coins = Instantiate(coin, RandomRoadCoordinate(), Quaternion.identity);
            coins.transform.localEulerAngles = new Vector3(0, 90, 90);
            coins.transform.SetParent(statics.transform);
        }
    }

    float RandomRoadZ()
    {
        return Random.Range(0f, 1000f);
    }

    void Update()
    {
        CheckLose();
        CheckLevelOutOfArray();
    }

    void CheckLevelOutOfArray()
    {
        if (level > 3) level = 1;
    }

    void CheckLose()
    {
        if (!mainMenu.activeInHierarchy && !winScreen.activeInHierarchy && !loseScreen.activeInHierarchy)
        {
            if ((int)carRB.velocity.magnitude == 0)
            {
                StartCoroutine(ThreeSecondsOfStayStillCheck());
            }
            else if ((int)carRB.gameObject.transform.position.y < -3)
            {
                loseScreen.SetActive(true);
            }
        }
    }

    IEnumerator ThreeSecondsOfStayStillCheck()
    {
        yield return new WaitForSeconds(3);
        if ((int)carRB.velocity.magnitude == 0)
        {
            loseScreen.SetActive(true);
        }
    }
}


[System.Serializable]
public class WeightedSpawn
{
    public string Name; 
    public GameObject Prefab;
    [Range(0f, 100f)] public float Weight;
}
