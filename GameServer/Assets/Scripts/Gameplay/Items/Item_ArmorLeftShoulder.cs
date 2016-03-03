using Common;

namespace Gameplay.Items
{
    public class Item_ArmorLeftShoulder : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ArmorLeftShoulder;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_LEFTSHOULDER;
        }
    }
}