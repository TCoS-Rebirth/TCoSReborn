using System.Collections.Generic;
using Common;
using Gameplay.Entities;
using Gameplay.Items.ItemComponents;
using Gameplay.RequirementSpecifier;
using UnityEngine;

namespace Gameplay.Items
{
    public class Item_Type : ScriptableObject
    {
        public bool BindOnEquip;
        public bool BindOnPickup;
        public int BuyPriceValue;

        public List<Item_Component> Components = new List<Item_Component>();
        public string Description;
        public bool Equipable;
        public EquipmentSlot equipmentSlot;
        public string internalName;
        public EItemRarity ItemRarity;

        [Header("ReadOnly")]
        public EItemType ItemType;

        public byte MinLevel;

        [Header("Info")]
        public string Name;

        public bool RecyclableIntoMoney;
        public int RecyclePriceValue;
        public List<Content_Requirement> Requirements = new List<Content_Requirement>();
        public int resourceID;
        public bool Sellable;
        public int SellPriceValue;

        [Header("General")]
        public int StackableAmount;

        public bool Tradable;

        [Header("OnUse")]
        public float UseCooldown;

        public virtual SBAnimWeaponFlags GetWeaponType()
        {
            return SBAnimWeaponFlags.AnimWeapon_None;
        }

        public virtual EItemType GetItemType()
        {
            return EItemType.IT_MiscellaneousKey;
        }

        public virtual EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_NO_SLOT;
        }

        public void OnSheathe(Character aPawn)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] != null)
                {
                    Components[i].OnSheathe(aPawn);
                }
            }
        }

        public void OnDraw(Character aPawn)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] != null)
                {
                    Components[i].OnDraw(aPawn);
                }
            }
        }

        public void OnUnequip(Character aPawn, Game_Item aItem)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] != null)
                {
                    Components[i].OnUnequip(aPawn, aItem);
                }
            }
        }

        public void OnEquip(Character aPawn, Game_Item aItem)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] != null)
                {
                    Components[i].OnEquip(aPawn, aItem);
                }
            }
        }

        public bool CanEquip(Character aPawn, Game_Item aItem)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] != null)
                {
                    if (!Components[i].CanEquip(aPawn, aItem)) return false;
                }
            }
            return true;
        }

        public bool CanUse(Character aPawn, Game_Item aItem)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] != null)
                {
                    if (!Components[i].CanUse(aPawn, aItem)) return false;
                }
            }
            for (int i = 0; i < Requirements.Count; i++)
            {
                if (Requirements[i] != null)
                {
                    if (!Requirements[i].CheckPawn(aPawn))
                    {
                        return false;
                    }
                }
            }
            if (aPawn.Stats.FameLevel < MinLevel)
            {
                return false;
            }
            return true;
        }

    }
}