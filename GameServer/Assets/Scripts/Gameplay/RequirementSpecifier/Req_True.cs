using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_True : Content_Requirement
    {
        public override bool isMet(PlayerCharacter p)
        {
            return true;
        }

        public override bool isMet(NpcCharacter n)
        {
            return true;
        }

        public override bool CheckPawn(Character character)
        {
            return true;
        }
    }
}