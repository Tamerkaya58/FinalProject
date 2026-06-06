using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject car;
    private Rigidbody rb;

    public GameObject mainMenu;
    public GameObject loseMessage;
    public GameObject winMessage;

    public TMP_Text uiElementSpeed;
    public TMP_Text uiElementPoints;
    public TMP_Text uiElementLevel;

    private string speedUnit;

    // Cached values to avoid updating TMP text every frame
    private int lastDisplayedSpeed = -1;
    private int lastDisplayedPoints = -1;
    private int lastDisplayedLevel = -1;

    private void Start()
    {
        if (car != null)
            rb = car.GetComponent<Rigidbody>();

        UpdateSpeedUnit();

        if (GameManager.restartFromTryAgain)
        {
            if (mainMenu != null) mainMenu.SetActive(false);
            if (loseMessage != null) loseMessage.SetActive(false);
            if (winMessage != null) winMessage.SetActive(false);

            ActivateTextWithParents(uiElementSpeed);
            ActivateTextWithParents(uiElementPoints);
            ActivateTextWithParents(uiElementLevel);
        }
    }

    private void Update()
    {
        if (rb == null) return;

        float speedValue = rb.velocity.magnitude * 3.6f;
        int displayedSpeed;

        if (speedUnit == "MPH")
        {
            displayedSpeed = (int)(speedValue * 0.62f);
            if (displayedSpeed != lastDisplayedSpeed)
            {
                lastDisplayedSpeed = displayedSpeed;
                uiElementSpeed.text = displayedSpeed + " MPH";
            }
        }
        else
        {
            displayedSpeed = (int)speedValue;
            if (displayedSpeed != lastDisplayedSpeed)
            {
                lastDisplayedSpeed = displayedSpeed;
                uiElementSpeed.text = displayedSpeed + " KM/H";
            }
        }

        if (GameManager.instance != null)
        {
            int currentPoints = Mathf.FloorToInt(GameManager.instance.currentPoints);
            if (currentPoints != lastDisplayedPoints)
            {
                lastDisplayedPoints = currentPoints;
                uiElementPoints.text = currentPoints.ToString("N0");
            }

            if (GameManager.level != lastDisplayedLevel)
            {
                lastDisplayedLevel = GameManager.level;
                uiElementLevel.text = "LEVEL : " + lastDisplayedLevel;
            }
        }
    }

    private void ActivateTextWithParents(TMP_Text text)
    {
        if (text == null) return;

        text.gameObject.SetActive(true);

        Transform parent = text.transform.parent;
        while (parent != null)
        {
            parent.gameObject.SetActive(true);
            parent = parent.parent;
        }
    }

    public void UpdateSpeedUnit()
    {
        speedUnit = PlayerPrefs.GetString("SpeedUnitPref", "KMH");
        lastDisplayedSpeed = -1; // Force refresh
    }

    public void QuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;

        GameManager.restartFromTryAgain = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            GameManager.restartFromTryAgain = true;
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
