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
            if (SBOperator.Operate(c.Stats.GetPePRank(), Operator, RequiredPep))
                return true;
            return false;
        }

        public override bool CheckPawn(Character character)
        {
            var p = character as PlayerCharacter;
            if (!p) return false;
            return isMet(p);
        }
    }
}