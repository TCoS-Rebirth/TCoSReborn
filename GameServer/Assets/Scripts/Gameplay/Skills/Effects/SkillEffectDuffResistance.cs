using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffResistance : SkillEffectDuff
    {
        [ReadOnly] public EDuffAttackType attackType;

        [ReadOnly] public ValueSpecifier value;
    }
}