using System;
using System.Collections.Generic;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    [Serializable]
    public class Req_And : Content_Requirement
    {
        public List<Content_Requirement> Requirements = new List<Content_Requirement>();

        public override bool isMet(PlayerCharacter p)
        {
            foreach (var req in Requirements)
            {
                if (!req.isMet(p))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool isMet(NpcCharacter n)
        {
            foreach (var req in Requirements)
            {
                if (!req.isMet(n))
                {
                    return false;
                }
            }
            return true;
        }
    }
}