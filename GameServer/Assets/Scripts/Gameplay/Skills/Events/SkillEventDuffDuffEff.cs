using Gameplay.Skills.Effects;
using UnityEngine;

namespace Gameplay.Skills.Events
{
    public class SkillEventDuffDuffEff : ScriptableObject
    {
        [ReadOnly] public float Delay;

        [ReadOnly] public SkillEffectDuff effect;

        [ReadOnly] public SkillEventFX ExecuteFXEvent;

        [ReadOnly] public string internalName;

        [ReadOnly] public float Interval;

        [ReadOnly] public int RepeatCount;

        [ReadOnly] public int resourceID;

        public void Execute(RunningSkillContext sInfo)
        {
            //needs!
        }

        public void DeepClone()
        {
            if (ExecuteFXEvent != null)
            {
                ExecuteFXEvent = Instantiate(ExecuteFXEvent);
                ExecuteFXEvent.DeepClone();
            }
        }

        public void Reset()
        {
            if (ExecuteFXEvent != null)
            {
                ExecuteFXEvent.Reset();
            }
        }
    }
}