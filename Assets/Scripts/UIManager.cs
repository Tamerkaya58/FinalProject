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
        ShowUI();
    }

    private void ShowUI()
    {
        if (rb == null) return;

        float baseSpeed = rb.velocity.magnitude * 3.6f; // Fiziğe uygun olması için 3.6f kullanıyoruz

        if (speedUnit == "MPH")
        {
            int mphSpeed = (int)(baseSpeed * 0.62f);
            uiElementSpeed.text = mphSpeed + " MPH";
        }
        else
        {
            uiElementSpeed.text = ((int)baseSpeed) + " KM/H";
        }

        if (GameManager.instance != null)
        {
            // Puanı tam sayıya yuvarlayarak ekranda göster
            uiElementPoints.text = Mathf.FloorToInt(GameManager.instance.currentPoints).ToString("N0");
            uiElementLevel.text = "LEVEL : " + GameManager.level.ToString();
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
            GameManager.restartFromTryAgain = true; // Auto start next level
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