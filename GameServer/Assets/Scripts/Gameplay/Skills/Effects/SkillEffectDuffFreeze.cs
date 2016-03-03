namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffFreeze : SkillEffectDuff
    {
        [ReadOnly] public bool animation;

        [ReadOnly] public ValueSpecifier freezeValue;

        [ReadOnly] public bool movement;

        [ReadOnly] public bool rotation;
    }
}