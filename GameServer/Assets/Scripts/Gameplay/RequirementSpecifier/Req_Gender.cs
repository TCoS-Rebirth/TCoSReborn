using System;
using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Gender : Content_Requirement
    {
        public NPCGender Gender;

        public override bool isMet(PlayerCharacter p)
        {
            return p.Appearance.GetGender() == Gender;
        }

        public override bool isMet(NpcCharacter n)
        {
            throw new NotImplementedException();
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