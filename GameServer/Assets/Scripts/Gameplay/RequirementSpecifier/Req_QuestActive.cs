using System;
using Gameplay.Entities;
using Database.Static;

namespace Gameplay.RequirementSpecifier
{
    public class Req_QuestActive : Content_Requirement
    {
        public SBResource RequiredQuest; //Quest_Type

        public override bool isMet(PlayerCharacter p)
        {
            foreach (var quest in p.QuestData.curQuests)
            {
                if (quest.questID == RequiredQuest.ID) { return true; }
            }
            return false;
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }
    }
}