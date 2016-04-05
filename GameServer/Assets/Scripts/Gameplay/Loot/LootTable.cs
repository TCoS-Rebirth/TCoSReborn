using System.Collections.Generic;
using Gameplay.Items;
using UnityEngine;
using Common;

namespace Gameplay.Loot
{
    public class LootTable : ScriptableObject
    {
        [ReadOnly]
        public string Name;
        [ReadOnly]
        ETableType TableType;
        [ReadOnly]
        int MinDropQuantity;
        [ReadOnly]
        int MaxDropQuantity;
        [ReadOnly]
        int MoneyBase;
        [ReadOnly]
        int MoneyPerLevel;

        [ReadOnly]
        public List<LootTableEntry> Entries = new List<LootTableEntry>();


        public Game_Item GenerateLoot()
        {
            Debug.LogWarning("TODO (calculate it properly)");
            if (Entries.Count == 0)
            {
                return null;
            }
            //LootTableEntry rndEntry = entries[Random.Range(0, entries.Count - 1)];
            //return rndEntry.item;
            return null;
        }
    }
}