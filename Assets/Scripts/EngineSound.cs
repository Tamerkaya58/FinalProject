using UnityEngine;

public class EngineSound : MonoBehaviour
{
    public AudioSource engineAudioSource;
    public Rigidbody carRigidbody;

    [Header("Ses Ayarları")]
    public float minPitch = 0.5f;
    public float maxPitch = 2.5f;
    public float maxSpeed = 100f;

    // EKLENEN KISIM: Motorun çalışıp çalışmadığını kontrol eden anahtar
    public bool isEngineOn = false;

    void Start()
    {
        if (engineAudioSource == null) engineAudioSource = GetComponent<AudioSource>();
        if (carRigidbody == null) carRigidbody = GetComponent<Rigidbody>();

        // Oyun başlarken (Ana menüdeyken) sesin çalmadığından emin ol
        engineAudioSource.Stop();
    }

    void Update()
    {
        // Eğer motor kapalıysa (ana menüdeysek) alt kodları hiç okuma ve hesaplama yapma
        if (!isEngineOn) return;

        // Motor açıksa ama ses durmuşsa (oyun yeni başladıysa) sesi başlat
        if (!engineAudioSource.isPlaying)
        {
            engineAudioSource.Play();
        }

        float currentSpeed = carRigidbody.velocity.magnitude;
        float speedRatio = currentSpeed / maxSpeed;

        float gasInput = Mathf.Abs(Input.GetAxis("Vertical"));

        float targetPitch = minPitch + (speedRatio * 1.5f) + (gasInput * 0.5f);
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, targetPitch, Time.deltaTime * 5f);
    }

    // ANA MENÜDEKİ "PLAY" BUTONUNA BAĞLAYACAĞIMIZ FONKSİYON
    public void StartEngine()
    {
        isEngineOn = true;
    }
}