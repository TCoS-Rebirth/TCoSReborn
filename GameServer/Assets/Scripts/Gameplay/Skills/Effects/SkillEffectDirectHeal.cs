using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDirectHeal : SkillEffectDirect
    {
        [ReadOnly] public float aggroMultiplier = 1;

        [ReadOnly] public ValueSpecifier heal;

        public override bool Fire(RunningSkillContext sInfo, Character target)
        {
            if (target != null)
            {
                var result = target.Heal(sInfo.SkillPawn, sInfo.ExecutingSkill, Mathf.Abs((int) heal.CalculateValue(sInfo)));
                sInfo.SkillPawn.OnHealingCaused(result);
                return true;
            }
            return false;
        }
    }
}