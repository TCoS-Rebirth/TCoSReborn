using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_False : Content_Requirement
    {
        public override bool isMet(PlayerCharacter p)
        {
            return false;
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }
    }
}