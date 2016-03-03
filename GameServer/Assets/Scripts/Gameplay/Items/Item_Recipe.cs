using Common;

namespace Gameplay.Items
{
    public class Item_Recipe : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_Recipe;
        }
    }
}