using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro kullanımı için kütüphaneyi ekledik

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public static bool restartFromTryAgain = false;

    public Rigidbody carRB;

    [Header("=== Menüler ===")]
    public GameObject mainMenu;
    public GameObject loseScreen;
    public GameObject winScreen;

    [Header("=== Lose Screen UI Textleri ===")]
    public TextMeshProUGUI loseScoreText;       // Kazanılan Puan
    public TextMeshProUGUI loseHighScoreText;   // En Yüksek Puan
    public TextMeshProUGUI loseDistanceText;    // Gidilen Mesafe (km)

    [Header("=== Oyun İçi UI Textleri (İsteğe Bağlı) ===")]
    public TextMeshProUGUI inGameScoreText;     // Oynarken skoru görmek istersen
    public TextMeshProUGUI inGameDistanceText;  // Oynarken mesafeyi görmek istersen

    [Header("=== Puan Ayarları ===")]
    public float pointsPerSecond = 50f; // Saniyede kazanılacak puan

    public bool gameStarted = false;

    public static int level = 1;

    // Skor ve Mesafe Değişkenleri
    public float currentPoints = 0f;
    private float distanceTraveledMeters = 0f;
    private int highScore = 0;

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

        // En yüksek skoru sistemden çek (Daha önce kaydedilmişse)
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        gameStarted = true;
        playerStartedDriving = false;
        stillTimer = 0f;

        // Değerleri sıfırla
        currentPoints = 0f;
        distanceTraveledMeters = 0f;

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
        // Oyuncu sürmeye başladıysa hesaplamaları yap
        if (playerStartedDriving && carRB != null)
        {
            // Zaman bazlı puan artışı
            currentPoints += pointsPerSecond * Time.deltaTime;

            // Mesafe ölçümü: Aracın hızı (m/s) * geçen zaman = alınan yol (metre)
            distanceTraveledMeters += carRB.velocity.magnitude * Time.deltaTime;

            // Eğer oyun içi UI'lara atama yaptıysan anlık olarak güncelle
            if (inGameScoreText != null)
                inGameScoreText.text = Mathf.FloorToInt(currentPoints).ToString("N0");

            if (inGameDistanceText != null)
                inGameDistanceText.text = (distanceTraveledMeters / 1000f).ToString("F1") + " km";
        }
    }

    private void CheckLose()
    {
        if (loseScreen != null && loseScreen.activeInHierarchy) return;
        if (winScreen != null && winScreen.activeInHierarchy) return;
        if (carRB == null) return;

        float speedKmh = carRB.velocity.magnitude * 3.6f; // m/s'yi km/h'ye çevirmek için 3.6 ile çarpmak daha doğru

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
        gameStarted = false; // Puan sistemi ve fizik hesaplamaları durur
        stillTimer = 0f;
        playerStartedDriving = false;

        // Skoru integer'a yuvarla
        int finalScore = Mathf.FloorToInt(currentPoints);

        // Yeni skor en yüksek skordan büyükse kaydet
        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save(); // Değişiklikleri diske yaz
        }

        // Metreyi kilometreye çevir
        float distanceKm = distanceTraveledMeters / 1000f;

        // UI Text atamaları
        if (loseScoreText != null)
            loseScoreText.text = "SKORUNUZ:\n" + finalScore.ToString("N0");

        if (loseHighScoreText != null)
            loseHighScoreText.text = "EN YÜKSEK SKOR:\n" + highScore.ToString("N0");

        if (loseDistanceText != null)
            loseDistanceText.text = "GİDİLEN MESAFE:\n" + distanceKm.ToString("F1") + " km";

        Time.timeScale = 0f; // Oyunu durdur

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

        if (mainMenu != null) mainMenu.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
        if (winScreen != null) winScreen.SetActive(false);
    }
}