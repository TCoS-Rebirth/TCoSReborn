using System;
using Gameplay.Entities;
using Database.Static;

namespace Gameplay.RequirementSpecifier
{
    public class Req_TargetActive : Content_Requirement
    {        
        public SBResource quest; //Quest_Type
        public int objective;

        public override bool isMet(PlayerCharacter p)
        {
            var fullQuest = GameData.Get.questDB.GetQuest(quest.ID);

            //target active if target not completed and all pretargets complete
            //return (    (!p.QuestTargetIsComplete(fullQuest, objective))
            //        &&  p.PreTargetsComplete(fullQuest.targets[objective], fullQuest));

            //Valshaaran - experimental - remove pretargets requirement, just require quest to be active instead
            //ConditionalEnemy data seems to imply that this is the correct implementation
            //(NPC spawn dependent on target progress the player needs to talk to said NPC for, otherwise)
            return (!p.QuestTargetIsComplete(fullQuest, objective)
                    && p.HasQuest(quest.ID));

        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }

        public override bool CheckPawn(Character character)
        {
            var p = character as PlayerCharacter;
            if (!p) return false;
            return isMet(p);
        }
    }
}