using Common;
using Database.Static;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (SBLocalizedStrings))]
public class SBLocalizedStringsEditor : Editor
{
    float elementHeight = 16f;
    Rect filterInputRect = new Rect(100, 46, 90, 16);

    Rect filterLabelRect = new Rect(5, 46, 90, 16);
    SBLanguage languageFilter = SBLanguage.English;
    Vector2 scrollPos;

    void OnEnable()
    {
        languageFilter = SBLanguage.English;
    }

    public override void OnInspectorGUI()
    {
        var sls = target as SBLocalizedStrings;
        //if (sls.Strings.Count == 0)
        //{
        //    if (GUILayout.Button("Parse")) { Parse(sls); }
        //}
        float inspectorHeight = Screen.height - 74;
        float inspectorWidth = Screen.width - 20;
        GUI.Label(filterLabelRect, "LanguageFilter:");
        languageFilter = (SBLanguage) EditorGUI.EnumPopup(filterInputRect, languageFilter);
        scrollPos = GUI.BeginScrollView(new Rect(0, 60, inspectorWidth + 20, inspectorHeight - 20), scrollPos,
            new Rect(0, 0, inspectorWidth, elementHeight*sls.Strings.Count));
        for (var i = 0; i < sls.Strings.Count; i++)
        {
            var elementY = i*elementHeight;
            if (elementY < scrollPos.y)
            {
                continue;
            }
            ShowSingleEntry(new Rect(5, elementY, inspectorWidth, elementHeight), sls.Strings[i]);
            if (elementY >= scrollPos.y + inspectorHeight)
            {
                break;
            }
        }
        GUI.EndScrollView();
    }

    void ShowSingleEntry(Rect r, SBLocalizedString s)
    {
        GUI.Label(r, string.Format("{0,-8}: {1}", s.ID, s.languageStrings[(int) languageFilter]), EditorStyles.label);
    }

    //    int arraySize = fileReader.ReadInt32();
    //    fileReader.ReadInt32();//skip "header"
    //    BinaryReader fileReader = new BinaryReader(fs);
    //    FileStream fs = File.OpenRead(fileName);
    //    if (string.IsNullOrEmpty(fileName)) { return; }
    //    string fileName = EditorUtility.OpenFilePanel("", "", "s");
    //{

    //public void Parse(SBLocalizedStrings sbls)

    //    for (int i = 0; i < arraySize; ++i)
    //    {
    //        int recordId = fileReader.ReadInt32();
    //        int languageNumber = fileReader.ReadInt32();
    //        int stringLength = fileReader.ReadInt32();
    //        string data = System.Text.Encoding.UTF8.GetString(fileReader.ReadBytes(stringLength)).Replace("\0", string.Empty);
    //        fileReader.ReadByte();
    //        fileReader.ReadByte();
    //        fileReader.ReadByte();
    //        fileReader.ReadByte();
    //        fileReader.ReadByte();
    //        SBLocalizedString ls = sbls.GetString(recordId);
    //        bool isNew = false;
    //        if (ls == null) 
    //        { 
    //            ls = new SBLocalizedString();
    //            ls.ID = recordId;
    //            isNew = true;
    //        }
    //        ls.languageStrings[languageNumber-1] = data;
    //        if (isNew)
    //        {
    //            sbls.Strings.Add(ls);
    //        }
    //    }
    //    Debug.Log("Parsed " + sbls.Strings.Count + " localized strings");
    //    EditorUtility.SetDirty(sbls);
    //    EditorUtility.SetDirty(target);
    //}
}