using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Vfx Settings")]
    [Tooltip("The Vfx Prefab To Play At The Bomb Position.")]
    [SerializeField] private GameObject PlayerVfxPrefab;

    [Tooltip("Determines If The Bomb Destroys Itself Upon Trigger.")]
    [SerializeField] private bool DestroyOnTrigger = true;

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
        if (OtherCollider.CompareTag("Player"))
        {
            SpawnVfx();
            Debug.Log("Trigger Entered: Playing Vfx At Bomb Location.");

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
            // Instantiating the VFX and keeping its reference
            GameObject SpawnedVfx = Instantiate(PlayerVfxPrefab, transform.position, transform.rotation);
            
            // Forcing destruction after 2 seconds to prevent memory leaks
            // Adjust the 2f value based on your particle's actual duration
            Destroy(SpawnedVfx, 2f); 
        }
        else
        {
            Debug.LogWarning("Bomb: Vfx Prefab Is Missing!");
        }
    }
}