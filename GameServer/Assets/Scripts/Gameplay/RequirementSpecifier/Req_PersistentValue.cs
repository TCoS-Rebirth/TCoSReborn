using System;
using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_PersistentValue : Content_Requirement
    {
        public int context; //Content_Type
        public EContentOperator Operator;
        public int Value;
        public int VariableID;

        public override bool isMet(PlayerCharacter p)
        {
            var perVar = p.persistentVars.GetValue((int)p.ActiveZone.ID, VariableID);
            return SBOperator.Operate(perVar, Operator, Value);
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }
    }
}