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

    private string speedUnit; // Ayarlardan çekeceğimiz birim bilgisi

    private void Start()
    {
        rb = car.GetComponent<Rigidbody>();

        // Oyun başladığında kaydedilmiş birim ayarını çek (Varsayılan KMH)
        UpdateSpeedUnit();
    }

    void Update()
    {
        ShowUI();
    }

    void ShowUI()
    {
        // Senin belirlediğin temel hız değeri (km/h olarak)
        float baseSpeed = rb.velocity.magnitude * 5f;

        // Seçilen birime göre ekrana yazdır ve hesapla
        if (speedUnit == "MPH")
        {
            int mphSpeed = (int)(baseSpeed * 0.62f); // KM'yi Mil'e çevir
            uiElementSpeed.text = mphSpeed.ToString() + " MPH";
        }
        else
        {
            // Varsayılan KM/H durumu
            uiElementSpeed.text = ((int)baseSpeed).ToString() + " KM/H";
        }

        uiElementPoints.text = GameManager.instance.points.ToString();
        uiElementLevel.text = "LEVEL : " + GameManager.level.ToString();
    }

    // Eğer oyun içindeyken ayar değiştirilirse, SettingsManager'dan bu fonksiyonu çağırabiliriz
    public void UpdateSpeedUnit()
    {
        speedUnit = PlayerPrefs.GetString("SpeedUnitPref", "KMH");
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