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

        public override bool Apply(FSkill_Type skill, Character skillPawn, Character targetPawn)
        {
            if (targetPawn != null)
            {
                targetPawn.Damage(skillPawn, skill, (int) damage.CalculateValue(skill, skillPawn, targetPawn), skillPawn.OnDamageCaused);
                return true;
            }
            return false;
        }
    }
}