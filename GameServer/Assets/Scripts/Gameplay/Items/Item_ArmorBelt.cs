using Common;

namespace Gameplay.Items
{
    public class Item_ArmorBelt : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ArmorBelt;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_BELT;
        }
    }
}