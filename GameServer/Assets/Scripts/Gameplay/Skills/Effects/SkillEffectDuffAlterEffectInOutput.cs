using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffAlterEffectInOutput : SkillEffectDuff
    {
        [ReadOnly] public ValueSpecifier alterEffectValue;

        [ReadOnly] public EDuffAttackType attackType;

        [ReadOnly] public EEffectType effectType;

        [ReadOnly] public bool ignoreMultiplier;

        [ReadOnly] public float increasePerUse;

        [ReadOnly] public EDuffMagicType magicType;

        [ReadOnly] public EAlterEffectMode mode;

        [ReadOnly] public float useInterval;

        [ReadOnly] public int uses;

        [ReadOnly] public ValueSpecifier value;

        [ReadOnly] public EValueMode valueMode;
    }
}