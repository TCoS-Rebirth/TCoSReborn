using System;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_NPC : Content_Requirement
    {
        public override bool isMet(PlayerCharacter p)
        {
            return false;
        }

        public override bool isMet(NpcCharacter n)
        {
            return true;
        }

        public override bool CheckPawn(Character character)
        {
            var p = character as NpcCharacter;
            if (p != null) return true;
            return false;
        }
    }
}