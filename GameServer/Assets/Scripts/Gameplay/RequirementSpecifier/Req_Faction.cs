using Gameplay.Entities;

namespace Gameplay.RequirementSpecifier
{
    public class Req_Faction : Content_Requirement
    {
        public Taxonomy RequiredTaxonomy;
        public int taxonomyID;
        public string temporaryTaxonomyName;

        public override bool isMet(PlayerCharacter p)
        {
            if (p.Faction.ID == taxonomyID)
            {
                return true;
            }
            return false;
        }


        public override bool isMet(NpcCharacter n)
        {
            if (n.Type.TaxonomyFaction.ID == taxonomyID)
            {
                return true;
            }
            return false;
        }

        public override bool CheckPawn(Character character)
        {
            var p = character as PlayerCharacter;
            if (p != null) return isMet(p);
            var n = character as NpcCharacter;
            if (n != null) return isMet(n);
            return false;
        }
    }
}