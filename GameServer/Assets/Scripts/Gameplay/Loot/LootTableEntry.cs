using System;
using Gameplay.Items;

namespace Gameplay.Loot
{
    [Serializable]
    public class LootTableEntry
    {
        public int chance;
        public Item_Type itemType;
        public int maxLevel;
        public int maxQuantity;
        public int minLevel;
        public int minQuantity;
        public string temporaryItemName;
    }
}