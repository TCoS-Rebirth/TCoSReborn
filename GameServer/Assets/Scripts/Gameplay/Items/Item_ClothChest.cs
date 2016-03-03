using Common;

namespace Gameplay.Items
{
    public class Item_ClothChest : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ClothChest;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_CHEST;
        }
    }
}