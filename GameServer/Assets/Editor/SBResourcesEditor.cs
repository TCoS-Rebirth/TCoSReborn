using System.Collections.Generic;
using Database.Static;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (SBResources))]
public class SBResourcesEditor : Editor
{
    List<SBResource> cached = new List<SBResource>();
    string filter = "";
    Vector2 scrollPos;

    void OnEnable()
    {
        filter = "";
        cached = new List<SBResource>((target as SBResources).Res);
    }

    public override void OnInspectorGUI()
    {
        var sr = target as SBResources;
        float inspectorHeight = Screen.height - 74;
        float inspectorWidth = Screen.width - 20;
        GUI.Label(new Rect(5, 46, 40, 16), "Filter: ");
        EditorGUI.BeginChangeCheck();
        filter = GUI.TextField(new Rect(45, 46, inspectorWidth - 100, 16), filter);
        if (EditorGUI.EndChangeCheck())
        {
            if (filter.Length > 0 && !string.IsNullOrEmpty(filter))
            {
                cached.Clear();
                foreach (var s in sr.GetFilteredResources(filter))
                {
                    cached.Add(s);
                }
            }
            else
            {
                cached.Clear();
                cached = new List<SBResource>(sr.Res);
            }
        }
        var elementHeight = 16f;
        scrollPos = GUI.BeginScrollView(new Rect(0, 60, inspectorWidth + 20, inspectorHeight - 20), scrollPos, new Rect(0, 0, inspectorWidth, elementHeight*cached.Count));
        for (var i = 0; i < cached.Count; i++)
        {
            var elementY = i*elementHeight;
            if (elementY < scrollPos.y)
            {
                continue;
            }
            ShowSingleEntry(new Rect(5, elementY, inspectorWidth, elementHeight), cached[i]);
            if (elementY > scrollPos.y + inspectorHeight)
            {
                break;
            }
        }
        GUI.EndScrollView();
    }

    void ShowSingleEntry(Rect r, SBResource res)
    {
        GUI.Label(r, string.Format("{0,-7} {1}", res.ID, res.Name), EditorStyles.label);
    }
}