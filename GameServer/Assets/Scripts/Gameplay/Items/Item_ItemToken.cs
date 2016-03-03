using Common;

namespace Gameplay.Items
{
    public class Item_ItemToken : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_ItemToken;
        }
    }
}