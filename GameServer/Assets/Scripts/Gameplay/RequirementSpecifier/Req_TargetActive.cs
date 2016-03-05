using System;
using Gameplay.Entities;
using Database.Static;

namespace Gameplay.RequirementSpecifier
{
    public class Req_TargetActive : Content_Requirement
    {        
        public SBResource quest; //Quest_Type
        public int objective;

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
            //TODO: Implement Req_TargetActive
            throw new NotImplementedException();
        }
    }
}