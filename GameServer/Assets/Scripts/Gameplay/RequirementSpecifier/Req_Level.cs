using System;
using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    [Serializable]
    public class Req_Level : Content_Requirement
    {
        public EContentOperator Operator;
        public int RequiredLevel;

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
            return SBOperator.Operate(c.Stats.FameLevel, Operator, RequiredLevel);
        }

        public override bool CheckPawn(Character character)
        {
            return isMet(character);
        }
    }
}