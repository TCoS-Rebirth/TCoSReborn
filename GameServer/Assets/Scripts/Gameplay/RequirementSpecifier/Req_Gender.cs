using System;
using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Gender : Content_Requirement
    {
        public NPCGender Gender;

        public override bool isMet(PlayerCharacter p)
        {
            switch (Gender)
            {
                case NPCGender.ENG_Male:
                    if (p.Appearance.Gender == CharacterGender.Male)
                    {
                        return true;
                    }
                    return false;

                case NPCGender.ENG_Female:
                    if (p.Appearance.Gender == CharacterGender.Female)
                    {
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

        public override bool isMet(NpcCharacter n)
        {
            throw new NotImplementedException();
        }
    }
}