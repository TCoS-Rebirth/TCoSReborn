using System.Collections.Generic;
using Common;
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

        [Header("ReadOnly")] public EItemType ItemType;

        public byte MinLevel;

        [Header("Info")] public string Name;

        public bool RecyclableIntoMoney;
        public int RecyclePriceValue;
        public List<Content_Requirement> Requirements = new List<Content_Requirement>();
        public int resourceID;
        public bool Sellable;
        public int SellPriceValue;

        [Header("General")] public int StackableAmount;

        public bool Tradable;

        [Header("OnUse")] public float UseCooldown;

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
    }
}