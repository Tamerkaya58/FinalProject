using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager instance { get; private set; }
    // References
    public Rigidbody carRB;
    // UI
    public GameObject mainMenu;
    public GameObject loseScreen;
    public GameObject winScreen;

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
        if ((int)carRB.velocity.magnitude == 0)
        {
            loseScreen.SetActive(true);
        }
    }
}

