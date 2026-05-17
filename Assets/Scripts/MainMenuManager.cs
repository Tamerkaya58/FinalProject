using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menü Panelleri")]
    public GameObject mainMenuPanel;      
    public GameObject mapSelectionPanel;  

    private void Start()
    {
        
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (mapSelectionPanel != null) mapSelectionPanel.SetActive(false);
    }

    public void OpenMapSelection()
    {
        mainMenuPanel.SetActive(false);
        mapSelectionPanel.SetActive(true);
    }

    
    public void BackToMainMenu()
    {
        mapSelectionPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

   

    public void LoadCityLevel()
    {
        GameManager.restartFromTryAgain = true; 
        SceneManager.LoadScene("Level_City");
    }

    public void LoadDesertLevel()
    {
        GameManager.restartFromTryAgain = true;
        SceneManager.LoadScene("Level_Desert");
    }

    public void LoadSnowLevel()
    {
        GameManager.restartFromTryAgain = true;
        SceneManager.LoadScene("Level_Snow");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}