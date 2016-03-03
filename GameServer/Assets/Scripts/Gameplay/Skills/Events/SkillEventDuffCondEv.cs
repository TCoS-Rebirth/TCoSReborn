using Common;
using UnityEngine;

namespace Gameplay.Skills.Events
{
    public class SkillEventDuffCondEv : ScriptableObject
    {
        [ReadOnly] public EDuffAttackType AttackType;

        [ReadOnly] public EDuffCondition Condition;

        [ReadOnly] public float Delay;

        [ReadOnly] public EEffectType EffectType;

        [ReadOnly] public SkillEvent Event;

        [ReadOnly] public float IncreasePerUse;

        [ReadOnly] public string internalName;

        [ReadOnly] public float Interval;

        [ReadOnly] public EDuffMagicType MagicType;

        [ReadOnly] public int MaximumTriggersPerUse;

        [ReadOnly] public int resourceID;

        [ReadOnly] public ESkillTarget Target;

        [ReadOnly] public int Uses;

        public void Execute(SkillContext sInfo)
        {
            //needs!
        }

        public void DeepClone()
        {
            if (Event != null)
            {
                Event = Instantiate(Event);
                Event.DeepClone();
            }
        }

        public void Reset()
        {
            if (Event != null)
            {
                Event.Reset();
            }
        }
    }
}