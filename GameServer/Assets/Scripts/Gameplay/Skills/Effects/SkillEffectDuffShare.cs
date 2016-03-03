using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDuffShare : SkillEffectDuff
    {
        [ReadOnly] public EDuffAttackType attackType;

        [ReadOnly] public string description;

        [ReadOnly] public EEffectType effectType;

        [ReadOnly] public float increasePerUse;

        [ReadOnly] public bool isBloodLink;

        [ReadOnly] public EDuffMagicType magicType;

        [ReadOnly] public EShareMode mode;

        [ReadOnly] public float shareRatio;

        [ReadOnly] public ValueSpecifier shareValue;

        [ReadOnly] public AudioVisualSkillEffect sourceFX;

        [ReadOnly] public AudioVisualSkillEffect targetFX;

        [ReadOnly] public string temporarySourceFXName;

        [ReadOnly] public string temporaryTargetFXName;

        [ReadOnly] public EShareType type;

        [ReadOnly] public float useInterval;

        [ReadOnly] public int uses;
    }
}