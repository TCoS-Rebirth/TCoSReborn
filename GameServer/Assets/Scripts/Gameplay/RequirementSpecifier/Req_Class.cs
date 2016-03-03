using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Class : Content_Requirement
    {
        public EContentClass RequiredClass;

        public override bool isMet(PlayerCharacter p)
        {
            return isMet(p);
        }

        public override bool isMet(NpcCharacter n)
        {
            return isMet(n);
        }

        public bool isMet(Character c)
        {
            if (c.ClassType == RequiredClass)
                return true;
            return false;
        }
    }
}