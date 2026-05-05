using UnityEngine;
using UnityEditor;

public class SkipIntroTool
{
    private const string SkipIntroKey = "DevTool_SkipIntro";

    [MenuItem("Tools/Skip Intro")]
    private static void ToggleSkipIntro()
    {
        bool isSkipping = EditorPrefs.GetBool(SkipIntroKey, false);
        EditorPrefs.SetBool(SkipIntroKey, !isSkipping);
    }

    [MenuItem("Tools/Skip Intro", true)]
    private static bool ToggleSkipIntroValidate()
    {
        Menu.SetChecked("Tools/Skip Intro", EditorPrefs.GetBool(SkipIntroKey, false));
        return true;
    }
}
