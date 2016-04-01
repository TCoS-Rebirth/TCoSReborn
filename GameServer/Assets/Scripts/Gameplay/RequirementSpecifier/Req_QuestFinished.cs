using System;
using Gameplay.Entities;
using Database.Static;

namespace Gameplay.RequirementSpecifier
{
    public class Req_QuestFinished : Content_Requirement
    {
        public SBResource RequiredQuest; //Quest_Type
        public int TimesFinished;

        public override bool isMet(PlayerCharacter p)
        {
            foreach (var questID in p.questData.completedQuestIDs)
            {
                if (questID == RequiredQuest.ID) { return true; }
            }

            return false;
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }
    }
}