using UnityEngine;
using System.Collections;


public abstract class BaseHazard : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected MeshRenderer hazardMesh;
    [SerializeField] protected float fadeDuration = 1.5f;
    [Tooltip("The collider that detects the car from far away.")]
    [SerializeField] protected Collider detectionTrigger; 

    // Internal State
    private MaterialPropertyBlock _propBlock;
    private bool _isActivated = false;
    private int _baseColorID;

    // ---------------------------------------------------------
    // 1. ABSTRACT METHODS (You MUST implement these in child classes)
    // ---------------------------------------------------------
    
    /// <summary>
    /// Run specific setup logic here 
    /// (e.g., reset variables, randomize rotation).
    /// Called automatically during after Awake of BaseHazard's child classes.
    /// </summary>
    protected abstract void OnInitialize();

    /// <summary>
    /// What happens when the car actually crashes into the hazard class that 
    /// inherits from BaseHazard class?
    /// (e.g., Game Over, Subtract Health, Play Explosion Sound)
    /// </summary>
    protected abstract void OnHazardCollision(GameObject player);

    // ---------------------------------------------------------
    // 2. CORE LOGIC
    // ---------------------------------------------------------

    private void Awake()
    {
        /*
        To skip the initial step after the feature is implemented
        we need to merge meshes for hazards for this implementation
        */
        if (hazardMesh != null)
        {
            // Cache shader property ID for performance
            _baseColorID = Shader.PropertyToID("_Color"); // Use "_Color" for Built-in, "_BaseColor" for URP
            _propBlock = new MaterialPropertyBlock();

            // 1. Hide the mesh immediately
            SetHazardAlpha(0f);
        }
        Debug.Log($"{this.GetType().Name} is running BaseHazard.Awake()");
        
        // 2. Call the child class specific initialization
        OnInitialize();
    }

    /// <summary>
    /// Checks for the "Long Range" detection trigger.
    /// </summary>
    public virtual void OnTriggerEnter(Collider other)
    {
        // Optimization: Use Tags or Layers to filter specifically for the Player
        if (!_isActivated && other.CompareTag("Player"))
        {
            // Verify if the trigger hit was actually our detection zone
            // (Useful if you have multiple triggers on the same object)
            if (detectionTrigger.bounds.Intersects(other.bounds)) 
            {
                StartCoroutine(FadeInRoutine());
                _isActivated = true;
            }
        }
    }

    /// <summary>
    /// Checks for the actual "Physical" collision.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnHazardCollision(collision.gameObject);
        }
    }

    // ---------------------------------------------------------
    // 3. VISUAL LOGIC (Dynamic Mesh Handling)
    // ---------------------------------------------------------

    private IEnumerator FadeInRoutine()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            SetHazardAlpha(alpha);
            yield return null;
        }

        SetHazardAlpha(1f);
    }

    private void SetHazardAlpha(float alpha)
    {
        if (hazardMesh == null) return;

        // Get the current color, update alpha, apply via PropertyBlock
        // This is much more performant than accessing .material directly
        hazardMesh.GetPropertyBlock(_propBlock);
        
        // Note: For this to work, your Material Surface Type must be "Transparent" 
        // in the Inspector, not "Opaque".
        Color currentColor = hazardMesh.sharedMaterial.GetColor(_baseColorID); // Get base color from shared material
        currentColor.a = alpha;
        
        _propBlock.SetColor(_baseColorID, currentColor);
        hazardMesh.SetPropertyBlock(_propBlock);
    }
}