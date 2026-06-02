using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public static bool restartFromTryAgain = false;

    public Rigidbody carRB;

    public GameObject mainMenu;
    public GameObject loseScreen;
    public GameObject winScreen;

    public bool gameStarted = false;

    public static int level = 1;
    public int points;

    private bool playerStartedDriving = false;
    private float stillTimer = 0f;

    private float moveStartSpeed = 5f;
    private float stopSpeedLimit = 1f;
    private float loseDelay = 3f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        SetLevelByScene();

        gameStarted = true;
        playerStartedDriving = false;
        stillTimer = 0f;

        if (mainMenu != null) mainMenu.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
        if (winScreen != null) winScreen.SetActive(false);

        restartFromTryAgain = false;
    }

    private void Update()
    {
        if (!gameStarted)
            return;

        CheckLose();
    }

    private void SetLevelByScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Level_City")
            level = 1;
        else if (sceneName == "Level_Desert")
            level = 2;
        else if (sceneName == "Level_Snow")
            level = 3;
    }

    private void CheckLose()
    {
        if (loseScreen != null && loseScreen.activeInHierarchy) return;
        if (winScreen != null && winScreen.activeInHierarchy) return;
        if (carRB == null) return;

        float speedKmh = carRB.velocity.magnitude * 5f;

        if (!playerStartedDriving)
        {
            if (speedKmh >= moveStartSpeed)
            {
                playerStartedDriving = true;
                stillTimer = 0f;
            }

            return;
        }

        if (speedKmh <= stopSpeedLimit)
        {
            stillTimer += Time.deltaTime;

            if (stillTimer >= loseDelay)
            {
                LoseGame();
            }
        }
        else
        {
            stillTimer = 0f;
        }
    }

    public void LoseGame()
    {
        gameStarted = false;
        stillTimer = 0f;
        playerStartedDriving = false;

        Time.timeScale = 0f;

        if (loseScreen != null)
            loseScreen.SetActive(true);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        gameStarted = true;
        stillTimer = 0f;
        playerStartedDriving = false;

        if (mainMenu != null) mainMenu.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
        if (winScreen != null) winScreen.SetActive(false);
    }
}