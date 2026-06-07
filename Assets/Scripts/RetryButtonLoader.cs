using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryButtonLoader : MonoBehaviour
{
    public void RetryCurrentLevel()
    {
        Time.timeScale = 1f;

        ChunkSpawner.ClearPool();

        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}