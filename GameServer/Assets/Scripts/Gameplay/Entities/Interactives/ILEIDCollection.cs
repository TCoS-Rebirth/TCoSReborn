using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.Entities.Interactives
{

    public class ILEIDCollection : ScriptableObject
    {

        public List<ILEID> ileIDs = new List<ILEID>();

        public bool TryAdd(ILEID i)
        {
            foreach(var member in ileIDs)
            {
                if (member.levelObjID == i.levelObjID)
                    return false;
                if (member.gameObjName == i.gameObjName)
                    return false;
            }

            ileIDs.Add(i);
            return true;
        }

        public bool Remove(int loID)
        {
            foreach (var member in ileIDs)
            {
                if (member.levelObjID == loID)
                {
                    ileIDs.Remove(member);
                    return true;
                }
            }
            return false;
        }

        public bool Remove(string goName)
        {
            foreach (var member in ileIDs)
            {
                if (member.gameObjName == goName)
                {
                    ileIDs.Remove(member);
                    return true;
                }
            }
            return false;
        }

        public int GetLOID(string goName)
        {
            foreach (var member in ileIDs)
            {
                if (member.gameObjName == goName) { return member.levelObjID; }
            }
            return -1;
        }

        public string GetGOName(int loID)
        {
            foreach(var member in ileIDs)
            {
                if (member.levelObjID == loID) { return member.gameObjName; }
            }
            return null;
        }
    }
}