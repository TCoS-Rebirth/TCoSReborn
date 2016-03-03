using Common;

namespace Gameplay.Skills.Effects
{
    public class SkillEffectDirectTeleport : SkillEffectDirect
    {
        const int MAX_TELEPORT_RETRIES = 4;

        [ReadOnly] public ETeleportMode mode;

        [ReadOnly] public float offset;

        [ReadOnly] public ERotationMode rotation;

        [ReadOnly] public ValueSpecifier teleportValue;
    }
}