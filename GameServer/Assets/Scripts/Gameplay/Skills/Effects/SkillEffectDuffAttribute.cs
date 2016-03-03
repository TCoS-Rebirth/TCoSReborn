using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffAttribute : SkillEffectDuff
    {
        [ReadOnly] public ECharacterAttributeType attribute;

        [ReadOnly] public ValueSpecifier value;
    }
}