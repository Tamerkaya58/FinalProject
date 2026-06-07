using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseMenuButtons : MonoBehaviour
{
    public void GoToMapChoose()
    {
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("OpenMapChoose", 1);

        SceneManager.LoadScene("MainMenu");
    }
}