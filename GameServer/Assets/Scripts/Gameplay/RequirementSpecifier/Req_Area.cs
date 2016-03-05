using System;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Area : Content_Requirement
    {
        public string AreaTag;

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
            //TODO: Implement Req_Area
            throw new NotImplementedException();
        }
    }
}