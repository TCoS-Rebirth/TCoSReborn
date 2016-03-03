using System;
using System.Collections.Generic;

namespace Gameplay.Items.ItemComponents
{
    public class IC_TokenSlot : Item_Component
    {
        public enum ESigilSlotType
        {
            SST_None,
            SST_Weapon_1,
            SST_Weapon_2,
            SST_Weapon_3,
            SST_Weapon_PVP,
            SST_Weapon_Ranged,
            SST_Armor_1,
            SST_Armor_2,
            SST_Armor_3,
            SST_Jewelry_Exclusive
        }

        public List<TokenSlot> slots = new List<TokenSlot>();

        [Serializable]
        public class TokenSlot
        {
            public int rank;
            public ESigilSlotType SlotType;
        }
    }
}