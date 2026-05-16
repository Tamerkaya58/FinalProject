using UnityEngine;
using UnityEditor;

public class SpawnLoggingTool
{
    private const string SpawnLoggingKey = "DevTool_SpawnLogging";

    [MenuItem("Tools/Spawn Logging/Enabled")]
    private static void ToggleSpawnLogging()
    {
        bool isEnabled = EditorPrefs.GetBool(SpawnLoggingKey, false);
        EditorPrefs.SetBool(SpawnLoggingKey, !isEnabled);
    }

    [MenuItem("Tools/Spawn Logging/Enabled", true)]
    private static bool ToggleSpawnLoggingValidate()
    {
        Menu.SetChecked("Tools/Spawn Logging/Enabled", EditorPrefs.GetBool(SpawnLoggingKey, false));
        return true;
    }

    public static bool IsEnabled()
    {
        return EditorPrefs.GetBool(SpawnLoggingKey, false);
    }
}
