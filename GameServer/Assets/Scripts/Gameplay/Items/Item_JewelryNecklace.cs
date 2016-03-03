using Common;

namespace Gameplay.Items
{
    public class Item_JewelryNecklace : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_JewelryNecklace;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_NECKLACE;
        }
    }
}