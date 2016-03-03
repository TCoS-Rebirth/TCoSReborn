using Common;

namespace Gameplay.Items
{
    public class Item_Resource : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_Resource;
        }
    }
}