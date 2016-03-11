using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Database.Static;
using PackageExtractor;
using PackageExtractor.Adapter;
using UnityEditor;
using UnityEngine;

public struct LogMessage
{
    public string text;
    public Color color;

    public LogMessage(string text, Color color)
    {
        this.text = text;
        this.color = color;
    }
}

public class PackageExtractorWindow : EditorWindow
{
    ExtractorAdapter activeAdapter;
    PackageWrapper activeWrapper;
    Rect adapterRect;
    PackageReadConfig arrayDefinitions;

    Dictionary<string, Type> availableAdapters = new Dictionary<string, Type>();
    Rect contentRect;
    bool isBuildingTree;

    string lastPath = "";

    Rect loadButtonRect;
    SBLocalizedStrings localizedStrings;

    List<LogMessage> logMessages = new List<LogMessage>();
    Rect logRect;

    Vector2 logScrollPos;
    Rect logScrollRect;
    Rect logViewRect;
    Rect menuRect;
    Rect openedInfoRect;
    Rect openedPackageRect;

    List<PackageWrapper> openedPackages = new List<PackageWrapper>();
    Vector2 openedPackageScrollPos;
    Rect passToAdapterRect;
    float prevScreenSize;

    GUIStyle propertiesTextStyle;
    Rect propertyViewRect;
    Rect pushRect;
    Rect reopenRect;
    SBResources resources;

    BackgroundWorker treeBuilder;

    public PackageWrapper ActiveWrapper
    {
        get { return activeWrapper; }
    }

    public List<PackageWrapper> StashedPackages
    {
        get { return openedPackages; }
    }

    public ExtractorAdapter ActiveAdapter
    {
        get { return activeAdapter; }
    }
    void CollectAvailableAdapters()
    {
        availableAdapters.Clear();
        var adapterTypes = typeof (ExtractorAdapter).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof (ExtractorAdapter))).ToArray();
        availableAdapters.Add("Adapter", null);
        for (var i = 0; i < adapterTypes.Length; i++)
        {
            availableAdapters.Add(adapterTypes[i].Name, adapterTypes[i]);
        }
    }

    public void PassToAdapter(WrappedPackageObject wrappedObject)
    {
        if (activeAdapter != null)
        {
            if (wrappedObject != null)
            {
                LogString("Passing selection to adapter: " + wrappedObject.sbObject.Name, Color.white);
            }
            else
            {
                LogString("Passing package to adapter", Color.white);
            }
            activeAdapter.HandlePackageContent(wrappedObject, resources, localizedStrings);
        }
        else
        {
            LogString("No adapter assigned!", Color.yellow);
        }
    }

    public WrappedPackageObject FindObjectWrapper(string name, bool includesParentName)
    {
        if (activeWrapper == null)
        {
            return null;
        }
        return activeWrapper.FindObjectWrapper(name, includesParentName);
    }

    void OnEnable()
    {
        CollectAvailableAdapters();
        logMessages.Clear();
        resources = AssetDatabase.LoadAssetAtPath<SBResources>("Assets/GameData/SBResources/SBResources.asset");
        localizedStrings = AssetDatabase.LoadAssetAtPath<SBLocalizedStrings>("Assets/GameData/SBResources/SBLocalizedStrings.asset");
        arrayDefinitions = AssetDatabase.LoadAssetAtPath<PackageReadConfig>("Assets/Editor/PackageExtractor/Properties/ArrayDefinitions.asset");
    }

    void OnDestroy()
    {
        if (treeBuilder != null)
        {
            treeBuilder.CancelAsync();
        }
    }

    void LogString(string msg, Color c)
    {
        logMessages.Add(new LogMessage(msg, c));
        if (logMessages.Count > 100)
        {
            logMessages.RemoveAt(0);
        }
    }

    void OnInspectorUpdate()
    {
        var curScreenSize = position.width + position.height;
        if (prevScreenSize != curScreenSize)
        {
            OnResized();
            prevScreenSize = curScreenSize;
        }
        Repaint();
    }

    void OnResized()
    {
        var spacing = 5f;
        menuRect = new Rect(spacing, spacing, 200 - spacing, position.height - 400 - spacing*2f);
        logRect = new Rect(spacing, position.height - 200 + spacing, position.width - spacing*2f, 200 - spacing*2f);
        contentRect = new Rect(200 + spacing, spacing, position.width - 600 - spacing*2f, position.height - 200 - spacing);
        loadButtonRect = new Rect(0, 0, 200 - spacing, 20);
        reopenRect = new Rect(0, 20, loadButtonRect.width*0.5f, loadButtonRect.height);
        pushRect = new Rect(loadButtonRect.width*0.5f, 20, loadButtonRect.width*0.5f, loadButtonRect.height);
        openedInfoRect = new Rect(0, 40, 200 - spacing, 20);
        logViewRect = new Rect(0, 0, logRect.width, logRect.height);
        logScrollRect = new Rect(logViewRect);
        logViewRect.width -= 15f;
        propertyViewRect = new Rect(contentRect.width + menuRect.width + spacing*3f, contentRect.y, 400 - spacing, contentRect.height - 50);
        adapterRect = new Rect(menuRect.x, menuRect.y + openedInfoRect.height + 60, menuRect.width - spacing*2f, menuRect.height - 100);
        openedPackageRect = new Rect(menuRect.x, menuRect.y + menuRect.height + spacing, menuRect.width, 200);
        passToAdapterRect = new Rect(propertyViewRect.x, propertyViewRect.y + propertyViewRect.height + spacing, propertyViewRect.width, 20);
    }

    public void Reset(bool fullReset)
    {
        if (fullReset)
        {
            if (activeWrapper != null)
            {
                activeWrapper.OnDestroy();
            }
            activeWrapper = null;
        }
        logMessages.Clear();
        prevScreenSize = 0;
    }

    public void LoadNewPackage(string packageFileName, Action finishedCallback = null)
    {
        if (activeWrapper != null)
        {
            activeWrapper.OnDestroy();
        }
        activeWrapper = new PackageWrapper(packageFileName, arrayDefinitions, LogString);
        activeWrapper.LogString = LogString;
        if (activeAdapter != null)
        {
            if (!activeAdapter.IsCompatible(activeWrapper))
            {
                LogString(string.Format("Assigned adapter ({0}) is not compatible", activeAdapter.Name), Color.yellow);
                RemoveAdapter();
            }
        }
        treeBuilder = new BackgroundWorker {WorkerSupportsCancellation = true};
        treeBuilder.DoWork += activeWrapper.Load;
        treeBuilder.RunWorkerCompleted += OnFinishedBuildingTree;
        isBuildingTree = true;
        treeBuilder.RunWorkerAsync(finishedCallback);
    }

    void OnFinishedBuildingTree(object sender, RunWorkerCompletedEventArgs e)
    {
        if (openedPackages.Count > 0)
        {
            LogString("Resolving references from stashed packages, this can take a while!", Color.blue);
            activeWrapper.ResolveReferences(openedPackages.Where(o => o.Name != activeWrapper.Name).ToList());
        }
        LogString("Finished building tree view", Color.green);
        isBuildingTree = false;
        treeBuilder.RunWorkerCompleted -= OnFinishedBuildingTree;
        treeBuilder.DoWork -= activeWrapper.Load;
        treeBuilder = null;
        var callback = e.Result as Action;
        if (callback != null)
        {
            EditorApplication.delayCall += new EditorApplication.CallbackFunction(callback);
        }
    }

    public void CreateAdapter(Type adapterType)
    {
        if (!adapterType.IsSubclassOf(typeof (ExtractorAdapter)))
        {
            return;
        }
        var newAdapter = Activator.CreateInstance(adapterType) as ExtractorAdapter;
        if (newAdapter == null)
        {
            LogString("Error creating adapter", Color.yellow);
            return;
        }
        if (!newAdapter.IsCompatible(activeWrapper))
        {
            LogString(string.Format("Assigned adapter ({0}) is not compatible", newAdapter.Name), Color.yellow);
        }
        else
        {
            newAdapter.extractorWindowRef = this;
            newAdapter.OnLogMessage = LogString;
            activeAdapter = newAdapter;
        }
    }

    public void RemoveAdapter()
    {
        if (activeAdapter != null)
        {
            activeAdapter.OnLogMessage = null;
            activeAdapter.extractorWindowRef = null;
        }
        activeAdapter = null;
    }

    public void OnGUI()
    {
        GUI.BeginGroup(logRect, EditorStyles.textArea);
        var logLabelRect = new Rect(0, 0, logViewRect.width, 20);
        logScrollPos = GUI.BeginScrollView(logScrollRect, logScrollPos, logViewRect, false, true);
        for (var i = 0; i < logMessages.Count; i++)
        {
            logLabelRect.y = logLabelRect.height*i;
            if (logLabelRect.y + logLabelRect.height >= logScrollPos.y && logLabelRect.y <= logScrollPos.y + logRect.height)
            {
                var m = logMessages[logMessages.Count - i - 1];
                GUI.backgroundColor = m.color;
                GUI.Label(logLabelRect, m.text, EditorStyles.helpBox);
            }
        }
        GUI.backgroundColor = Color.white;
        logViewRect.height = Mathf.Max(logLabelRect.y + logLabelRect.height, logRect.height + 1);
        GUI.EndScrollView();
        GUI.EndGroup();

        GUI.enabled = !isBuildingTree;
        GUI.BeginGroup(contentRect, EditorStyles.helpBox);
        if (activeWrapper != null)
        {
            activeWrapper.Draw(contentRect);
        }
        GUI.EndGroup();
        GUI.BeginGroup(propertyViewRect, EditorStyles.helpBox);
        if (activeWrapper != null)
        {
            if (propertiesTextStyle == null)
            {
                propertiesTextStyle = EditorStyles.label;
                propertiesTextStyle.richText = true;
            }
            activeWrapper.DrawProperties(propertyViewRect, propertiesTextStyle);
        }
        GUI.EndGroup();
        GUI.enabled = activeWrapper != null && activeWrapper.inspectedObject != null;
        if (GUI.Button(passToAdapterRect, "Pass Selection To Adapter"))
        {
            PassToAdapter(activeWrapper.inspectedObject);
        }
        GUI.enabled = activeWrapper != null;
        if (GUI.Button(new Rect(passToAdapterRect.x, passToAdapterRect.y + 20, passToAdapterRect.width, passToAdapterRect.height), "Pass Package to Adapter"))
        {
            PassToAdapter(null);
        }
        GUI.enabled = true;
        GUI.BeginGroup(menuRect, EditorStyles.helpBox);
        GUI.enabled = !isBuildingTree;
        if (GUI.Button(loadButtonRect, "Open Package"))
        {
            var fileName = EditorUtility.OpenFilePanel("SBPackage", lastPath, "*sba;*sbm;*sbu;*sbx;*sbt;*sbw;*sbg");
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                lastPath = fileName;
                if (resources == null)
                {
                    LogString("Missing SBResources file, prompting to load manually", Color.yellow);
                    var sbresFile = EditorUtility.OpenFilePanel("Select SBResources", "", "asset");
                    if (!string.IsNullOrEmpty(sbresFile))
                    {
                        resources = AssetDatabase.LoadAssetAtPath<SBResources>(sbresFile);
                    }
                }
                if (localizedStrings == null)
                {
                    LogString("Missing SBLocalizedStrings file, prompting to load manually", Color.yellow);
                    var sblocFile = EditorUtility.OpenFilePanel("Select SBLocalizedStrings", "", "asset");
                    if (!string.IsNullOrEmpty(sblocFile))
                    {
                        localizedStrings = AssetDatabase.LoadAssetAtPath<SBLocalizedStrings>(sblocFile);
                    }
                }
                if (arrayDefinitions == null)
                {
                    LogString("Missing ArrayDefinitions file, prompting to load manually", Color.yellow);
                    var adFile = EditorUtility.OpenFilePanel("Select ArrayDefinitions", "", "asset");
                    if (!string.IsNullOrEmpty(adFile))
                    {
                        arrayDefinitions = AssetDatabase.LoadAssetAtPath<PackageReadConfig>(adFile);
                    }
                }
                if (resources != null && localizedStrings != null && arrayDefinitions != null)
                {
                    Reset(false);
                    LogString("Loading Package: " + fileName, Color.green);
                    LoadNewPackage(lastPath);
                }
                else
                {
                    Reset(true);
                    LogString("resource file, localizedStrings, or arraydefinitions missing. Aborting", Color.red);
                }
            }
        }
        GUI.enabled = true;
        if (activeWrapper != null)
        {
            GUI.enabled = isBuildingTree == false;
            if (GUI.Button(reopenRect, "Reload"))
            {
                Reset(true);
                LoadNewPackage(lastPath);
            }
            if (GUI.Button(pushRect, "Stash"))
            {
                for (var i = 0; i < openedPackages.Count; i++)
                {
                    if (openedPackages[i].Name == activeWrapper.Name)
                    {
                        LogString("Already in stash", Color.yellow);
                        return;
                    }
                }
                openedPackages.Add(activeWrapper);
                activeWrapper.OnLogMessage = null;
                activeWrapper = null;
                return;
            }
            GUI.enabled = true;
            GUI.Label(openedInfoRect, string.Format("Package: {0}", activeWrapper.Name));
            if (activeAdapter == null)
            {
                var selectedIndex = 0;
                selectedIndex = EditorGUI.Popup(new Rect(adapterRect.x, adapterRect.y, adapterRect.width, 15), selectedIndex, availableAdapters.Keys.ToArray());
                if (selectedIndex > 0)
                {
                    var adapterType = availableAdapters.Values.ToArray()[selectedIndex];
                    CreateAdapter(adapterType);
                }
            }
        }
        if (activeAdapter != null)
        {
            GUI.backgroundColor = Color.gray;
            GUI.Label(new Rect(menuRect.x - 5f, openedInfoRect.y + 20, menuRect.width, 16), activeAdapter.Name, EditorStyles.miniButton);
            GUI.backgroundColor = Color.white;
            if (GUI.Button(new Rect(menuRect.width - 30, openedInfoRect.y + 20, 16, 16), "X"))
            {
                RemoveAdapter();
                goto CANCEL;
            }
            GUILayout.BeginArea(adapterRect);
            GUILayout.Label(activeAdapter.Description, EditorStyles.helpBox, GUILayout.Width(adapterRect.width - 10));
            activeAdapter.DrawGUI(adapterRect);
            GUILayout.EndArea();
        }
        CANCEL:
        GUI.EndGroup();
        GUILayout.BeginArea(openedPackageRect, EditorStyles.helpBox);
        openedPackageScrollPos = GUILayout.BeginScrollView(openedPackageScrollPos);
        for (var i = 0; i < openedPackages.Count; i++)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(openedPackages[i].Name, EditorStyles.helpBox))
            {
                Reset(true);
                activeWrapper = openedPackages[i];
                openedPackages.RemoveAt(i);
                break;
            }
            if (GUILayout.Button("X"))
            {
                openedPackages[i].OnDestroy();
                openedPackages.RemoveAt(i);
                break;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}