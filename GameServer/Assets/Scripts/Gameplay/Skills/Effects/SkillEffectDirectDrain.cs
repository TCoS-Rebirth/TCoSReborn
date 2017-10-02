using System;
using Common;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDirectDrain : SkillEffectDirect
    {
        [ReadOnly] public ValueSpecifier drainedAmount;

        [ReadOnly] public ECharacterStateHealthType drainedCharacterStat;

        [ReadOnly] public ECharacterStateHealthType gainedCharacterStat;

        [ReadOnly] public float multiplier = 1f;

        [ReadOnly] public ValueSpecifier multiplierVS;

        public override bool Apply(FSkill_Type skill, Character skillPawn, Character targetPawn)
        {
            if (targetPawn != null)
            {
                var drainValue = (int)drainedAmount.CalculateValue(skill, skillPawn, targetPawn);
                //if (multiplierVS != null) //could this be for gain only?
                //{
                //    drainValue = (int)(drainValue * multiplierVS.CalculateValue(skill, skillPawn, targetPawn));
                //}
                //else
                //{
                //    drainValue = (int)(drainValue * multiplier);
                //}
                switch (drainedCharacterStat)
                {
                    case ECharacterStateHealthType.ECSTH_Physique:
                        targetPawn.Stats.IncreasePhysique(drainValue);
                        break;
                    case ECharacterStateHealthType.ECSTH_Morale:
                        targetPawn.Stats.IncreaseMorale(drainValue);
                        break;
                    case ECharacterStateHealthType.ECSTH_Concentration:
                        targetPawn.Stats.IncreaseConcentration(drainValue);
                        break;
                    case ECharacterStateHealthType.ECSTH_Health:
                        targetPawn.Stats.SetHealth(targetPawn.Stats.mRecord.CopyHealth + drainValue);
                        break;
                }
                Debug.Log("TODO: gain character stat from draining");
                return true;
            }
            return false;
        }
    }
}