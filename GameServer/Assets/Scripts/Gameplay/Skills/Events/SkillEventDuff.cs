using System.Collections.Generic;
using Common;
using Gameplay.Entities;

namespace Gameplay.Skills.Events
{
    public class SkillEventDuff : SkillEventFX
    {
        [ReadOnly] public List<SkillEventDuffCondEv> ConditionalEvents = new List<SkillEventDuffCondEv>();

        [ReadOnly] public string Description;

        [ReadOnly] public List<SkillEventDuffDirectEff> DirectEffects = new List<SkillEventDuffDirectEff>();

        [ReadOnly] public List<SkillEventDuffDuffEff> DuffEffects = new List<SkillEventDuffDuffEff>();

        [ReadOnly] public float Duration;

        [ReadOnly] public SkillEvent Event;

        [ReadOnly] public float EventInterval;

        [ReadOnly] public int EventRepeatCount;

        //var transient array<DirectEffectRunData> DirectEffectTimers;
        //var transient array<DuffEffectRunData> DuffEffectTimers;
        //var transient array<ConditionalEventRunData> ConditionalEventTimers;
        //var transient float EventTimer;
        //var transient int EventActualRepeatCount;
        //var transient bool DuffEventDone;
        //var transient bool UninstallWhenDone;
        //var transient array<Game_Pawn> Targets;

        bool fired;

        [ReadOnly] public string Name;

        [ReadOnly] public EDuffPriority Priority;

        [ReadOnly] public bool RunUntilAbort;

        [ReadOnly] public int StackCount;

        [ReadOnly] public EStackType StackType;

        [ReadOnly] public bool Visible;

        public override void Execute(SkillContext sInfo, Character triggerPawn)
        {
            base.Execute(sInfo, triggerPawn);
            if (!fired)
            {
                triggerPawn.AddDuff(this, Duration, StackType, StackCount, Visible);
                fired = true;
            }
        }

        public bool Apply(SkillContext sInfo, Character triggerPawn)
        {
            if (triggerPawn != null)
            {
                triggerPawn.AddDuff(this, Duration, StackType, StackCount, Visible);
                return true;
            }
            return false;
        }

        public override void DeepClone()
        {
            base.DeepClone();
            for (var i = 0; i < DirectEffects.Count; i++)
            {
                DirectEffects[i] = Instantiate(DirectEffects[i]);
                DirectEffects[i].DeepClone();
            }
            for (var i = 0; i < DuffEffects.Count; i++)
            {
                DuffEffects[i] = Instantiate(DuffEffects[i]);
                DuffEffects[i].DeepClone();
            }
            if (Event != null)
            {
                Event = Instantiate(Event);
                Event.DeepClone();
            }
            for (var i = 0; i < ConditionalEvents.Count; i++)
            {
                ConditionalEvents[i] = Instantiate(ConditionalEvents[i]);
                ConditionalEvents[i].DeepClone();
            }
        }

        public override void Reset()
        {
            base.Reset();
            fired = false;
            for (var i = 0; i < DirectEffects.Count; i++)
            {
                DirectEffects[i].Reset();
            }
            for (var i = 0; i < DuffEffects.Count; i++)
            {
                DuffEffects[i].Reset();
            }
            for (var i = 0; i < ConditionalEvents.Count; i++)
            {
                ConditionalEvents[i].Reset();
            }
            if (Event != null)
            {
                Event.Reset();
            }
        }
    }
}