using Common;

namespace Gameplay.Items
{
    public class Item_ArmorLeftGauntlet : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ArmorLeftGauntlet;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_LEFTGAUNTLET;
        }
    }
}