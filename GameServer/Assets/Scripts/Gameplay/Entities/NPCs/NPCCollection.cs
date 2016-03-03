using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Entities.NPCs
{
    public class NPCCollection : ScriptableObject
    {
        public List<NPC_Type> types = new List<NPC_Type>();

        internal NPC_Type GetItem(int rID)
        {
            for (var i = 0; i < types.Count; i++)
            {
                if (types[i].resourceID == rID)
                {
                    return types[i];
                }
            }
            return null;
        }
    }
}