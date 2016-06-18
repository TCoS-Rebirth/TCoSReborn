using System;
using System.Collections.Generic;
using Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Skills
{
    [Serializable]
    public class ValueSpecifier
    {
        public float absoluteMaximum;

        public float absoluteMinimum;
        public bool addCharStatRelated;
        public bool addComboLength;
        public bool addConstant;
        public bool addTargetCountRelated;
        public bool applyIncrease;
        public EVSCharacterStatistic characterStatistic;
        public float charStatMaximumMultiplier;
        public float charStatMinimumMultiplier;
        public float comboLengthMaximum;
        public float comboLengthMinimum;
        public float constantMaximum;
        public float constantMinimum;
        public bool divideValue;
        public bool ignoreFameModifier;
        public float linkedAttributeModifier;
        public float NPCIncrease;
        public float playerIncrease;
        public string referenceName;
        public EVSSource source;
        public float spiritIncrease;
        public float targetCountMaximumMultiplier;
        public float targetCountMinimumMultiplier;
        public List<TaxonomyIncrease> taxonomyIncreases = new List<TaxonomyIncrease>();

        public float CalculateValue(RunningSkillContext sInfo)
        {
            //TODO calculate correct values
            if (!Mathf.Approximately(constantMaximum, 0))
            {
                var val = Random.Range(constantMinimum, constantMaximum);
                val = val + val*linkedAttributeModifier*GetSourceStatisticValue(sInfo);
                return val;
            }
            else
            {
                var val = Random.Range(absoluteMinimum, absoluteMaximum);
                val = val + val*linkedAttributeModifier*GetSourceStatisticValue(sInfo);
                return val;
            }
        }

        float GetSourceStatisticValue(RunningSkillContext sInfo)
        {
            switch (source)
            {
                case EVSSource.EVSS_TriggerPawn:
                    return sInfo.SkillPawn.GetCharacterStatistic(characterStatistic);
                case EVSSource.EVSS_TargetPawn:
                    return sInfo.PreferedTarget != null ? sInfo.PreferedTarget.GetCharacterStatistic(characterStatistic) : 0;
                default:
                    return 0;
            }
        }

        [Serializable]
        public class TaxonomyIncrease
        {
            public float increase;
            public Taxonomy node;
            public string temporaryTaxonomyName;
        }
    }
}