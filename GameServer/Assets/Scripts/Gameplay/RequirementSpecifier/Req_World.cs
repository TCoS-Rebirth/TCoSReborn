using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_World : Content_Requirement
    {
        public int RequiredWorld;

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
            //TODO: check RequiredWorld corresponds to MapIDs enum
            if (c.LastZoneID == (MapIDs) RequiredWorld)
                return true;
            return false;
        }
    }
}