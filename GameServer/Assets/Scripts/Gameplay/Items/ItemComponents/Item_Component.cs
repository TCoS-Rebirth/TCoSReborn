using Common;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Items.ItemComponents
{
    public class Item_Component : ScriptableObject
    {
        public virtual bool CanEquip(Character ch, Game_Item item)
        {
            return true;
        }

        public virtual bool CanUse(Character ch, Game_Item item)
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

        public virtual void OnEquip(Character ch, Game_Item item)
        {
        }

        public virtual void OnUnequip(Character ch, Game_Item item)
        {
        }

        public virtual void OnUse(Character ch, Game_Item item)
        {
        }
    }
}