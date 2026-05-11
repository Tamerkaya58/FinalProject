using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject car;
    public float smoothSpeed = 0.3f; // Yumuşatma hızı

    [Header("Third Person Ayarları")]
    public Vector3 tpsOffset;
    public Vector3 tpsRotation;

    [Header("First Person Ayarları")]
    public Vector3 fpsOffset;
    public Vector3 fpsRotation;

    private int cameraMode;

    void Start()
    {
        if (car == null) car = GameObject.Find("Car");

        UpdateCameraView();

        // EKLENEN KISIM: Oyun başlar başlamaz kamerayı anında hedefe oturt!
        SnapToTarget();
    }

    public void UpdateCameraView()
    {
        cameraMode = PlayerPrefs.GetInt("CameraViewPref", 1);
    }

    // Kamerayı Lerp (yumuşatma) olmadan anında yerine ışınlayan fonksiyon
    private void SnapToTarget()
    {
        if (car == null) return;

        if (cameraMode == 1)
        {
            transform.position = car.transform.TransformPoint(tpsOffset);
            transform.rotation = car.transform.rotation * Quaternion.Euler(tpsRotation);
        }
        else if (cameraMode == 0)
        {
            transform.position = car.transform.TransformPoint(fpsOffset);
            transform.rotation = car.transform.rotation * Quaternion.Euler(fpsRotation);
        }
    }

    void LateUpdate()
    {
        if (car == null) return;

        if (cameraMode == 1)
        {
            // --- THIRD PERSON (Dışarıdan) ---
            Vector3 desiredPos = car.transform.TransformPoint(tpsOffset);
            transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);

            Quaternion desiredRot = car.transform.rotation * Quaternion.Euler(tpsRotation);
            transform.rotation = Quaternion.Lerp(transform.rotation, desiredRot, smoothSpeed);
        }
        else if (cameraMode == 0)
        {
            // --- FIRST PERSON (İçeriden) ---
            transform.position = car.transform.TransformPoint(fpsOffset);
            transform.rotation = car.transform.rotation * Quaternion.Euler(fpsRotation);
        }
    }
}