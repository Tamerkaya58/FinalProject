using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [Header("Ana Paneller")]
    public GameObject settingsMainPanel;
    public GameObject generalSubPanel;
    public GameObject audioSubPanel;
    public GameObject displaySubPanel;

    [Header("Ses Ayarlarý (UI Sliders)")]
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider engineSlider;

    [Header("Buton Referanslarý (Görsel Ýçin)")]
    public Button btnResHD;
    public Button btnResQHD;
    public Button btnQualLow;
    public Button btnQualUltra;
    public Button btnFPS60;
    public Button btnFPS120;
    public Button btnCam1st;
    public Button btnCam3rd;
    public Button btnUnitKMH;
    public Button btnUnitMPH;

    [Header("Seçim Renkleri")]
    public Color selectedColor = Color.green; 
    public Color normalColor = Color.white;  
    [Header("Kamera Referansý")]
    public CameraFollow cameraFollowScript;

    private void Start()
    {
        LoadSettings();
    }

   
    public void OpenSettingsFromMainMenu()
    {
        settingsMainPanel.SetActive(true);

        
        LoadSettings();

        ShowGeneralPanel();
    }

    public void ShowGeneralPanel() { DeactivateAllSubPanels(); generalSubPanel.SetActive(true); }
    public void ShowAudioPanel() { DeactivateAllSubPanels(); audioSubPanel.SetActive(true); }
    public void ShowDisplayPanel() { DeactivateAllSubPanels(); displaySubPanel.SetActive(true); }

    private void DeactivateAllSubPanels()
    {
        if (generalSubPanel != null) generalSubPanel.SetActive(false);
        if (audioSubPanel != null) audioSubPanel.SetActive(false);
        if (displaySubPanel != null) displaySubPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        settingsMainPanel.SetActive(false);
    }

    //  SES AYARLARI 
    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVol", volume);
        PlayerPrefs.SetFloat("MasterVolumePref", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVol", volume);
        PlayerPrefs.SetFloat("MusicVolumePref", volume);
    }

    public void SetEngineVolume(float volume)
    {
        audioMixer.SetFloat("EngineVol", volume);
        PlayerPrefs.SetFloat("EngineVolumePref", volume);
    }

    //  GÖRÜNTÜ AYARLARI 
    public void SetResolutionHD()
    {
        Screen.SetResolution(1920, 1080, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionPref", 1080);
        UpdateButtonVisuals(btnResHD, btnResQHD);
        Debug.Log("Çözünürlük HD (1920x1080) yapýldý.");
    }

    public void SetResolutionQHD()
    {
        Screen.SetResolution(2560, 1440, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionPref", 1440);
        UpdateButtonVisuals(btnResQHD, btnResHD);
        Debug.Log("Çözünürlük QHD (2560x1440) yapýldý.");
    }

    public void SetQualityLow()
    {
        QualitySettings.SetQualityLevel(0);
        PlayerPrefs.SetInt("QualityPref", 0);
        UpdateButtonVisuals(btnQualLow, btnQualUltra);
        Debug.Log("Grafik kalitesi DÜÞÜK yapýldý.");
    }

    public void SetQualityUltra()
    {
        QualitySettings.SetQualityLevel(5);
        PlayerPrefs.SetInt("QualityPref", 5);
        UpdateButtonVisuals(btnQualUltra, btnQualLow);
        Debug.Log("Grafik kalitesi ULTRA yapýldý.");
    }

    public void SetFPS60()
    {
        Application.targetFrameRate = 60;
        PlayerPrefs.SetInt("FPSPref", 60);
        UpdateButtonVisuals(btnFPS60, btnFPS120);
        Debug.Log("FPS Limiti 60'a sabitlendi.");
    }

    public void SetFPS120()
    {
        Application.targetFrameRate = 120;
        PlayerPrefs.SetInt("FPSPref", 120);
        UpdateButtonVisuals(btnFPS120, btnFPS60);
        Debug.Log("FPS Limiti 120'ye sabitlendi.");
    }

  
    public void SetCameraFirstPerson()
    {
        PlayerPrefs.SetInt("CameraViewPref", 0);
        PlayerPrefs.Save();
        UpdateButtonVisuals(btnCam1st, btnCam3rd);

        if (cameraFollowScript != null) cameraFollowScript.UpdateCameraView();
        else Debug.LogError("HATA: CameraFollow scripti SettingsManager'a atanmamýþ!");
    }

    public void SetCameraThirdPerson()
    {
        PlayerPrefs.SetInt("CameraViewPref", 1);
        PlayerPrefs.Save();
        UpdateButtonVisuals(btnCam3rd, btnCam1st);

        if (cameraFollowScript != null) cameraFollowScript.UpdateCameraView();
        else Debug.LogError("HATA: CameraFollow scripti SettingsManager'a atanmamýþ!");
    }

    public void SetUnitsKMH()
    {
        PlayerPrefs.SetString("SpeedUnitPref", "KMH");
        PlayerPrefs.Save();
        UpdateButtonVisuals(btnUnitKMH, btnUnitMPH);

        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null) uiManager.UpdateSpeedUnit();
    }

    public void SetUnitsMPH()
    {
        PlayerPrefs.SetString("SpeedUnitPref", "MPH");
        PlayerPrefs.Save();
        UpdateButtonVisuals(btnUnitMPH, btnUnitKMH);

        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null) uiManager.UpdateSpeedUnit();
    }

   
    private void UpdateButtonVisuals(Button selectedBtn, Button unselectedBtn)
    {
        if (selectedBtn != null) selectedBtn.image.color = selectedColor;
        if (unselectedBtn != null) unselectedBtn.image.color = normalColor;
    }

    private void LoadSettings()
    {
        // Ses
        if (masterSlider != null) masterSlider.value = PlayerPrefs.GetFloat("MasterVolumePref", 0f);
        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat("MusicVolumePref", 0f);
        if (engineSlider != null) engineSlider.value = PlayerPrefs.GetFloat("EngineVolumePref", 0f);

        // Çözünürlük Butonlarý
        int savedRes = PlayerPrefs.GetInt("ResolutionPref", 1080);
        if (savedRes == 1080) { Screen.SetResolution(1920, 1080, Screen.fullScreen); UpdateButtonVisuals(btnResHD, btnResQHD); }
        else if (savedRes == 1440) { Screen.SetResolution(2560, 1440, Screen.fullScreen); UpdateButtonVisuals(btnResQHD, btnResHD); }

        // FPS Butonlarý
        int savedFPS = PlayerPrefs.GetInt("FPSPref", 60);
        Application.targetFrameRate = savedFPS;
        if (savedFPS == 60) UpdateButtonVisuals(btnFPS60, btnFPS120);
        else UpdateButtonVisuals(btnFPS120, btnFPS60);

        // Kalite Butonlarý
        int savedQual = PlayerPrefs.GetInt("QualityPref", 5);
        QualitySettings.SetQualityLevel(savedQual);
        if (savedQual == 0) UpdateButtonVisuals(btnQualLow, btnQualUltra);
        else UpdateButtonVisuals(btnQualUltra, btnQualLow);

        // Kamera Butonlarý
        int savedCam = PlayerPrefs.GetInt("CameraViewPref", 1);
        if (savedCam == 0) UpdateButtonVisuals(btnCam1st, btnCam3rd);
        else UpdateButtonVisuals(btnCam3rd, btnCam1st);

        // Hýz Birimi Butonlarý
        string savedUnit = PlayerPrefs.GetString("SpeedUnitPref", "KMH");
        if (savedUnit == "KMH") UpdateButtonVisuals(btnUnitKMH, btnUnitMPH);
        else UpdateButtonVisuals(btnUnitMPH, btnUnitKMH);
    }
}