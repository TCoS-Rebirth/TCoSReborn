using System;
using Gameplay.Entities;
using Database.Static;

namespace Gameplay.RequirementSpecifier
{
    public class Req_QuestReq : Content_Requirement
    {
        public SBResource quest; //Quest_Type

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