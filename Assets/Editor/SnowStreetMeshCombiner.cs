#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SnowStreetMeshCombiner : EditorWindow
{
    [MenuItem("Tools/Combine SnowStreet Meshes")]
    public static void ShowWindow()
    {
        GetWindow<SnowStreetMeshCombiner>("Mesh Combiner");
    }

    [MenuItem("Tools/Combine SnowStreet Meshes/Execute Combine")]
    public static void CombineSelectedPrefab()
    {
        string prefabPath = "Assets/SnowStreet 1.prefab";
        string combinedMeshPath = "Assets/SnowStreet_Combined.asset";

        // Check if combined mesh already exists
        if (System.IO.File.Exists(combinedMeshPath))
        {
            if (!EditorUtility.DisplayDialog("Overwrite?",
                "SnowStreet_Combined.asset already exists. Overwrite?",
                "Yes", "Cancel"))
                return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[MeshCombiner] Prefab not found at {prefabPath}");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (instance == null)
        {
            Debug.LogError("[MeshCombiner] Failed to instantiate prefab.");
            return;
        }

        Transform roadTrigger = null;
        try
        {
            // Find RoadTrigger recursively and detach it temporarily
            roadTrigger = FindChildRecursive(instance.transform, "RoadTrigger");
            if (roadTrigger != null)
                roadTrigger.SetParent(null);

            // Collect all MeshFilters recursively (excluding RoadTrigger and root)
            List<MeshFilter> allFilters = new List<MeshFilter>();
            CollectMeshFiltersRecursive(instance.transform, allFilters);

            Debug.Log($"[MeshCombiner] Found {allFilters.Count} MeshFilters to combine.");

            if (allFilters.Count == 0)
            {
                Debug.LogError("[MeshCombiner] No MeshFilters found!");
                return;
            }

            // Group by material
            Dictionary<Material, List<CombineInstance>> groupsByMaterial =
                new Dictionary<Material, List<CombineInstance>>();

            foreach (MeshFilter mf in allFilters)
            {
                if (mf.sharedMesh == null) continue;

                MeshRenderer mr = mf.GetComponent<MeshRenderer>();
                if (mr == null || mr.sharedMaterial == null) continue;

                Material mat = mr.sharedMaterial;
                if (!groupsByMaterial.ContainsKey(mat))
                    groupsByMaterial[mat] = new List<CombineInstance>();

                CombineInstance ci = new CombineInstance();
                ci.mesh = mf.sharedMesh;
                ci.transform = mf.transform.localToWorldMatrix;
                groupsByMaterial[mat].Add(ci);
            }

            Debug.Log($"[MeshCombiner] Material groups: {groupsByMaterial.Count}");

            // Delete all child objects (except root and detached RoadTrigger)
            var toDelete = new List<GameObject>();
            foreach (Transform child in instance.transform)
                toDelete.Add(child.gameObject);
            foreach (GameObject go in toDelete)
                DestroyImmediate(go);

            int groupIndex = 0;
            foreach (var kvp in groupsByMaterial)
            {
                Material mat = kvp.Key;
                List<CombineInstance> combines = kvp.Value;

                Mesh combinedMesh = new Mesh();
                combinedMesh.name = $"SnowStreet_Combined_{groupIndex}";
                combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

                // Batch combine in groups of 10000 to avoid memory spikes
                List<Mesh> subMeshes = new List<Mesh>();
                const int batchSize = 10000;
                for (int i = 0; i < combines.Count; i += batchSize)
                {
                    int count = Mathf.Min(batchSize, combines.Count - i);
                    CombineInstance[] batch = combines.GetRange(i, count).ToArray();

                    Mesh batchMesh = new Mesh();
                    batchMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    batchMesh.CombineMeshes(batch, true, true);
                    subMeshes.Add(batchMesh);

                    float pct = (float)(i + count) / combines.Count * 100f;
                    EditorUtility.DisplayProgressBar("Combining Meshes",
                        $"Batch {subMeshes.Count}... ({pct:F0}%)", pct / 100f);
                }

                // Combine all batches into final mesh
                CombineInstance[] finalCombines = new CombineInstance[subMeshes.Count];
                for (int i = 0; i < subMeshes.Count; i++)
                {
                    finalCombines[i] = new CombineInstance
                    {
                        mesh = subMeshes[i],
                        transform = Matrix4x4.identity
                    };
                }

                combinedMesh.CombineMeshes(finalCombines, false, true);

                Debug.Log($"[MeshCombiner] Group {groupIndex}: {combines.Count} meshes -> {combinedMesh.vertexCount} vertices, {combinedMesh.triangles.Length / 3} triangles.");

                // Save combined mesh asset
                AssetDatabase.CreateAsset(combinedMesh, combinedMeshPath);

                // Create replacement GameObject
                GameObject combinedGO = new GameObject("CombinedMesh");
                combinedGO.transform.SetParent(instance.transform);
                combinedGO.transform.localPosition = Vector3.zero;
                combinedGO.transform.localRotation = Quaternion.identity;
                combinedGO.transform.localScale = Vector3.one;

                MeshFilter cmf = combinedGO.AddComponent<MeshFilter>();
                cmf.sharedMesh = combinedMesh;

                MeshRenderer cmr = combinedGO.AddComponent<MeshRenderer>();
                cmr.sharedMaterial = mat;
                cmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                cmr.receiveShadows = true;

                // Clean up temp meshes
                foreach (Mesh m in subMeshes)
                    DestroyImmediate(m);

                groupIndex++;
            }

            EditorUtility.ClearProgressBar();

            // Re-attach RoadTrigger
            if (roadTrigger != null)
                roadTrigger.SetParent(instance.transform);

            // Save prefab (must be done BEFORE destroying instance)
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Debug.Log($"[MeshCombiner] Prefab saved: {prefabPath}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Done",
                $"Combined {allFilters.Count} meshes into a single mesh.\n\n" +
                $"Combined mesh: {combinedMeshPath}\n" +
                $"Prefab updated: {prefabPath}\n\n" +
                "Don't forget to test in Play Mode!",
                "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"[MeshCombiner] Error: {e.Message}\n{e.StackTrace}");
        }
        finally
        {
            // Clean up the instance (after save is complete)
            if (instance != null)
                DestroyImmediate(instance);
        }

        Debug.Log("[MeshCombiner] Done!");
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private static void CollectMeshFiltersRecursive(Transform t, List<MeshFilter> list)
    {
        foreach (Transform child in t)
        {
            if (child.name == "RoadTrigger") continue;

            MeshFilter mf = child.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
                list.Add(mf);

            CollectMeshFiltersRecursive(child, list);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("SnowStreet Mesh Combiner", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This will combine all ~37,384 individual meshes in SnowStreet 1.prefab " +
            "into a single combined mesh. This reduces draw calls from 37K to 1.\n\n" +
            "WARNING: Make a backup of SnowStreet 1.prefab before proceeding!",
            MessageType.Warning);

        GUILayout.Space(10);

        if (GUILayout.Button("Combine SnowStreet 1.prefab Meshes", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("Confirm",
                "This will combine all 37,384 meshes in SnowStreet 1.prefab into a single mesh.\n\n" +
                "Make sure you have a backup!\n\n" +
                "Continue?",
                "Yes, combine", "Cancel"))
            {
                CombineSelectedPrefab();
            }
        }
    }
}

#endif

