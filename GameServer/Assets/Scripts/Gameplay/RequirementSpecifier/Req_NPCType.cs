using System;
using Gameplay.Entities;
using Gameplay.Entities.NPCs;

namespace Gameplay.RequirementSpecifier
{
    public class Req_NPCType : Content_Requirement
    {
        public int npcID;
        public NPC_Type RequiredNPCType;
        public string temporaryNPCTypeName;

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
            //TODO: Implement Req_NPC_Type
            throw new NotImplementedException();
        }

        public override bool CheckPawn(Character character)
        {
            throw new NotImplementedException();
        }
    }
}