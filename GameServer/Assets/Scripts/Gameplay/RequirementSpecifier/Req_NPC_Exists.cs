using System;
using Gameplay.Entities;
using Gameplay.Entities.NPCs;

namespace Gameplay.RequirementSpecifier
{
    public class Req_NPC_Exists : Content_Requirement
    {
        public bool MustBeAlive;
        public int npcID;
        public NPC_Type NPCType;
        public string temporaryNPCName;

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
            //TODO: Implement Req_NPC_Exists
            throw new NotImplementedException();
        }
    }
}