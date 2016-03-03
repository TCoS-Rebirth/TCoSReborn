using Common;

namespace Gameplay.Items
{
    public class Item_ArmorRightGauntlet : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ArmorRightGauntlet;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_RIGHTGAUNTLET;
        }
    }
}