using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffDisableSkillUse : SkillEffectDuff
    {
        [ReadOnly] public EDuffAttackType byAttackType;

        [ReadOnly] public EDuffMagicType byMagicType;

        [ReadOnly] public ValueSpecifier disableSkillUseValue;
    }
}