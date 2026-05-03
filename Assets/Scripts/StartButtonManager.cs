using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButtonManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject speedUI;
    public GameObject pointsUI;
    public GameObject levelUI;

    public void StartGame()
    {
        mainMenu.SetActive(false);

        speedUI.SetActive(true);
        pointsUI.SetActive(true);
        levelUI.SetActive(true);

        GameManager.instance.gameStarted = true;
    }
}