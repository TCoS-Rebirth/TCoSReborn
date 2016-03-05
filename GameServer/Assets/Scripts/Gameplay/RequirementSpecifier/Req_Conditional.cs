using System;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Conditional : Content_Requirement
    {
        public Content_Requirement Condition;
        public Content_Requirement Requirement;

        public override bool isMet(PlayerCharacter p)
        {
            Debug.Log("Req_Conditional.isMet(PlayerCharacter p) : Check that this requirement functions as intended");
            if (Condition.isMet(p)) { return Requirement.isMet(p); }
            else { return false; }
        }

        public override bool isMet(NpcCharacter n)
        {
            Debug.Log("Req_Conditional.isMet(NpcCharacter n) : Check that this requirement functions as intended");
            if (Condition.isMet(n)) { return Requirement.isMet(n); }
            else { return false; }
        }
    }
}