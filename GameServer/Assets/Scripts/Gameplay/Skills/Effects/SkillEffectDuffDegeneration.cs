using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffDegeneration : SkillEffectDuff
    {
        [ReadOnly] public ECharacterStateHealthType state;

        [ReadOnly] public ValueSpecifier value;
    }
}