using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Database.Static
{
    [Serializable]
    public class SBLocalizedString
    {
        public int ID;
        public string[] languageStrings = {"[No Text]", "[No Text]", "[No Text]", "[No Text]", "[No Text]", "[No Text]"};
    }

    public class SBLocalizedStrings : ScriptableObject
    {
        [SerializeField, HideInInspector] List<SBLocalizedString> strings = new List<SBLocalizedString>();

        public List<SBLocalizedString> Strings
        {
            get { return strings; }
        }

        public string GetString(int id, SBLanguage language)
        {
            if ((int) language >= 0 && (int) language <= 6)
            {
                for (var i = 0; i < strings.Count; i++)
                {
                    if (strings[i].ID == id)
                    {
                        return strings[i].languageStrings[(int) language];
                    }
                }
            }
            return "[Invalid StringReference]";
        }

        public SBLocalizedString GetString(int id)
        {
            for (var i = 0; i < strings.Count; i++)
            {
                if (strings[i].ID == id)
                {
                    return strings[i];
                }
            }
            return null;
        }
    }
}