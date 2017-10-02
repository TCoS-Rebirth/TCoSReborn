using System.Collections.Generic;
using Common;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills.Events
{
    public class FSkillEventDuff : FSkillEventFx
    {
        [ReadOnly] public List<SkillEventDuffCondEv> ConditionalEvents = new List<SkillEventDuffCondEv>();

        [ReadOnly] public string Description;

        [ReadOnly] public List<SkillEventDuffDirectEff> DirectEffects = new List<SkillEventDuffDirectEff>();

        [ReadOnly] public List<SkillEventDuffDuffEff> DuffEffects = new List<SkillEventDuffDuffEff>();

        [ReadOnly] public float Duration;

        [ReadOnly] public FSkill_Event Event;

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

        [ReadOnly] public string Name;

        [ReadOnly] public EDuffPriority Priority;

        [ReadOnly] public bool RunUntilAbort;

        [ReadOnly] public int StackCount;

        [ReadOnly] public EStackType StackType;

        [ReadOnly] public bool Visible;

        public void FireCondition(Character trigger, EDuffCondition condition, EDuffAttackType attackType, EDuffMagicType magicType = EDuffMagicType.EDMT_None,
            EEffectType effectType = EEffectType.EET_Damage)
        {
            for (var i = 0; i < ConditionalEvents.Count; i++)
            {
                var cEv = ConditionalEvents[i];
                if (cEv.Condition == condition
                    && cEv.AttackType == attackType
                    && cEv.MagicType == magicType
                    && cEv.EffectType == effectType
                    )
                {
                    trigger.Skills.FireCondition(trigger, condition, attackType, magicType, effectType);
                }
            }
        }

        public bool Apply(FSkill_Type skill, Character skillPawn, Character targetPawn)
        {
            targetPawn.AddDuff(this, Duration, StackType, StackCount, Visible); 
            return true;
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