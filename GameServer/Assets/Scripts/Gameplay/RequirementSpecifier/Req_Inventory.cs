using Common;
using Gameplay.Entities;
using Gameplay.Items;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Inventory : Content_Requirement
    {
        public int Amount;
        public Item_Type Item;
        public int itemID;
        public EContentOperator Operator;
        public string temporaryItemName;

        public override bool isMet(PlayerCharacter p)
        {
            var itemCount = 0;
            foreach (var it in p.ItemManager.GetItems(EItemLocationType.ILT_Inventory))
            {
                if (it.Type.resourceID == itemID)
                {
                    itemCount += it.StackSize;
                }
            }

            if (SBOperator.Operate(itemCount, Operator, Amount))
            {
                return true;
            }
            return false;
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }
    }
}