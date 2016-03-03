using Common;

namespace Gameplay.Items
{
    public class Item_WeaponRanged : Item_Type
    {
        public override SBAnimWeaponFlags GetWeaponType()
        {
            return SBAnimWeaponFlags.AnimWeapon_Bow;
        }

        public override EItemType GetItemType()
        {
            return EItemType.IT_WeaponRanged;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_RANGEDWEAPON;
        }
    }
}