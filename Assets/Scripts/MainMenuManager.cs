using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void LoadCityLevel()
    {
        GameManager.restartFromTryAgain = true; // Auto-start the level
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
