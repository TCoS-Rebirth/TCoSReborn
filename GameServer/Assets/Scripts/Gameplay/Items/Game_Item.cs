using System;
using Common;
using UnityEngine;
using Gameplay.Loot;
using Gameplay.Entities;

namespace Gameplay.Items
{
    [Serializable]
    public class Game_Item : ScriptableObject
    {
        public byte Attuned = 0;
        public int CharacterID = -1;
        public byte Color1 = 0;
        public byte Color2 = 0;
        public int DBID = 0;
        public int LocationID = 0;
        public int LocationSlot = 0;
        public EItemLocationType LocationType = EItemLocationType.ILT_Unknown;
        public int StackSize = 1;
        public Item_Type Type;
        public float UseTime;

        //Constructor from ContentItem object
        public void SetupFromCItem(Content_Inventory.ContentItem cItem)
        {
            Color1 = cItem.Color1;
            Color2 = cItem.Color2;
            Type = cItem.Item;
            StackSize = cItem.StackSize;
        }

        public void SetupFromLoot(LootTransaction.DroppedLootItem dlItem)
        {
            Attuned = (byte)(dlItem.Item.Item.BindOnPickup ? 1 : 0);      
            Type = dlItem.Item.Item;
            StackSize = dlItem.Item.Quantity;
        }
    }
}