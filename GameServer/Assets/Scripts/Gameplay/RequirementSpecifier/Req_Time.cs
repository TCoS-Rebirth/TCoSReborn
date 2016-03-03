using System;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Time : Content_Requirement
    {
        public float BeginTime;
        public float EndTime;

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
            //TODO : Implement Req_Time logic
            throw new NotImplementedException();
        }
    }
}