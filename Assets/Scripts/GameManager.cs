using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public Rigidbody carRB;

    public GameObject mainMenu;
    public GameObject loseScreen;
    public GameObject winScreen;

    public bool gameStarted = false;

    public static int level = 1;

    public int points;

    private void Awake()
    {
        InitializeSingleton();
    }

    private void InitializeSingleton()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    void Update()
    {
        if (!gameStarted)
            return;

        CheckLose();
        CheckLevelOutOfArray();
    }

    void CheckLevelOutOfArray()
    {
        if (level > 3) level = 1;
    }

    void CheckLose()
    {
        if (!mainMenu.activeInHierarchy && !winScreen.activeInHierarchy && !loseScreen.activeInHierarchy)
        {
            if ((int)carRB.velocity.magnitude == 0)
            {
                StartCoroutine(ThreeSecondsOfStayStillCheck());
            }
            else if ((int)carRB.gameObject.transform.position.y < -3)
            {
                loseScreen.SetActive(true);
            }
        }
    }

    IEnumerator ThreeSecondsOfStayStillCheck()
    {
        yield return new WaitForSeconds(3);

        if (gameStarted && (int)carRB.velocity.magnitude == 0)
        {
            loseScreen.SetActive(true);
        }
    }
}