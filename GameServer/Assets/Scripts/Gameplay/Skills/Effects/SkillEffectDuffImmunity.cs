using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffImmunity : SkillEffectDuff
    {
        [ReadOnly] public EDuffAttackType byAttackType;

        [ReadOnly] public EEffectType byEffectType;

        [ReadOnly] public EDuffMagicType byMagicType;

        [ReadOnly] public ValueSpecifier immunityValue;

        [ReadOnly] public AudioVisualSkillEffect sourceFX;

        [ReadOnly] public string temporarySourceFXName;
    }
}