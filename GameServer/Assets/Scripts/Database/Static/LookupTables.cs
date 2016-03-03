using System.Collections.Generic;
using Gameplay;
using Gameplay.Entities.NPCs;
using UnityEngine;

namespace Database.Static
{
    public class LookupTables : ScriptableObject
    {
        [SerializeField, ReadOnly] public Dictionary<int, NPCTList> factionMembers;

        public bool Init(List<NPCCollection> npcs, List<Taxonomy> factions)
        {
            var npcData = new List<NPC_Type>();
            foreach (var collection in npcs)
            {
                npcData.AddRange(collection.types);
            }

            factionMembers = FillFactionMembers(npcData, factions);
            Debug.Log("GameData : Populated faction members lookup table with " + factionMembers.Count + " factions");
            return true;
        }

        Dictionary<int, NPCTList> FillFactionMembers(List<NPC_Type> npcData, List<Taxonomy> factionData)
        {
            var output = new Dictionary<int, NPCTList>();
            foreach (var f in factionData)
            {
                var fID = f.ID;
                foreach (var nt in npcData)
                {
                    if (nt.TaxonomyFaction == null) continue;
                    if (nt.TaxonomyFaction.ID != fID) continue;
                    NPCTList existingList;
                    if (output.TryGetValue(fID, out existingList))
                    {
                        //If faction already in dictionary, get IntList, 
                        //remove existing, add new Int and re-add IntList
                        output.Remove(fID);
                        existingList.Values.Add(nt);
                        output.Add(fID, existingList);
                    }
                    else
                    {
                        //If new, just add to dictionary
                        var newList = new NPCTList();
                        newList.Values.Add(nt);
                        output.Add(fID, newList);
                    }
                }
            }
            return output;
        }
    }

    public class NPCTList
    {
        List<NPC_Type> _values = new List<NPC_Type>();

        public List<NPC_Type> Values
        {
            get { return _values; }
            set { _values = value; }
        }

        public int Count
        {
            get { return _values.Count; }
        }
    }
}