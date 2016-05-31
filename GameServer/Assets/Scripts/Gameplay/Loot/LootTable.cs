using System.Collections.Generic;
using Gameplay.Items;
using UnityEngine;
using Common;

namespace Gameplay.Loot
{
    /// <summary>
    /// Collection of items the attached NPC (by type or by faction) might drop
    /// </summary>
    public class LootTable : ScriptableObject
    {
        /// <summary>
        /// Name by which the LT object is referred to in the packages
        /// </summary>
        [ReadOnly]
        public string Reference;

        [ReadOnly]
        public string Name;
        [ReadOnly]
        public ETableType TableType;
        [ReadOnly]
        public int MinDropQuantity;
        [ReadOnly]
        public int MaxDropQuantity;
        [ReadOnly]
        public int MoneyBase;
        [ReadOnly]
        public int MoneyPerLevel;

        [ReadOnly]
        public List<LootTableEntry> Entries = new List<LootTableEntry>();

        public List<DroppedItem> GenerateLoot()
        {
            //TODO: Take table type into account?
            var output = new List<DroppedItem>();

            if (MinDropQuantity > 0) {
                //Generate MinDropQuantity guaranteed drops
                for (int n = 0; n < MinDropQuantity; n++)
                {
                    output.Add(guaranteedDrop());
                }
            }


            //Roll random drops
            foreach(var entry in Entries)
            {
                output.Add(entry.GenerateLoot());
            }

            //If more than Max drops
            if (MaxDropQuantity > 0 && output.Count > MaxDropQuantity)
            {
                //Randomly remove an item from list until at max threshold count
                while (output.Count > MaxDropQuantity)
                {
                    int removeRand = Random.Range(0, output.Count);
                    output.RemoveAt(removeRand);
                }
            }

            return output;
        }

        public int GenerateMoney(int fameLevel)
        {
            int output = 0;
            output += MoneyBase;
            output += (MoneyPerLevel * fameLevel);
            return output;
        }

        DroppedItem guaranteedDrop ()
        {
            int chanceTot = 0;
            foreach(var entry in Entries) {
                chanceTot += entry.Chance;
            }

            int roll = Random.Range(0, chanceTot);
            int i = 0;
            for (i = 0 ; roll > 0; i++)
            {
                roll -= Entries[i].Chance;                
            }

            var selected = Entries[i];

            var drop = new DroppedItem();

            drop.Item = selected.ItemType;
            drop.MaxLevel = selected.MaxLevel;
            drop.MinLevel = selected.MinLevel;

            drop.Quantity = 1;
            if (selected.MaxQuantity > 1)
            {
                drop.Quantity = Random.Range(selected.MinQuantity, selected.MaxQuantity);
            }
            return drop;
        }

    }
}