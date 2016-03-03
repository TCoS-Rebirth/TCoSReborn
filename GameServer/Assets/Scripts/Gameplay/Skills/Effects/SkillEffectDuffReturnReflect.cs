using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffReturnReflect : SkillEffectDuff
    {
        [ReadOnly] public EDuffAttackType attackType;

        [ReadOnly] public EEffectType effectType;

        [ReadOnly] public float increasePerUse;

        [ReadOnly] public EDuffMagicType magicType;

        [ReadOnly] public EReturnReflectMode mode;

        [ReadOnly] public float multiplier;

        [ReadOnly] public ValueSpecifier returnReflectValue;

        [ReadOnly] public AudioVisualSkillEffect sourceFX;

        [ReadOnly] public AudioVisualSkillEffect targetFX;

        [ReadOnly] public string temporarySourceFXName;

        [ReadOnly] public string temporaryTargetFXName;

        [ReadOnly] public float useInterval;

        [ReadOnly] public int uses;
    }
}