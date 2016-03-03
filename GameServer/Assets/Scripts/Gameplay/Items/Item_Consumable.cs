using Common;

namespace Gameplay.Items
{
    public class Item_Consumable : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_Consumable;
        }
    }
}