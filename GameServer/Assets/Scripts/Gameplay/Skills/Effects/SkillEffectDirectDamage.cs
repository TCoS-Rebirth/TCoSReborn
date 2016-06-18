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

        public override bool Fire(RunningSkillContext sInfo, Character target)
        {
            if (target != null)
            {
                target.Damage(sInfo.SkillPawn, sInfo.ExecutingSkill, (int) damage.CalculateValue(sInfo), sInfo.SkillPawn.OnDamageCaused);
                //sInfo.SkillPawn.OnDamageCaused(result);
                return true;
            }
            return false;
        }
    }
}