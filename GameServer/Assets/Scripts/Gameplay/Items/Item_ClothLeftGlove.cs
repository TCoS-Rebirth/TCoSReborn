using Common;

namespace Gameplay.Items
{
    public class Item_ClothLeftGlove : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ClothLeftGlove;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_LEFTGLOVE;
        }
    }
}