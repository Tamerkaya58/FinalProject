using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour
{
    [Header("Vfx Settings")]
    [SerializeField] private GameObject PlayerVfxPrefab;

    [SerializeField] private bool DestroyOnTrigger = true;

    [Header("Score Penalty")]
    [SerializeField] private float pointPenalty = 50f;

    [Header("Speed Penalty")]
    [SerializeField] private float speedMultiplier = 0.85f;

    [SerializeField] private float minKeepSpeedKmh = 45f;

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
        if (!OtherCollider.CompareTag("Player")) return;

        triggered = true;

        SpawnVfx();

        Rigidbody carRB = OtherCollider.GetComponentInParent<Rigidbody>();

        if (carRB != null)
        {
            StartCoroutine(ApplySpeedPenaltyAfterPhysics(carRB));
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.AddPoints(-pointPenalty);
        }

        Debug.Log("Trigger Entered: Bomb hit. Speed reduced and points deducted.");

        if (DestroyOnTrigger)
        {
            StartCoroutine(DestroyAfterPhysics());
        }
    }

    private IEnumerator ApplySpeedPenaltyAfterPhysics(Rigidbody carRB)
    {
        Vector3 oldVelocity = carRB.velocity;

        Collider[] bombColliders = GetComponentsInChildren<Collider>();

        foreach (Collider col in bombColliders)
        {
            col.enabled = false;
        }

        yield return new WaitForFixedUpdate();

        if (carRB != null)
        {
            Vector3 newVelocity = oldVelocity * speedMultiplier;

            float minKeepSpeed = minKeepSpeedKmh / 3.6f;

            if (oldVelocity.magnitude > minKeepSpeed && newVelocity.magnitude < minKeepSpeed)
            {
                newVelocity = oldVelocity.normalized * minKeepSpeed;
            }

            carRB.velocity = newVelocity;
        }
    }

    private IEnumerator DestroyAfterPhysics()
    {
        yield return new WaitForFixedUpdate();

        Destroy(gameObject);
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