using Common;

namespace Gameplay.Items
{
    public class Item_WeaponDualWielding : Item_Type
    {
        public override SBAnimWeaponFlags GetWeaponType()
        {
            return SBAnimWeaponFlags.AnimWeapon_DualWielding;
        }

        public override EItemType GetItemType()
        {
            return EItemType.IT_WeaponDualWielding;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_MELEEWEAPON;
        }
    }
}