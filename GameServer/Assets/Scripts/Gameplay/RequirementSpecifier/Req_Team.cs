using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Team : Content_Requirement
    {
        public EContentOperator Operator;
        public int RequiredSize;

        public override bool isMet(PlayerCharacter p)
        {
            int teamSize;

            if (p.Team == null)
            {
                teamSize = 1;
            }
            else
            {
                teamSize = p.Team.memberCount;
            }

            return SBOperator.Operate(teamSize, Operator, RequiredSize);
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }
    }
}