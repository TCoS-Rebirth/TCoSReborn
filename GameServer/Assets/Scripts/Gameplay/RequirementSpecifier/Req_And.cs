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

        public override bool CheckPawn(Character character)
        {
            var p = character as PlayerCharacter;
            if (p != null) return isMet(p);
            var n = character as NpcCharacter;
            if (n != null) return isMet(n);
            return false;
        }
    }
}