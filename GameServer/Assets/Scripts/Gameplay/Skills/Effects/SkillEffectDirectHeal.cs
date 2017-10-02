using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDirectHeal : SkillEffectDirect
    {
        [ReadOnly] public float aggroMultiplier = 1;

        [ReadOnly] public ValueSpecifier heal;

        public override bool Apply(FSkill_Type skill, Character skillPawn, Character targetPawn)
        {
            if (targetPawn != null)
            {
                var result = targetPawn.Heal(skillPawn, skill, Mathf.Abs((int) heal.CalculateValue(skill, skillPawn, targetPawn)));
                skillPawn.OnHealingCaused(result);
                return true;
            }
            return false;
        }
    }
}