using Common;

namespace Gameplay.Items
{
    public class Item_ArmorLeftShin : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ArmorLeftShin;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_LEFTSHIN;
        }
    }
}