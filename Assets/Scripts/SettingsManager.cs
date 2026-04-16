using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [Header("Ana Paneller")]
    public GameObject settingsMainPanel; // Tüm ayarlarýn arka planý ve çerçevesi

    [Header("Alt Kategoriler")]
    public GameObject generalSubPanel;
    public GameObject audioSubPanel;
    public GameObject displaySubPanel;

    // --- ANA MENÜDEN GELEN TETÝKLEME ---
    // Main Menu'deki Settings butonuna bu fonksiyonu baðla
    public void OpenSettingsFromMainMenu()
    {
        settingsMainPanel.SetActive(true); // Önce ana paneli açar
        ShowGeneralPanel();                // Otomatik olarak genel ayarlarý getirir
    }

    // --- ALT PANEL GEÇÝÞLERÝ ---

    public void ShowGeneralPanel()
    {
        DeactivateAllSubPanels();
        generalSubPanel.SetActive(true); // Sadece Genel panelini açar
    }

    public void ShowAudioPanel()
    {
        DeactivateAllSubPanels();
        audioSubPanel.SetActive(true);   // Genel'i ve diðerlerini kapatýr, Ses'i açar
    }

    public void ShowDisplayPanel()
    {
        DeactivateAllSubPanels();
        displaySubPanel.SetActive(true); // Genel'i ve diðerlerini kapatýr, Görüntü'yü açar
    }

    // Diðer tüm alt panelleri kapatan yardýmcý metod (Ana panel hariç)
    private void DeactivateAllSubPanels()
    {
        if (generalSubPanel != null) generalSubPanel.SetActive(false);
        if (audioSubPanel != null) audioSubPanel.SetActive(false);
        if (displaySubPanel != null) displaySubPanel.SetActive(false);
    }

    // --- KAPATMA VE ANA MENÜ ---
    public void CloseSettings()
    {
        settingsMainPanel.SetActive(false);
    }
}