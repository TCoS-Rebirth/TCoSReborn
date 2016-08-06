using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Player : Content_Requirement
    {
        public override bool isMet(PlayerCharacter p)
        {
            return true;
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }

        public override bool CheckPawn(Character character)
        {
            var p = character as PlayerCharacter;
            if (!p) return false;
            return isMet(p);
        }
    }
}