using Common;

namespace Gameplay.Items
{
    public class Item_WeaponOneHanded : Item_Type
    {
        public override SBAnimWeaponFlags GetWeaponType()
        {
            return SBAnimWeaponFlags.AnimWeapon_SingleHanded;
        }

        public override EItemType GetItemType()
        {
            return EItemType.IT_WeaponOneHanded;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_MELEEWEAPON;
        }
    }
}