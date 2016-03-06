using UnityEditor;
using UnityEngine;

public static class SpellbornMenu
{
    const string mainmenu = "Spellborn";

    [MenuItem(mainmenu + "/PackageExtractor")]
    public static void ParsePackage()
    {
        var window = EditorWindow.GetWindow<PackageExtractorWindow>("SBPackage");
        window.minSize = new Vector2(800, 600);
    }

    [MenuItem("Spellborn/SkillInspector")]
    public static void OpenSkillVisWindow()
    {
        EditorWindow.GetWindow<FSkillInspectorWindow>("SkillInspector");
    }
}