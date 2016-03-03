using System;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_NPC : Content_Requirement
    {
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
            //TODO: Implement Req_NPC
            throw new NotImplementedException();
        }
    }
}