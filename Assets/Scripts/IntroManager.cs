using UnityEngine;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject introCanvas;
    public VideoPlayer videoPlayer;

    void Start()
    {
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