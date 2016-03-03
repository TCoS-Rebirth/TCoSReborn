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

        public override bool Fire(SkillContext sInfo, Character target)
        {
            if (target != null)
            {
                var drainedValue = (int) drainedAmount.CalculateValue(sInfo);
                var realDrainedAmount = target.SetCharacterStat(drainedCharacterStat, target.GetCharacterStat(drainedCharacterStat) + drainedValue);
                if (multiplierVS != null)
                {
                    realDrainedAmount = (int) (realDrainedAmount*multiplierVS.CalculateValue(sInfo));
                }
                else
                {
                    realDrainedAmount = (int) (realDrainedAmount*multiplier);
                }
                target.SetCharacterStat(gainedCharacterStat, realDrainedAmount);
                return true;
            }
            return false;
        }
    }
}