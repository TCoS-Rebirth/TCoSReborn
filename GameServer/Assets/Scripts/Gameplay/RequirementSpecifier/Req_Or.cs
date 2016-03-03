using System.Collections.Generic;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Or : Content_Requirement
    {
        public List<Content_Requirement> Requirements = new List<Content_Requirement>();

        public override bool isMet(PlayerCharacter p)
        {
            foreach (var req in Requirements)
            {
                if (req.isMet(p))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool isMet(NpcCharacter n)
        {
            foreach (var req in Requirements)
            {
                if (req.isMet(n))
                {
                    return true;
                }
            }
            return false;
        }
    }
}