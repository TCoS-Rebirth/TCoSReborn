using Common;

namespace Gameplay.Items
{
    public class Item_WeaponShield : Item_Type
    {
        public override SBAnimWeaponFlags GetWeaponType()
        {
            return SBAnimWeaponFlags.AnimWeapon_SingleShield;
        }

        public override EItemType GetItemType()
        {
            return EItemType.IT_WeaponShield;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_SHIELD;
        }
    }
}