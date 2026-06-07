using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public static bool restartFromTryAgain = false;

    public Rigidbody carRB;

    [Header("=== Menüler ===")]
    public GameObject mainMenu;
    public GameObject loseScreen;
    public GameObject winScreen;

    [Header("=== Ses ===")]
    public AudioSource musicPlayer;

    [Header("=== Lose Screen UI Textleri ===")]
    public TextMeshProUGUI loseScoreText;
    public TextMeshProUGUI loseHighScoreText;
    public TextMeshProUGUI loseDistanceText;

    [Header("=== Oyun İçi UI Textleri ===")]
    public TextMeshProUGUI inGameScoreText;
    public TextMeshProUGUI inGameDistanceText;

    [Header("=== Puan Ayarları ===")]
    public float pointsPerSecond = 50f;

    public bool gameStarted = false;
    public static int level = 1;

    public float currentPoints = 0f;
    private float distanceTraveledMeters = 0f;
    private int highScore = 0;

    private bool playerStartedDriving = false;
    private float stillTimer = 0f;

    [Header("=== Lose Ayarları ===")]
    [SerializeField] private float moveStartSpeed = 5f;
    [SerializeField] private float stopSpeedLimit = 1f;
    [SerializeField] private float loseDelay = 3f;

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

        highScore = PlayerPrefs.GetInt("HighScore", 0);

        gameStarted = true;
        playerStartedDriving = false;
        stillTimer = 0f;

        currentPoints = 0f;
        distanceTraveledMeters = 0f;

        if (mainMenu != null) mainMenu.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
        if (winScreen != null) winScreen.SetActive(false);

        restartFromTryAgain = false;

        UpdateInGameUI();
    }

    private void Update()
    {
        if (!gameStarted)
            return;

        CheckLose();
        CalculateScoreAndDistance();
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

    private void CalculateScoreAndDistance()
    {
        if (!playerStartedDriving || carRB == null)
            return;

        currentPoints += pointsPerSecond * Time.deltaTime;
        distanceTraveledMeters += carRB.velocity.magnitude * Time.deltaTime;

        UpdateInGameUI();
    }

    private void UpdateInGameUI()
    {
        if (inGameScoreText != null)
            inGameScoreText.text = Mathf.FloorToInt(currentPoints).ToString("N0");

        if (inGameDistanceText != null)
            inGameDistanceText.text = (distanceTraveledMeters / 1000f).ToString("F1") + " km";
    }

    private void CheckLose()
    {
        if (loseScreen != null && loseScreen.activeInHierarchy) return;
        if (winScreen != null && winScreen.activeInHierarchy) return;
        if (carRB == null) return;

        float speedKmh = carRB.velocity.magnitude * 3.6f;

        // Oyun başında araç 5 km/h hıza ulaşmadan lose kontrolü ASLA başlamaz.
        if (!playerStartedDriving)
        {
            if (speedKmh >= 5f)
            {
                playerStartedDriving = true;
                stillTimer = 0f;
            }

            return;
        }

        // Araç 5 km/h gördükten sonra 0'a yakın durursa lose sayacı başlar.
        if (speedKmh <= 0.5f)
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

    public void AddPoints(float amount)
    {
        currentPoints += amount;

        if (currentPoints < 0)
            currentPoints = 0;

        UpdateInGameUI();
    }

    public void LoseGame()
    {
        if (!playerStartedDriving)
            return;

        gameStarted = false;
        stillTimer = 0f;
        playerStartedDriving = false;

        int finalScore = Mathf.FloorToInt(currentPoints);

        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        float distanceKm = distanceTraveledMeters / 1000f;

        if (loseScoreText != null)
            loseScoreText.text = "Score:\n" + finalScore.ToString("N0");

        if (loseHighScoreText != null)
            loseHighScoreText.text = "Highest Score:\n" + highScore.ToString("N0");

        if (loseDistanceText != null)
            loseDistanceText.text = "Distance:\n" + distanceKm.ToString("F1") + " km";

        if (musicPlayer != null)
            musicPlayer.Stop();

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

        currentPoints = 0f;
        distanceTraveledMeters = 0f;

        UpdateInGameUI();

        if (mainMenu != null) mainMenu.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
        if (winScreen != null) winScreen.SetActive(false);

        if (musicPlayer != null && !musicPlayer.isPlaying)
            musicPlayer.Play();
    }
}