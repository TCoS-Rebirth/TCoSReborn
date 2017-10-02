using System;
using System.Collections.Generic;
using Common;
using Gameplay.Entities;
using Network;
using UnityEngine;

namespace Gameplay.Items
{

    public class Game_PlayerItemManager : Game_ItemManager
    {

        PlayerCharacter Owner;

        public void Init(PlayerCharacter owner)
        {
            Owner = owner;
            LoadItems(Owner.dbRef.Items);
        }

        void OnItemChanged(Game_Item item, EItemChangeNotification notificationType, EItemLocationType locationType, int slotID, int locationID)
        {
            if (item == null)
            {
                var m = PacketCreator.S2C_GAME_PLAYERITEMMANAGER_SV2CL_REMOVEITEM(locationType, slotID, locationID);
                Owner.SendToClient(m);
            }
            else
            {
                var m = PacketCreator.S2C_GAME_PLAYERITEMMANAGER_SV2CL_SETITEM(item, notificationType);
                Owner.SendToClient(m);
            }
        }

        HashSet<int> filledItemSlots = new HashSet<int>();

        List<Game_Item> items = new List<Game_Item>();

        public const int MAX_INV = 200;

        int GetFreeItemSlot()
        {
            var i = 0;
            while (i < MAX_INV)
            {
                if (!filledItemSlots.Contains(i))
                {
                    break;
                }
                i++;
            }
            if (i == MAX_INV)
            {
                return -1;
            }
            return i;
        }

        public override bool HasFreeSpace(int slots)
        {
            return slots <= (MAX_INV - filledItemSlots.Count);
        }

        Game_Item CanStackToExisting(Game_Item item)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Type == item.Type)
                {
                    if (items[i].Type.StackableAmount == 0)
                    {
                        continue;
                    }
                    var existingContingent = items[i].Type.StackableAmount - items[i].StackSize;
                    if (existingContingent >= item.StackSize)
                    {
                        return items[i];
                    }
                }
            }
            return null;
        }

        public override bool AddItem(Game_Item item)
        {
            if (item == null)
            {
                throw new NullReferenceException("AddItem: Item null");
            }
            var stackToExisting = CanStackToExisting(item);
            if (stackToExisting == null)
            {
                var freeSlot = GetFreeItemSlot();
                if (freeSlot == -1)
                {
                    return false;
                }
                item.LocationType = EItemLocationType.ILT_Inventory;
                item.LocationSlot = freeSlot;
                items.Add(item);
                filledItemSlots.Add(freeSlot);
                OnItemChanged(item, EItemChangeNotification.ICN_Added, EItemLocationType.ILT_Inventory, freeSlot, 0);
                return true;
            }
            stackToExisting.StackSize += item.StackSize;
            OnItemChanged(stackToExisting, EItemChangeNotification.ICN_Stacked, EItemLocationType.ILT_Inventory, stackToExisting.LocationSlot, 0);
            return true;
        }

        public override bool RemoveItem(EItemLocationType locationType, int locationSlot, int locationID)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].LocationType == locationType && items[i].LocationSlot == locationSlot)
                {
                    OnItemChanged(null, EItemChangeNotification.ICN_Removed, locationType, locationSlot, locationID);
                    filledItemSlots.Remove(items[i].LocationSlot);
                    items[i].LocationType = EItemLocationType.ILT_Unknown;
                    items[i].LocationSlot = -1;
                    items[i].LocationID = -1;
                    items.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void MoveItem(EItemLocationType sourcelocationType, int sourceLocationSlot, int sourceLocationID, EItemLocationType targetLocationType,
            int targetLocationSlot, int targetLocationID)
        {
            var sourceItem = GetItem(sourcelocationType, sourceLocationSlot, sourceLocationID);
            if (sourceItem == null)
            {
                Debug.LogWarning("trying to move nonexistent item!");
                return;
            }
            //check if item in targetslot exists
            var targetItem = GetItem(targetLocationType, targetLocationSlot, targetLocationID);
            if (targetItem == null)
            {
                sourceItem.LocationType = targetLocationType;
                sourceItem.LocationSlot = targetLocationSlot;
                sourceItem.LocationID = targetLocationID;
                OnItemChanged(null, EItemChangeNotification.ICN_Removed, sourcelocationType, sourceLocationSlot, sourceLocationID);
                OnItemChanged(sourceItem, EItemChangeNotification.ICN_Moved, sourceItem.LocationType, sourceItem.LocationSlot, sourceItem.LocationID);
            }
            else
            {
                //check if stackable
                if (sourceItem.Type == targetItem.Type && targetItem.Type.StackableAmount > 0)
                {
                    var contingent = targetItem.Type.StackableAmount - targetItem.StackSize;
                    //can add stacks
                    if (contingent >= sourceItem.StackSize)
                    {
                        if (RemoveItem(sourcelocationType, sourceLocationSlot, sourceLocationID))
                        {
                            targetItem.StackSize += sourceItem.StackSize;
                            OnItemChanged(targetItem, EItemChangeNotification.ICN_Stacked, targetItem.LocationType, targetItem.LocationSlot, targetItem.LocationID);
                        }
                    }
                }
                else
                {
                    //check if swap places TODO
                    if (
                        (sourcelocationType == EItemLocationType.ILT_Equipment | targetLocationType == EItemLocationType.ILT_Equipment
                         && sourceItem.Type.GetEquipmentSlot() == targetItem.Type.GetEquipmentSlot())
                        ||
                        (sourcelocationType == targetLocationType & targetLocationType == EItemLocationType.ILT_Inventory)
                        )
                    {
                        sourceItem.LocationType = targetLocationType;
                        sourceItem.LocationSlot = targetLocationSlot;
                        sourceItem.LocationID = targetLocationID;
                        targetItem.LocationType = sourcelocationType;
                        targetItem.LocationSlot = sourceLocationSlot;
                        targetItem.LocationID = sourceLocationID;
                        OnItemChanged(sourceItem, EItemChangeNotification.ICN_Moved, sourceItem.LocationType, sourceItem.LocationSlot, sourceItem.LocationID);
                        OnItemChanged(targetItem, EItemChangeNotification.ICN_Moved, targetItem.LocationType, targetItem.LocationSlot, targetItem.LocationID);
                    }
                }
            }
        }

        public override List<Game_Item> GetItems(EItemLocationType location)
        {
            if (location == EItemLocationType.ILT_Unknown)
            {
                return items;
            }
            var equipList = new List<Game_Item>();
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].LocationType == location)
                {
                    equipList.Add(items[i]);
                }
            }
            return equipList;
        }

        public override Game_Item GetItem(EItemLocationType locationType, int locationSlot, int locationID)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].LocationType == locationType && items[i].LocationSlot == locationSlot)
                {
                    return items[i];
                }
            }
            return null;
        }

        public override Game_Item GetEquippedItem(EquipmentSlot slot)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].LocationType == EItemLocationType.ILT_Equipment && items[i].LocationSlot == (int)slot)
                {
                    return items[i];
                }
            }
            return null;
        }

        public void LoadItems(List<Game_Item> unsortedItems)
        {
            items = unsortedItems;
        }

        public bool HasItemStack(Content_Inventory.ContentItem item)
        {
            int count = 0;

            foreach (var i in items)
            {
                if (i.Type.resourceID == item.itemID)
                {
                    count += i.StackSize;
                    if (count >= item.StackSize) return true;
                }
            }
            return false;
        }

        public override void GiveInventory(Content_Inventory inventory)
        {
            foreach (var cItem in inventory.Items)
            {

                var gi = CreateInstance<Game_Item>();
                gi.SetupFromCItem(cItem);

                //TODO: Handle attuned status?
                //if (attuned) { gi.Attuned = 1; }

                AddItem(gi);
            }
        }

        public bool HasInventory(Content_Inventory inventory)
        {
            foreach (var cItem in inventory.Items)
            {
                if (!HasItemStack(cItem))
                {
                    return false;
                }
            }
            return true;
        }
    }
}