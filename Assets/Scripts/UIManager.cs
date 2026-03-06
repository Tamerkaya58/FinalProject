using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject car;
    Rigidbody rb;

    public GameObject mainMenu;

    public TMP_Text uiElementSpeed;
    public TMP_Text uiElementPoints;
    public TMP_Text uiElementLevel;

    private void Start()
    {

        rb = car.GetComponent<Rigidbody>();
    }
    void Update()
    {
        ShowUI();
    }

    void ShowUI()
    {
        uiElementSpeed.text = ((int)(rb.velocity.magnitude * 5)).ToString() + " km/h";
        uiElementPoints.text = GameManager.instance.points.ToString();
        uiElementLevel.text = "LEVEL : " + GameManager.level.ToString();
    }

    public void QuitApp()
    {
        Debug.Log("QUIT CLICK GELDI");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }


    public void TryAgain()
    {
        SceneManager.LoadScene(0);
        mainMenu.SetActive(false);
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(0);
        mainMenu.SetActive(false);
    }
}
