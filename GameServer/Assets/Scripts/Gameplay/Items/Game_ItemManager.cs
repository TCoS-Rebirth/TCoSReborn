using System.Collections.Generic;
using Common;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Items
{
    public abstract class Game_ItemManager: ScriptableObject
    {
        public abstract bool AddItem(Game_Item item);
        public abstract bool RemoveItem(EItemLocationType locationType, int locationSlot, int locationID);
        public abstract bool HasFreeSpace(int slots);
        public abstract Game_Item GetEquippedItem(EquipmentSlot slot);
        public abstract List<Game_Item> GetItems(EItemLocationType location);
        public abstract Game_Item GetItem(EItemLocationType locationType, int locationSlot, int locationID);
        public abstract void GiveInventory(Content_Inventory inventory);
    }
}
