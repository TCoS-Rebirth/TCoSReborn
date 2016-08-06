using Common;
using Gameplay.Entities;

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
                var drainedValue = (int) drainedAmount.CalculateValue(skill, skillPawn, targetPawn);
                var realDrainedAmount = targetPawn.Stats.SetCharacterStat(drainedCharacterStat, targetPawn.Stats.GetCharacterStat(drainedCharacterStat) + drainedValue);
                if (multiplierVS != null)
                {
                    realDrainedAmount = (int) (realDrainedAmount*multiplierVS.CalculateValue(skill, skillPawn, targetPawn));
                }
                else
                {
                    realDrainedAmount = (int) (realDrainedAmount*multiplier);
                }
                targetPawn.Stats.SetCharacterStat(gainedCharacterStat, realDrainedAmount);
                return true;
            }
            return false;
        }
    }
}