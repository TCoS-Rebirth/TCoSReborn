using Common;

namespace Gameplay.Items
{
    public class Item_Broken : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_Broken;
        }
    }
}