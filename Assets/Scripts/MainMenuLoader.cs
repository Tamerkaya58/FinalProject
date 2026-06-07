using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLoader : MonoBehaviour
{
    public void GoToMainMenu()
    {
        PlayerPrefs.SetInt("OpenMapChoose", 1);
        SceneManager.LoadScene("MainMenu");
    }
}