using Common;

namespace Gameplay.Items
{
    public class Item_QuestItem : Item_Type
    {
        public override EItemType GetItemType()
        {
            return EItemType.IT_QuestItem;
        }
    }
}