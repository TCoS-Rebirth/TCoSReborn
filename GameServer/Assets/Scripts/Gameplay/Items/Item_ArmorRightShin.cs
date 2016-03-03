using Common;

namespace Gameplay.Items
{
    public class Item_ArmorRightShin : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ArmorRightShin;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_RIGHTSHIN;
        }
    }
}