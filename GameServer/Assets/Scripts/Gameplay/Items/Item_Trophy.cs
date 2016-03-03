using Common;

namespace Gameplay.Items
{
    public class Item_Trophy : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_Trophy;
        }
    }
}