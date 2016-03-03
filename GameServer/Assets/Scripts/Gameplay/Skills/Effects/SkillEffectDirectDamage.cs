using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDirectDamage : SkillEffectDirect
    {
        [ReadOnly] public float aggroMultiplier;

        [ReadOnly] public ValueSpecifier damage;

        [ReadOnly] public bool ignoreResist;

        [ReadOnly] public Vector3 momentum;

        [ReadOnly] public float rearIncrease;

        public override bool Fire(SkillContext sInfo, Character target)
        {
            if (target != null)
            {
                var result = target.Damage(sInfo.Caster, sInfo.ExecutingSkill, (int) damage.CalculateValue(sInfo));
                sInfo.Caster.OnDamageCaused(result);
                return true;
            }
            return false;
        }
    }
}