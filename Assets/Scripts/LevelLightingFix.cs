using UnityEngine;
using UnityEngine.Rendering;

public class LevelLightingFix : MonoBehaviour
{
    public Light directionalLight;

    private void Start()
    {
        // Ortam ýþýðýný biraz azalt
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.8f, 0.8f, 0.8f);

        // Güneþ ýþýðý
        if (directionalLight != null)
        {
            directionalLight.intensity = 0.85f;
            directionalLight.color = Color.white;
            directionalLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
    }
}