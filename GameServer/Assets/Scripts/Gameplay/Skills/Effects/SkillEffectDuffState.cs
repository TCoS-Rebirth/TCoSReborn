using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffState : SkillEffectDuff
    {
        [ReadOnly] public ECharacterAttributeType attribute;

        [ReadOnly] public ValueSpecifier value;
    }
}