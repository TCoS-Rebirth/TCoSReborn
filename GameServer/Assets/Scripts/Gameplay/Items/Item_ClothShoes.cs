using Common;

namespace Gameplay.Items
{
    public class Item_ClothShoes : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ClothShoes;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_SHOES;
        }
    }
}