using System.Collections.Generic;
using Common;

namespace Gameplay.Skills.Events
{
    public class FSkillEventTarget : FSkillEventFx
    {
        [ReadOnly] public List<Taxonomy> LimitToTaxonomy = new List<Taxonomy>();

        [ReadOnly] public int MaxTargets;

        [ReadOnly] public bool TargetAttached;

        [ReadOnly] public ETargetingBase TargetBase;

        [ReadOnly] public ETargetMode TargetBloodlinks;

        [ReadOnly] public ETargetMode TargetEnemies;

        [ReadOnly] public ETargetMode TargetFriendlies;

        [ReadOnly] public ETargetMode TargetGuildMembers;

        [ReadOnly] public ETargetMode TargetNeutrals;

        [ReadOnly] public ETargetMode TargetPartyMembers;

        [ReadOnly] public ETargetMode TargetPets;

        [ReadOnly] public ETargetMode TargetSelf;

        [ReadOnly] public ETargetMode TargetSpirits;

        [ReadOnly] public List<string> temporaryLimitToTaxonomy = new List<string>();
    }
}