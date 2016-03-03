using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffDrain : SkillEffectDuff
    {
        [ReadOnly] public ValueSpecifier drainedAmount;

        [ReadOnly] public ECharacterAttributeType drainedCharacterStat;

        [ReadOnly] public ECharacterAttributeType gainedCharacterStat;

        [ReadOnly] public float multiplier;

        [ReadOnly] public ValueSpecifier multiplierVS;
    }
}