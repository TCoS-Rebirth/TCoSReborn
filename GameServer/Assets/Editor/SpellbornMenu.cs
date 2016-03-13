using System;
using System.Collections.Generic;
using System.IO;
using PackageExtractor.Adapter;
using UnityEditor;
using UnityEngine;
using World;

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

#region Temporary AllZoneNPCExtraction
    [MenuItem(mainmenu + "/ExtractZoneNPCs")]
    public static void ExtractAllZoneNPCs()
    {
        zones.Clear();
        folderName = EditorUtility.OpenFolderPanel("map folder", "", "");
        if (folderName == "")
        {
            return;
        }
        zones.AddRange(GameObject.FindObjectsOfType<Zone>());
        if (zones.Count == 0) return;
        _currentZoneIndex = 0;
        windowRef = EditorWindow.GetWindow<PackageExtractorWindow>("SBPackage");
        EditorApplication.delayCall += Process;
    }

    static void OnLoaded()
    {
        windowRef.CreateAdapter(typeof (NPCSpawnerExtractor));
        var nps = windowRef.ActiveAdapter as NPCSpawnerExtractor;
        if (nps != null)
        {
            nps.targetZone = zones[_currentZoneIndex];
            windowRef.PassToAdapter(null);
        }
        windowRef.RemoveAdapter();
        windowRef.CreateAdapter(typeof (SpawnDeployerExtractor));
        var dps = windowRef.ActiveAdapter as SpawnDeployerExtractor;
        if (dps != null)
        {
            dps.targetZone = zones[_currentZoneIndex];
            windowRef.PassToAdapter(null);
        }
        _currentZoneIndex += 1;
        EditorApplication.delayCall += Process;
    }

    static void Process()
    {
        if (_currentZoneIndex >= zones.Count)
        {
            Debug.Log("end");
            folderName = "";
            if (windowRef != null)
            {
                windowRef.Close();  
            }
            zones.Clear();
            _currentZoneIndex = 0;
            return;
        }
        var z = zones[_currentZoneIndex];
        if (!string.IsNullOrEmpty(z.PackageFileName))
        {
            var fName = Path.Combine(folderName, z.PackageFileName).Replace(@"\", "/");
            windowRef.LoadNewPackage(fName, OnLoaded);
        }
    }

    static string folderName = "";
    static PackageExtractorWindow windowRef;
    static readonly List<Zone> zones = new List<Zone>();
    static int _currentZoneIndex = 0;

#endregion
}