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
            foreach (var quest in p.questData.curQuests)
            {
                if (quest.questID == RequiredQuest.ID) { return true; }
            }
            return false;
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