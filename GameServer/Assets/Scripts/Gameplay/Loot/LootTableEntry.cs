using System;
using Gameplay.Items;
using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Loot
{
    /// <summary>
    /// An item that the parent loot table might generate
    /// </summary>
    [Serializable]
    public class LootTableEntry
    {
        [ReadOnly]
        public Item_Type ItemType;
        [ReadOnly]
        public int MinQuantity;
        [ReadOnly]
        public int MaxQuantity;
        [ReadOnly]
        public int Chance;

        public int MaxLevel;
        
        public int MinLevel;
        
        public string TemporaryItemName;

        public DroppedItem GenerateLoot()
        {
            if (UnityEngine.Random.Range(0, 99) < Chance)
            {
                var output = new DroppedItem();

                int quant = 1;
                if (MaxQuantity > 1)
                {
                    quant = UnityEngine.Random.Range(MinQuantity, MaxQuantity);
                }

                output.Item = ItemType;
                output.MaxLevel = MaxLevel;
                output.MinLevel = MinLevel;
                output.Quantity = quant;

                return output;
            }
            else return null;
            
        }

    }
}