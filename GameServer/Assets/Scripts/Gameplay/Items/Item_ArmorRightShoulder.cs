using Common;

namespace Gameplay.Items
{
    public class Item_ArmorRightShoulder : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ArmorRightShoulder;
        }

        public override EquipmentSlot GetEquipmentSlot()
        {
            return EquipmentSlot.ES_RIGHTSHOULDER;
        }
    }
}