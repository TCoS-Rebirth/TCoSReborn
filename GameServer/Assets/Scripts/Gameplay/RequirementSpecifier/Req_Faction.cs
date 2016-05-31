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
            if (n.typeRef.TaxonomyFaction.ID == taxonomyID)
            {
                return true;
            }
            return false;
        }
    }
}