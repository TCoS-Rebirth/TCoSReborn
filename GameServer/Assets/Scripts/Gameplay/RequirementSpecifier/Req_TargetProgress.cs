using System;
using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_TargetProgress : Content_Requirement
    {
        public int objective;
        public EContentOperator Operator;
        public int Progress;
        public string quest; //Quest_Type

        public override bool isMet(PlayerCharacter p)
        {
            throw new NotImplementedException();
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }
    }
}