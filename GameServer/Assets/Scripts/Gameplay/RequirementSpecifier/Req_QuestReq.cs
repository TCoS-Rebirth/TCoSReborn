using System;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_QuestReq : Content_Requirement
    {
        public string quest; //Quest_Type

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
            //TODO: Implement Req_NPC_Exists
            throw new NotImplementedException();
        }
    }
}