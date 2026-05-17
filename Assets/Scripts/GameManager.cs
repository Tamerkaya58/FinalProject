using UnityEngine;

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

    private float stillTimer = 0f;
    private float stillSpeedLimit = 0.25f;
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
        stillTimer = 0f;

        if (mainMenu == null)
        {
            // If there's no main menu in this scene, we auto-start
            gameStarted = true;
            if (loseScreen != null) loseScreen.SetActive(false);
            if (winScreen != null) winScreen.SetActive(false);
        }

        if (restartFromTryAgain)
        {
            restartFromTryAgain = false;

            gameStarted = true;

            if (mainMenu != null) mainMenu.SetActive(false);
            if (loseScreen != null) loseScreen.SetActive(false);
            if (winScreen != null) winScreen.SetActive(false);
        }
    }

    private void Update()
    {
        if (!gameStarted)
            return;

        CheckLevelOutOfArray();
        CheckLose();
    }

    private void CheckLevelOutOfArray()
    {
        if (level > 3)
            level = 1;
    }

    private void CheckLose()
    {
        if (mainMenu != null && mainMenu.activeInHierarchy) return;
        if (winScreen != null && winScreen.activeInHierarchy) return;
        if (loseScreen != null && loseScreen.activeInHierarchy) return;
        if (carRB == null) return;

        if (carRB.transform.position.y < -3f)
        {
            LoseGame();
            return;
        }

        if (carRB.velocity.magnitude < stillSpeedLimit)
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

        Time.timeScale = 0f;

        if (loseScreen != null)
            loseScreen.SetActive(true);
    }

    public void StartGame()
    {
        gameStarted = true;
        stillTimer = 0f;

        if (mainMenu != null)
            mainMenu.SetActive(false);

        if (loseScreen != null)
            loseScreen.SetActive(false);

        if (winScreen != null)
            winScreen.SetActive(false);
    }
}