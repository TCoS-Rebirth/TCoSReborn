using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World;

public class SandboxUI : MonoBehaviour
{
    bool started = false;
    List<string> dbgMessages = new List<string>();
    Vector2 scrollPos;

    void OnGUI()
    {
        if (!started)
        {
            if (GUILayout.Button("Start"))
            {
                GameWorld.Instance.Startup();
                started = true;
            }
        }
        else
        {
            if (GUILayout.Button("Stop"))
            {
                Application.Quit();
            }
        }
        GUILayout.BeginVertical(GUI.skin.box);
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < dbgMessages.Count; i++)
        {
            GUILayout.Label(dbgMessages[i], GUI.skin.box);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    void Start()
    {
        if (Environment.GetCommandLineArgs().Contains("-batchmode"))
        {
            GameWorld.Instance.Startup();
            Application.targetFrameRate = 30;
            enabled = false;
            return;
        }
        if (!Application.isEditor)
        {
            Screen.SetResolution(400, 200, false, 15);
            Application.targetFrameRate = 30;
        }
        Application.logMessageReceived += OnLogMessage;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessage;
    }

    private void OnLogMessage(string condition, string stackTrace, LogType type)
    {
        dbgMessages.Add(condition);
        if (dbgMessages.Count > 10)
        {
            dbgMessages.RemoveAt(0);
        }
    }
}
