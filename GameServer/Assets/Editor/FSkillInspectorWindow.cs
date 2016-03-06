using System;
using System.Collections;
using Utility;
using Gameplay.Skills;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Common;
using Gameplay;

public class FSkillInspectorWindow : EditorWindow
{
    FSkill _selectedSkill;
    GUIStyle _boxStyle;
    GUIStyle _bigLabelStyle;
    GUIStyle _smallLabelStyle;

    bool _hideInfoFields = false;

    string _fieldInclusions = "";

    void OnGUI()
    {
        SetupStyles();
        var prevColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.gray;
        GUILayout.BeginVertical(_boxStyle);
        GUI.backgroundColor = prevColor;
        GUILayout.BeginHorizontal();
        GUILayout.Label("FSkill asset:", GUILayout.ExpandWidth(false));
        _selectedSkill = EditorGUILayout.ObjectField(_selectedSkill, typeof (FSkill), false, GUILayout.ExpandWidth(false)) as FSkill;
        if (_selectedSkill != null)
        {
            GUILayout.Label("Selected: " + _selectedSkill.skillname, _bigLabelStyle, GUILayout.ExpandWidth(false));
        }
        GUILayout.EndHorizontal();
        if (_selectedSkill != null)
        {
            GUILayout.BeginHorizontal();
            _hideInfoFields = GUILayout.Toggle(_hideInfoFields, "Hide non object fields", GUILayout.ExpandWidth(false));
            GUILayout.Label("Except when fieldname contains (comma separated):", GUILayout.ExpandWidth(false));
            _fieldInclusions = GUILayout.TextField(_fieldInclusions);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        if (_selectedSkill == null) return;
        DrawTree(_selectedSkill);
    }

    void SetupStyles()
    {
        if (_boxStyle == null)
        {
            _boxStyle = new GUIStyle(EditorStyles.helpBox) {stretchWidth = false};
        }
        if (_bigLabelStyle == null)
        {
            _bigLabelStyle = new GUIStyle(EditorStyles.label) {fontSize = 10, fontStyle = FontStyle.Bold};
        }
        if (_smallLabelStyle == null)
        {
            _smallLabelStyle = new GUIStyle(EditorStyles.label) {fontSize = 10};
        }
    }

    Vector2 scrollPos;
    void DrawTree(FSkill skill)
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        foreach (var keyFrame in skill.keyFrames)
        {
            if (keyFrame.EventGroup == null) continue;
            foreach (var skillEvent in keyFrame.EventGroup.events)
            {
                if (skillEvent == null) continue;
                GUILayout.Label(string.Format("SkillEvent: ({0}) {1} =", skillEvent.name, keyFrame.Name), GUILayout.ExpandWidth(false));
                RecurseNodes(skillEvent);
            }
        }
        GUILayout.EndScrollView();
    }

    void RecurseNodes(object s)
    {
        var indentation = 30;
        var infoFields = new List<FieldInfo>();
        var objectFields = new List<FieldInfo>();
        var objectListFields = new List<Tuple<FieldInfo, List<object>>>();
        var fields = s.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
        foreach (var field in fields)
        {
            if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var itemType = field.FieldType.GetGenericArguments()[0];
                if (!itemType.IsAssignableFrom(typeof(Taxonomy)))
                {
                    var tpl = new Tuple<FieldInfo, List<object>>(field, new List<object>());
                    var lst = (IList)field.GetValue(s);
                    if (lst != null)
                    {
                        foreach (var listEntry in lst)
                        {
                            if (listEntry == null) continue;
                            tpl.Item2.Add(listEntry);
                        }
                    }
                    objectListFields.Add(tpl);
                }
                else
                {
                    infoFields.Add(field);
                }
            }
            else
            {
                if (field.FieldType.IsSubclassOf(typeof (UnityEngine.Object)))
                {
                    objectFields.Add(field);
                }
                else
                {
                    infoFields.Add(field);
                }
            }
        }
        GUILayout.BeginVertical(_boxStyle, GUILayout.ExpandWidth(false));
        var o = s as UnityEngine.Object;
        if (o == null)
        {
            GUILayout.Label(s.GetType().Name, _bigLabelStyle, GUILayout.ExpandWidth(false));
        }
        else
        {
            GUILayout.Label(string.Format("({0}) {1}", o.GetType().Name, o.name), _bigLabelStyle, GUILayout.ExpandWidth(false));
        }
        var filters = new List<string>(_fieldInclusions.Split(new[] {','},StringSplitOptions.RemoveEmptyEntries));
        for (var i=0;i<infoFields.Count;i++)
        {
            var skip = _hideInfoFields;
            if (filters.Count > 0)
            {
                foreach (var filter in filters)
                {
                    if (infoFields[i].Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    {
                        skip = false;
                        break;
                    }
                }
            }               
            if (!skip)
            {
                GUILayout.Label(string.Format("{0}{1} = {2}",i==infoFields.Count-1? "└" : "│", infoFields[i].Name, infoFields[i].GetValue(s)), GUILayout.ExpandWidth(false));
            }
        }
        foreach (var objList in objectListFields)
        {
            GUILayout.Label(objList.Item1.Name + "= {", _bigLabelStyle, GUILayout.ExpandWidth(false));
            for (var i=0;i<objList.Item2.Count;i++)
            {
                GUILayout.Label("Entry: " + i, GUILayout.ExpandWidth(false));
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                GUILayout.Space(indentation);
                RecurseNodes(objList.Item2[i]);
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("}", _bigLabelStyle, GUILayout.ExpandWidth(false));
        }
        foreach (var field in objectFields)
        {
            var fieldContent = field.GetValue(s);
            if (fieldContent == null) continue;   
            GUILayout.Label(field.Name+"=", _bigLabelStyle, GUILayout.ExpandWidth(false));
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Space(indentation);
            RecurseNodes(fieldContent);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }
}
