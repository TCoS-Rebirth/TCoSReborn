using Common;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Items.ItemComponents
{
    public class Item_Component : ScriptableObject
    {
        public virtual bool CanEquip(Character ch, Item_Type item)
        {
            return true;
        }

        public virtual bool CanUse(Character ch, Item_Type item)
        {
            return true;
        }

        public virtual bool IsNPCItem()
        {
            return false;
        }

        public virtual EAppMainWeaponType GetWeaponType()
        {
            return EAppMainWeaponType.EMW_Undetermined;
        }

        public virtual void OnDraw(Character ch)
        {
        }

        public virtual void OnSheathe(Character ch)
        {
        }

        public virtual void OnEquip(Character ch, Item_Type item)
        {
        }

        public virtual void OnUnequiip(Character ch, Item_Type item)
        {
        }

        public virtual void OnUse(Character ch, Item_Type item)
        {
        }
    }
}