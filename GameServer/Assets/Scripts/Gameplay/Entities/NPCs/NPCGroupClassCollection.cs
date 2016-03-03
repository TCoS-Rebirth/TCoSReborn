using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Entities.NPCs
{
    public class NPCGroupClassCollection : ScriptableObject
    {
        public List<NPCGroupClass_Type> groupTypes = new List<NPCGroupClass_Type>();

        public NPCGroupClass_Type GetType(string typeName)
        {
            foreach (var t in groupTypes)
            {
                if (t.className == typeName)
                {
                    return t;
                }
            }
            return null;
        }
    }
}