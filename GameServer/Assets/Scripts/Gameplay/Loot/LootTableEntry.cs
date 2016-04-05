using System;
using Gameplay.Items;

namespace Gameplay.Loot
{
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
    }
}