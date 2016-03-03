using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffDirectionalDamage : SkillEffectDuff
    {
        [ReadOnly] public EDirectionDamageMode mode;

        [ReadOnly] public ValueSpecifier value;
    }
}