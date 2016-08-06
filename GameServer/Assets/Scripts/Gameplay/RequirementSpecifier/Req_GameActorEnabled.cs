using System;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_GameActorEnabled : Content_Requirement
    {
        public bool AllMustSucceed;
        public bool CheckForEnabled;
        public string Tag;

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
            //TODO: Implement this requirement
            throw new NotImplementedException();
        }

        public override bool CheckPawn(Character character)
        {
            throw new NotImplementedException();
        }
    }
}