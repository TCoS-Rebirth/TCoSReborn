using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffRegeneration : SkillEffectDuff
    {
        [ReadOnly] public ECharacterStateHealthType state;

        [ReadOnly] public ValueSpecifier value;
    }
}