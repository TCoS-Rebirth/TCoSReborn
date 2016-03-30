using System;
using System.Collections.Generic;
using Common;
using Gameplay.Items;
using UnityEngine;

namespace Gameplay.Entities.Players
{
    [Serializable]
    public class Player_ItemManager
    {
        public delegate void ItemChangeNotification(Game_Item item, EItemChangeNotification type, EItemLocationType locationType, int locationSlot, int locationID);

        HashSet<int> filledItemSlots = new HashSet<int>();

        List<Game_Item> items = new List<Game_Item>();
        public ItemChangeNotification OnItemChanged;

        public Player_ItemManager(ItemChangeNotification itemChangedCallback)
        {
            OnItemChanged = itemChangedCallback;
        }

        int GetFreeItemSlot()
        {
            var i = 0;
            while (i < 200)
            {
                if (!filledItemSlots.Contains(i))
                {
                    break;
                }
                i++;
            }
            if (i == 200)
            {
                return -1;
            }
            return i;
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

        public bool AddItem(Game_Item item)
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
                if (OnItemChanged != null)
                {
                    OnItemChanged(item, EItemChangeNotification.ICN_Added, EItemLocationType.ILT_Inventory, freeSlot, 0);
                }
                return true;
            }
            stackToExisting.StackSize += item.StackSize;
            if (OnItemChanged != null)
            {
                OnItemChanged(stackToExisting, EItemChangeNotification.ICN_Stacked, EItemLocationType.ILT_Inventory, stackToExisting.LocationSlot, 0);
            }
            return true;
        }

        public bool RemoveItem(EItemLocationType locationType, int locationSlot, int locationID)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].LocationType == locationType && items[i].LocationSlot == locationSlot)
                {
                    if (OnItemChanged != null)
                    {
                        OnItemChanged(null, EItemChangeNotification.ICN_Removed, locationType, locationSlot, locationID);
                    }
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
                if (OnItemChanged != null)
                {
                    OnItemChanged(null, EItemChangeNotification.ICN_Removed, sourcelocationType, sourceLocationSlot, sourceLocationID);
                    OnItemChanged(sourceItem, EItemChangeNotification.ICN_Moved, sourceItem.LocationType, sourceItem.LocationSlot, sourceItem.LocationID);
                }
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
                            if (OnItemChanged != null)
                            {
                                OnItemChanged(targetItem, EItemChangeNotification.ICN_Stacked, targetItem.LocationType, targetItem.LocationSlot, targetItem.LocationID);
                            }
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
                        if (OnItemChanged != null)
                        {
                            OnItemChanged(sourceItem, EItemChangeNotification.ICN_Moved, sourceItem.LocationType, sourceItem.LocationSlot, sourceItem.LocationID);
                            OnItemChanged(targetItem, EItemChangeNotification.ICN_Moved, targetItem.LocationType, targetItem.LocationSlot, targetItem.LocationID);
                        }
                    }
                }
            }
        }

        public List<Game_Item> GetItems(EItemLocationType location)
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

        public Game_Item GetItem(EItemLocationType locationType, int locationSlot, int locationID)
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

        public Game_Item GetEquippedItem(EquipmentSlot slot)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].LocationType == EItemLocationType.ILT_Equipment && items[i].LocationSlot == (int) slot)
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
    }
}