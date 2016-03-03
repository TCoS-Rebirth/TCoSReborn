using Common;

namespace Gameplay.Items
{
    public class Item_ArmorRightThigh : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ArmorRightThigh;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_RIGHTTHIGH;
        }
    }
}