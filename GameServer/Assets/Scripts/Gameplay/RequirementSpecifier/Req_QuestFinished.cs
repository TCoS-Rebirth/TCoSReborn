using System;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_QuestFinished : Content_Requirement
    {
        public string RequiredQuest; //Quest_Type
        public int TimesFinished;

        public override bool isMet(PlayerCharacter p)
        {
            throw new NotImplementedException();
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }
    }
}