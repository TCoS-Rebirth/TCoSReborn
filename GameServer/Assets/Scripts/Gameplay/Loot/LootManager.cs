using Common;
using Gameplay.Entities;
using System.Collections.Generic;

namespace Gameplay.Loot
{
    public class LootManager
    {
        private static LootManager _instance;

        [ReadOnly]
        public List<LootItem> lootItems;
        [ReadOnly]
        public int pvpLootItems;

        

        public void DropItems(List<LootTable> lootTables, List<PlayerCharacter> receivers, ELootMode lootMode, ELootSource lootSource)
        {
            //TODO
        }

        public static LootManager Get
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LootManager();
                }
                return _instance;
            }
        }

    }
}
