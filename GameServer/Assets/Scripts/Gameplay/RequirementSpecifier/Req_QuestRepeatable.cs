using System;
using Gameplay.Entities;
using Database.Static;

namespace Gameplay.RequirementSpecifier
{
    public class Req_QuestRepeatable : Content_Requirement
    {
        public SBResource Quest; //Quest_Type
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

        public override bool CheckPawn(Character character)
        {
            throw new NotImplementedException();
        }
    }
}