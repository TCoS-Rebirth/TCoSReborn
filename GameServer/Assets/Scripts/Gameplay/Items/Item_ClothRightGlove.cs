using Common;

namespace Gameplay.Items
{
    public class Item_ClothRightGlove : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ClothRightGlove;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_RIGHTGLOVE;
        }
    }
}