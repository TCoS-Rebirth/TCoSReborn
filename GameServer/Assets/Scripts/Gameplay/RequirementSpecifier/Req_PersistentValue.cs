using System;
using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_PersistentValue : Content_Requirement
    {
        public string context; //Content_Type
        public EContentOperator Operator;
        public int Value;
        public int VariableID;

        public override bool isMet(PlayerCharacter p)
        {
            return isMet();
        }

        public override bool isMet(NpcCharacter n)
        {
            return isMet();
        }

        public bool isMet()
        {
            throw new NotImplementedException();
        }
    }
}