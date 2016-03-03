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

    //private static void FixHide()

    //[MenuItem(mainmenu + "/FixHide")]
    //{
    //    List<ItemCollection> collectionAssets = new List<ItemCollection>();
    //    string[] files = System.IO.Directory.GetFiles(Application.dataPath + "/GameData/Items/");
    //    foreach (string f in files)
    //    {
    //        ItemCollection t = AssetDatabase.LoadAssetAtPath<ItemCollection>("Assets" + f.Replace(Application.dataPath, string.Empty));
    //        if (t != null)
    //        {
    //            collectionAssets.Add(t);
    //        }
    //    }
    //    List<ScriptableObject> allItems = new List<ScriptableObject>();
    //    foreach (ItemCollection itc in collectionAssets)
    //    {
    //        object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(itc));
    //        foreach (object o in objs)
    //        {
    //            if (o is ScriptableObject)
    //            {
    //                allItems.Add(o as ScriptableObject);
    //            }
    //        }
    //    }
    //    foreach (ScriptableObject it in allItems)
    //    {
    //        if (it is Content_Event || it is Content_Requirement || it is Item_Component)
    //        {
    //            it.hideFlags = HideFlags.HideInHierarchy;
    //        }
    //        EditorUtility.SetDirty(it);
    //    }
    //}
}