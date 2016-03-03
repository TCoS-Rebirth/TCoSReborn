using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_PePRank : Content_Requirement
    {
        public EContentOperator Operator;
        public int RequiredPep;

        public override bool isMet(PlayerCharacter p)
        {
            return isMet(p);
        }

        public override bool isMet(NpcCharacter n)
        {
            return isMet(n);
        }

        public bool isMet(Character c)
        {
            if (SBOperator.Operate(c.PepRank, Operator, RequiredPep))
                return true;
            return false;
        }
    }
}