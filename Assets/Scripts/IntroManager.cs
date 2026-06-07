using UnityEngine;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject introCanvas;
    public VideoPlayer videoPlayer;

    public AudioSource musicPlayer;

    void Start()
    {
        // Lose ekranýndan Main Menu'ye dönüldüyse
        if (PlayerPrefs.GetInt("OpenMapChoose", 0) == 1)
        {
            PlayerPrefs.SetInt("OpenMapChoose", 0);

            if (introCanvas != null)
                introCanvas.SetActive(false);

            if (mainMenu != null)
                mainMenu.SetActive(true);

            if (musicPlayer != null)
                musicPlayer.Play();

            return;
        }

#if UNITY_EDITOR
        if (UnityEditor.EditorPrefs.GetBool("DevTool_SkipIntro", false))
        {
            introCanvas.SetActive(false);
            mainMenu.SetActive(false);

            if (musicPlayer != null)
                musicPlayer.Play();

            StartButtonManager startBtn = FindObjectOfType<StartButtonManager>(true);
            if (startBtn != null)
            {
                startBtn.StartGame();
            }
            return;
        }
#endif

        mainMenu.SetActive(false);
        introCanvas.SetActive(true);

        if (musicPlayer != null)
            musicPlayer.Stop();

        videoPlayer.loopPointReached += IntroFinished;
        videoPlayer.Play();
    }

    void IntroFinished(VideoPlayer vp)
    {
        introCanvas.SetActive(false);
        mainMenu.SetActive(true);

        if (musicPlayer != null)
            musicPlayer.Play();
    }
}