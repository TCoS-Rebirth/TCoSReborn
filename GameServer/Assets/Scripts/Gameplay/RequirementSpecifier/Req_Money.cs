using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Money : Content_Requirement
    {
        public EContentOperator Operator;
        public int RequiredAmount;

        public override bool isMet(PlayerCharacter p)
        {
            return SBOperator.Operate(p.Money, Operator, RequiredAmount);
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }

        public override bool CheckPawn(Character character)
        {
            var p = character as PlayerCharacter;
            if (p != null)
            {
                return isMet(p);
            }
            return false;
        }
    }
}