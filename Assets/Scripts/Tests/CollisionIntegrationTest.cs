using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement; // Needed to load scene if necessary
using UnityEngine.TestTools;

public class CollisionIntegrationTest
{

    // REPLACE THIS with the exact name of your scene file
    private const string TEST_SCENE_NAME = "Test"; 

    [UnitySetUp]
    public IEnumerator Setup()
    {
        // 1. Load the scene containing the TestContext GameObject
        yield return SceneManager.LoadSceneAsync(TEST_SCENE_NAME);

        // 2. Optional: Wait a frame to ensure everything initializes
        yield return null;
    }

    [UnityTest]
    public IEnumerator RunAllCollisionTests()
    {
        // AŞAMA 1: Find the Reference Holder in the Scene
        // We look for the object we created in Step 2
        TestSceneReferences references = Object.FindObjectOfType<TestSceneReferences>();

        // Fail safely if we forgot to set up the scene
        Assert.IsNotNull(references, "CRITICAL ERROR: Could not find 'TestSceneReferences' script in the scene. Please create an object and attach the script with prefabs assigned.");
        Assert.IsNotNull(references.masterCollisionPrefab, "ERROR: Master Prefab is missing in the Reference Holder.");
        Assert.IsNotNull(references.carPrefab, "ERROR: Car Prefab is missing in the Reference Holder.");

        // Use the prefabs directly from the reference script
        GameObject masterInstance = Object.Instantiate(references.masterCollisionPrefab);
        masterInstance.transform.position = Vector3.zero;

        Debug.Log($"<color=cyan>--- TEST STARTING via References ---</color>");

        // AŞAMA 2: Loop through Test Cases (Logic remains the same)
        foreach (Transform testCase in masterInstance.transform)
        {
            testCase.gameObject.SetActive(true);
            
            Transform targetTr = FindFirstTarget(testCase);

            if (targetTr != null)
            {
                // Instantiate Car from the Reference
                GameObject carInstance = Object.Instantiate(references.carPrefab);
                carInstance.name = "Car";

                Vector3 spawnPos = targetTr.position - new Vector3(0, 0, 15f);
                carInstance.transform.position = spawnPos;
                carInstance.transform.LookAt(targetTr);

                var spy = targetTr.gameObject.AddComponent<PhysicsSpy>();

                Rigidbody rb = carInstance.GetComponent<Rigidbody>();
                if (rb == null) rb = carInstance.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.velocity = carInstance.transform.forward * 25f;

                yield return new WaitForSeconds(0.6f);

                AnalyzeAndPrint(targetTr.gameObject, spy);

                Object.Destroy(carInstance);
            }
            
            testCase.gameObject.SetActive(false);
        }

        Object.Destroy(masterInstance);
    }

    // --- LOGIC FUNCTIONS (Same as before) ---
    void AnalyzeAndPrint(GameObject target, PhysicsSpy spy)
    {
    MonoBehaviour[] scripts = target.GetComponents<MonoBehaviour>();
    
    // "Spy haricinde işe yarar bir script bulduk mu?" kontrolü için bayrak
    bool foundUserScript = false; 

    foreach (var script in scripts)
    {
        // PhysicsSpy'ı görmezden gel
        if (script is PhysicsSpy) continue;

        // Buraya geldiysek, Spy olmayan bir script bulduk demektir
        foundUserScript = true;

        var type = script.GetType();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        MethodInfo onTrigger = type.GetMethod("OnTriggerEnter", flags);
        MethodInfo onCollision = type.GetMethod("OnCollisionEnter", flags);

        // 1. TRIGGER KONTROLÜ
        if (spy.triggered)
        {
            if (onTrigger != null)
                Debug.Log($"[RESULT] <color=yellow>OnTriggerEnter</color> fired on: <b>{type.Name}</b>");
            else
                Debug.Log($"[INFO] <color=yellow>OnTriggerEnter</color> happened but <color=red>NO CALLBACK</color> assigned for: <b>{type.Name}</b>");
        }

        // 2. COLLISION KONTROLÜ
        if (spy.collided)
        {
            if (onCollision != null)
                Debug.Log($"[RESULT] <color=orange>OnCollisionEnter</color> fired on: <b>{type.Name}</b>");
            else
                Debug.Log($"[INFO] <color=orange>OnCollisionEnter</color> happened but <color=red>NO CALLBACK</color> assigned for: <b>{type.Name}</b>");
        }
    }

    // Döngü bitti, eğer Spy haricinde hiçbir script bulamadıysak uyarıyı şimdi ver
    if (!foundUserScript)
    {
        Debug.LogWarning($"[WARNING] No user scripts found (excluding PhysicsSpy) on target: <b>{target.name}</b>");
    }
    }

    Transform FindFirstTarget(Transform parent)
    {
        if (parent.GetComponent<Collider>() != null) return parent;
        foreach (Transform child in parent)
        {
            if (child.GetComponent<Collider>() != null) return child;
            var result = FindFirstTarget(child);
            if (result != null) return result;
        }
        return null;
    }
}

// --- SPY CLASS ---
public class PhysicsSpy : MonoBehaviour
{
    public bool triggered = false;
    public bool collided = false;
    private void OnTriggerEnter(Collider other) { if (other.name.Contains("Car")) triggered = true; }
    private void OnCollisionEnter(Collision collision) { if (collision.gameObject.name.Contains("Car")) collided = true; }
}