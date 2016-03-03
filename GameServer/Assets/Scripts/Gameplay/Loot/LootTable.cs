using System.Collections.Generic;
using Gameplay.Items;
using UnityEngine;

namespace Gameplay.Loot
{
    public class LootTable : ScriptableObject
    {
        [SerializeField] List<LootTableEntry> entries = new List<LootTableEntry>();

        public Game_Item GenerateLoot()
        {
            Debug.LogWarning("TODO (calculate it properly)");
            if (entries.Count == 0)
            {
                return null;
            }
            //LootTableEntry rndEntry = entries[Random.Range(0, entries.Count - 1)];
            //return rndEntry.item;
            return null;
        }
    }
}