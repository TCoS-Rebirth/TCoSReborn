using Common;

namespace Gameplay.Items
{
    public class Item_WeaponDoubleHanded : Item_Type
    {
        public override SBAnimWeaponFlags GetWeaponType()
        {
            return SBAnimWeaponFlags.AnimWeapon_DoubleHanded;
        }

        public override EItemType GetItemType()
        {
            return EItemType.IT_WeaponDoublehanded;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_MELEEWEAPON;
        }
    }
}