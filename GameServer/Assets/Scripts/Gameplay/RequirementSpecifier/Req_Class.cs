using Common;
using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Class : Content_Requirement
    {
        public EContentClass RequiredClass;

        public override bool isMet(PlayerCharacter p)
        {
            return isMet(p as Character);
           
        }

        public override bool isMet(NpcCharacter n)
        {
            return isMet(n as Character);
        }

        public bool isMet(Character c)
        {
            if (c.ArcheType == ClassArcheType.Rogue
                && RequiredClass == EContentClass.ECC_Rogue) return true;
            else if (c.ArcheType == ClassArcheType.Warrior
                    && RequiredClass == EContentClass.ECC_Warrior) return true;
            else if (c.ArcheType == ClassArcheType.Spellcaster
                    && RequiredClass == EContentClass.ECC_Spellcaster) return true;

            //TODO : Assign PlayerCharacters their proper class type (on login?)
            //Currently it is empty
            else return (c.ClassType == RequiredClass);
        }
    }
}