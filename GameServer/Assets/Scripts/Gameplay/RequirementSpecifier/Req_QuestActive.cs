using System;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_QuestActive : Content_Requirement
    {
        public string RequiredQuest; //Quest_Type

        public override bool isMet(PlayerCharacter p)
        {
            //TODO : Implement Req_QuestActive
            throw new NotImplementedException();
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }
    }
}