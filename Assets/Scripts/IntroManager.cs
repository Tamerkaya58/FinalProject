using UnityEngine;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject introCanvas;
    public VideoPlayer videoPlayer;

    void Start()
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorPrefs.GetBool("DevTool_SkipIntro", false))
        {
            introCanvas.SetActive(false);
            mainMenu.SetActive(false);

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

        videoPlayer.loopPointReached += IntroFinished;
        videoPlayer.Play();
    }

    void IntroFinished(VideoPlayer vp)
    {
        introCanvas.SetActive(false);
        mainMenu.SetActive(true);
    }
}