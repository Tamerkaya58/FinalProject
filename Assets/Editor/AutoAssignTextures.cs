#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class AutoAssignTextures : EditorWindow
{
    [MenuItem("Tools/Fix Car Materials")]
    static void FixMaterials()
    {
        string matPath = "Assets/Cars/Materials";
        string texPath = "Assets/Cars/textures";
        int count = 0;

        string[] mats = Directory.GetFiles(matPath, "*.mat", SearchOption.AllDirectories);
        string[] texFiles = Directory.GetFiles(texPath, "*.png", SearchOption.AllDirectories);

        foreach (string matFile in mats)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matFile);
            if (mat == null) continue;

            // Shader'i Standard'a cevir
            mat.shader = Shader.Find("Standard");

            string matName = Path.GetFileNameWithoutExtension(matFile).ToLower();

            foreach (string texFile in texFiles)
            {
                string texName = Path.GetFileNameWithoutExtension(texFile).ToLower();
                if (texName.Contains(matName) || matName.Contains(texName))
                {
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texFile);
                    if (tex != null)
                    {
                        mat.SetTexture("_MainTex", tex);
                        EditorUtility.SetDirty(mat);
                        count++;
                        break;
                    }
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Fixed {count} materials!");
    }
}
#endif
