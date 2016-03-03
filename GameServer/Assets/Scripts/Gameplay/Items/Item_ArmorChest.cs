using Common;

namespace Gameplay.Items
{
    public class Item_ArmorChest : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ArmorChest;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_CHESTARMOUR;
        }
    }
}