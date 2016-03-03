using Common.UnrealTypes;
using UnityEngine;

namespace Gameplay.Skills.Effects
{
    public class AudioVisualSkillEffect : SkillEffect
    {
        [ReadOnly] public byte category;

        [ReadOnly] public float duration;

        [ReadOnly] public float extraScale;

        [ReadOnly] public float introDuration;

        [ReadOnly] public Vector3 location;

        [ReadOnly] public float outroDuration;

        [ReadOnly] public float pulseDuration;

        [ReadOnly] public Rotator rotation;

        [ReadOnly] public float runningDuration;

        [ReadOnly] public bool scaleWithBase;
    }
}