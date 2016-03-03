using Common;

namespace Gameplay.Items
{
    public class Item_ArmorLeftThigh : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ArmorLeftThigh;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_LEFTTHIGH;
        }
    }
}