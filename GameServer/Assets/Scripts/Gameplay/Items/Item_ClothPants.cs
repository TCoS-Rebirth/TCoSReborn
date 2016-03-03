using Common;

namespace Gameplay.Items
{
    public class Item_ClothPants : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ClothPants;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_PANTS;
        }
    }
}