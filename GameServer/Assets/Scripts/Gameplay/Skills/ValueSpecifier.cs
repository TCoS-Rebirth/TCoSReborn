using System;
using System.Collections.Generic;
using Common;
using Gameplay.Entities;
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

        public float CalculateValue(FSkill_Type skill, Character skillPawn, Character targetPawn)
        {
            //TODO calculate correct values
            if (!Mathf.Approximately(constantMaximum, 0))
            {
                var val = Random.Range(constantMinimum, constantMaximum);
                val = val + val*linkedAttributeModifier*GetSourceStatisticValue(skillPawn, targetPawn);
                return val;
            }
            else
            {
                var val = Random.Range(absoluteMinimum, absoluteMaximum);
                val = val + val*linkedAttributeModifier*GetSourceStatisticValue(skillPawn, targetPawn);
                return val;
            }
        }

        float GetSourceStatisticValue(Character skillPawn, Character targetPawn)
        {
            switch (source)
            {
                case EVSSource.EVSS_TriggerPawn:
                    return skillPawn.Stats.GetCharacterStatistic(characterStatistic);
                case EVSSource.EVSS_TargetPawn:
                    return targetPawn != null ? targetPawn.Stats.GetCharacterStatistic(characterStatistic) : 0;
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