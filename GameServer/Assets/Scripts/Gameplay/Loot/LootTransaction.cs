using Common;
using Gameplay.Entities;
using System.Collections.Generic;

namespace Gameplay.Loot
{
    public class LootTransaction
    {

        public int TransactionID;

        public List<DroppedLootItem> LootItems;

        public List<PlayerCharacter> Receivers;

        public List<GroupLootSelection> SelectedDrops;

        public bool TimedTransaction;

        public float CurrentTimer;

        public ELootMode LootMode;

        public class DroppedLootItem
        {
            public int LootItemID;
            public DroppedItem Item;
            public bool Given;
            
        }

        public class GroupLootSelection
        {
            public int DroppedItemIndex;
            public List<PlayerCharacter> NeedList;
            public List<PlayerCharacter> GreedList;
        }

        
    }
}
