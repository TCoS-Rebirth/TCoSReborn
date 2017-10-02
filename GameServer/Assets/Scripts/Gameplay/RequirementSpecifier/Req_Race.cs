using System;
using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Race : Content_Requirement
    {
        public NPCRace RequiredRace;

        public override bool isMet(PlayerCharacter p)
        {
            //TODO: confirm p.appearance.Race corresponds to NPCRace enum
            if (RequiredRace == (NPCRace) p.Appearance.Race)
                return true;
            return false;
        }

        public override bool isMet(NpcCharacter n)
        {
            throw new NotImplementedException();
        }

        public override bool CheckPawn(Character character)
        {
            var p = character as PlayerCharacter;
            if (!p) return false;
            return isMet(p);
        }
    }
}