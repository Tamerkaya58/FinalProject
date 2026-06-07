using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Vfx Settings")]
    [Tooltip("The Vfx Prefab To Play At The Bomb Position.")]
    [SerializeField] private GameObject PlayerVfxPrefab;

    [Tooltip("Determines If The Bomb Destroys Itself Upon Trigger.")]
    [SerializeField] private bool DestroyOnTrigger = true;

    [Header("Score Penalty")]
    [SerializeField] private float pointPenalty = 50f;

    [Header("Speed Penalty")]
    [SerializeField] private float speedMultiplier = 0.7f;

    private bool triggered = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            SpawnVfx();
            Debug.Log("K Key Pressed: Playing Vfx At Bomb Location.");
        }
    }

    private void OnTriggerEnter(Collider OtherCollider)
    {
        if (triggered) return;

        if (OtherCollider.CompareTag("Player"))
        {
            triggered = true;

            SpawnVfx();

            // Aracý yavaþlat
            Rigidbody carRB = OtherCollider.GetComponent<Rigidbody>();

            if (carRB != null)
            {
                carRB.velocity *= speedMultiplier;
            }

            // Puan düþ
            if (GameManager.instance != null)
            {
                GameManager.instance.AddPoints(-pointPenalty);
            }

            Debug.Log("Trigger Entered: Bomb hit. Speed reduced and points deducted.");

            if (DestroyOnTrigger)
            {
                Destroy(gameObject);
            }
        }
    }

    private void SpawnVfx()
    {
        if (PlayerVfxPrefab != null)
        {
            GameObject SpawnedVfx = Instantiate(PlayerVfxPrefab, transform.position, transform.rotation);
            Destroy(SpawnedVfx, 2f);
        }
        else
        {
            Debug.LogWarning("Bomb: Vfx Prefab Is Missing!");
        }
    }
}