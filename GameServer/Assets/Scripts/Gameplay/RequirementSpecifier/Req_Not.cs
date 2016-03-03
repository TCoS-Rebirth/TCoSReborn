using System;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    [Serializable]
    public class Req_Not : Content_Requirement
    {
        public Content_Requirement Requirement;

        public override bool isMet(PlayerCharacter p)
        {
            return !Requirement.isMet(p);
        }

        public override bool isMet(NpcCharacter n)
        {
            return !Requirement.isMet(n);
        }
    }
}