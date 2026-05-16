using UnityEngine;
using UnityEditor;

/// <summary>
/// This script calculates and prints the boundaries (Bounds) of the currently selected GameObject in the Unity Editor.
/// It triggers automatically whenever the selection changes.
/// </summary>
[InitializeOnLoad]
public class SelectionBoundsPrinter
{
    private const string EnabledKey = "SelectionBoundsPrinter_Enabled";

    static SelectionBoundsPrinter()
    {
        // Subscribe to the selection change event
        Selection.selectionChanged += OnSelectionChanged;
    }

    [MenuItem("Tools/Selection Bounds/Enabled")]
    private static void ToggleEnabled()
    {
        bool isEnabled = EditorPrefs.GetBool(EnabledKey, true);
        EditorPrefs.SetBool(EnabledKey, !isEnabled);
    }

    [MenuItem("Tools/Selection Bounds/Enabled", true)]
    private static bool ToggleEnabledValidate()
    {
        Menu.SetChecked("Tools/Selection Bounds/Enabled", EditorPrefs.GetBool(EnabledKey, true));
        return true;
    }

    private static void OnSelectionChanged()
    {
        // Check if the feature is enabled
        if (!EditorPrefs.GetBool(EnabledKey, true))
        {
            return;
        }

        // Get the active GameObject
        GameObject activeObject = Selection.activeGameObject;

        // Only proceed if an object is selected
        if (activeObject == null)
        {
            return;
        }

        Bounds bounds = BoundsUtility.GetTotalBounds(activeObject);

        // Explicitly extract dimensions for clarity
        float width = bounds.size.x;
        float height = bounds.size.y;
        float depth = bounds.size.z;

        Vector3 pos = activeObject.transform.position;

        // Format and print the results to the console on a single line
        string report = $"<b>[Bounds]</b> {activeObject.name} -> " +
                        $"<color=#9b59b6>Pos:</color> ({pos.x:F2}, {pos.y:F2}, {pos.z:F2}) | " +
                        $"<color=#3498db>Width(X):</color> {width:F2} | " +
                        $"<color=#2ecc71>Height(Y):</color> {height:F2} | " +
                        $"<color=#e74c3c>Depth(Z):</color> {depth:F2}";

        Debug.Log(report);
    }
}
