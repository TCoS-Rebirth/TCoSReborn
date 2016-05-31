using Common;
using Gameplay.Entities;
using Gameplay.Items;
using System.Collections.Generic;

namespace Gameplay.Loot
{
    /// <summary>
    /// This class only appears in later Spellborn versions than currently used
    /// Revisit this if we upgrade to the later versions!
    /// </summary>
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
