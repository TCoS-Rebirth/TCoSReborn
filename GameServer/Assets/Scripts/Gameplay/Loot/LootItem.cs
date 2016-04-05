using Common;
using Gameplay.Entities;
using Gameplay.Items;
using System.Collections.Generic;

namespace Gameplay.Loot
{
    public class LootItem
    {
        [ReadOnly]
        public static float LOOT_TIMEOUT = 120;
        [ReadOnly]
        public int lootID;
        [ReadOnly]
        public Item_Type itemType;
        [ReadOnly]
        public float timer;
        [ReadOnly]
        public bool timerRunning;
        [ReadOnly]
        public int lootMasterID;
        [ReadOnly]
        public ELootMode lootMode;
        [ReadOnly]
        public ELootSource lootSource;
        [ReadOnly]
        public List<PlayerCharacter> receivers;
        [ReadOnly]
        public bool assigned;
        [ReadOnly]
        public int passSet;


    }
}
