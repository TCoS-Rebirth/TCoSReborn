using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDirectHeal : SkillEffectDirect
    {
        [ReadOnly] public float aggroMultiplier = 1;

        [ReadOnly] public ValueSpecifier heal;

        public override bool Fire(SkillContext sInfo, Character target)
        {
            if (target != null)
            {
                var result = target.Heal(sInfo.Caster, sInfo.ExecutingSkill, Mathf.Abs((int) heal.CalculateValue(sInfo)));
                sInfo.Caster.OnHealingCaused(result);
                return true;
            }
            return false;
        }
    }
}