using Common;
using Gameplay.Entities;
using Gameplay.Items;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Equipment : Content_Requirement
    {
        public Item_Type Equipment;
        public int equipmentID;
        public string temporaryEquipmentName;

        public override bool isMet(PlayerCharacter p)
        {
            foreach (var it in p.Items.GetItems(EItemLocationType.ILT_Equipment))
            {
                if (it.Type.resourceID == equipmentID)
                {
                    return true;
                }
            }

            return false;
        }

        public override bool isMet(NpcCharacter n)
        {
            return false;
        }

        public override bool CheckPawn(Character character)
        {
            var p = character as PlayerCharacter;
            if (p != null) return isMet(p);
            return false;
        }
    }
}