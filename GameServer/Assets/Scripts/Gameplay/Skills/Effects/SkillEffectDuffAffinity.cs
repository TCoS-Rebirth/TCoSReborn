using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffAffinity : SkillEffectDuff
    {
        [ReadOnly] public EDuffMagicType magicType;

        [ReadOnly] public ValueSpecifier value;
    }
}