using System;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_QuestRepeatable : Content_Requirement
    {
        public string quest; //Quest_Type
        public bool Repeatable;

        public override bool isMet(PlayerCharacter p)
        {
            return isMet();
        }

        public override bool isMet(NpcCharacter n)
        {
            return isMet();
        }

        public bool isMet()
        {
            throw new NotImplementedException();
        }
    }
}