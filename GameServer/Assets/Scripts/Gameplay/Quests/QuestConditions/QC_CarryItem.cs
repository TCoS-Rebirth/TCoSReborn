//==============================================================================
//  QC_CarryItem
//==============================================================================

using Gameplay.Entities;

namespace Gameplay.Quests.QuestConditions
{
    public class QC_CarryItem : QuestCondition
    {
        public Content_Inventory Cargo;


        public override bool UpdateAndCheck(PlayerCharacter pc, Quest_Type parentQuest)
        {
            //TODO : Proper player inventory check

            return base.UpdateAndCheck(pc, parentQuest);            
        }
        
    }
}