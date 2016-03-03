using Common;

namespace Gameplay.Items
{
    public class Item_ArmorHeadGear : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ArmorHeadGear;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_HELMET;
        }
    }
}