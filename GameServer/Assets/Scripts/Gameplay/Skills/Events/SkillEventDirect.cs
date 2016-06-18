using System.Collections.Generic;
using Common;
using Gameplay.Entities;
using Gameplay.Skills.Effects;
using UnityEngine;

namespace Gameplay.Skills.Events
{
    public class SkillEventDirect : SkillEventTarget
    {
        [ReadOnly] public SkillEffectDirectState _State;

        [ReadOnly] public SkillEventDuff Buff;

        int currentRepeat;

        [ReadOnly] public SkillEffectDirectDamage Damage;

        [ReadOnly] public float DamageMoraleBonus;

        [ReadOnly] public SkillEventDuff Debuff;

        [ReadOnly] public SkillEffectDirectDrain Drain;

        [ReadOnly] public SkillEffectDirectFireBodySlot FireBodySlot;

        bool firstExecute = true;

        [ReadOnly] public SkillEffectDirectHeal Heal;

        [ReadOnly] public SkillEventFX HitFXEvent;

        [ReadOnly] public float Interval;

        [ReadOnly] public bool KeepTargets;

        float lastRepeatTime;

        [ReadOnly] public SkillEventFX MissFXEvent;

        [ReadOnly] public bool PlayHurtSound;

        [ReadOnly] public SkillEffectRange Range;

        [ReadOnly] public int RepeatCount;

        [ReadOnly] public bool RepeatTargetFX;

        [ReadOnly] public SkillEffectDirectShapeShift ShapeShift;

        //var private transient bool LeaveTargetsBe;
        //var private transient int ActualRepeatCount;
        //var private transient int ActualTargetCount;
        //var private transient float NextDirectEventTime;
        //var transient array<Game_Pawn> Targets;
        //var private transient bool FirstExecute;
        //var private transient Vector DetachedRangePosition;

        protected List<Character> targets = new List<Character>();

        [ReadOnly] public int TargetsPerRepeat;

        [ReadOnly] public SkillEffectDirectTeleport Teleport;

        public override bool Execute(RunningSkillContext context)
        {
            base.Execute(context);
            if (!HasDelayPassed(context)) return false;
            if (firstExecute)
            {
                //Debug.Log(string.Format("[{0}] - executing gatherTargets in {1} of {2}", Time.time, this, context.ExecutingSkill));
                GatherTargets(context);
                firstExecute = false;
            }
            if (currentRepeat > RepeatCount)
            {
                //Debug.Log(string.Format("[{0}] - event finished: {1} of {2}", Time.time, this, context.ExecutingSkill));
                return true;
            }
            if (context.GetCurrentSkillTime() - lastRepeatTime < Interval) return false;
            //Debug.Log(string.Format("[{0}] - executing DirectEffects in {1} of {2}", Time.time, this, context.ExecutingSkill));
            ExecuteDirectEffects(context);
            currentRepeat++;
            lastRepeatTime = context.GetCurrentSkillTime();
            return false;
        }

        protected void ExecuteDirectEffects(RunningSkillContext sInfo)
        {
            var success = false;
            for (var i = 0; i < targets.Count /*Mathf.Min(targets.Count, TargetsPerRepeat)*/; i++) //probably not correct
            {
                if (_State != null)
                {
                    success = _State.Fire(sInfo, targets[i]);
                }
                if (Damage != null)
                {
                    success = Damage.Fire(sInfo, targets[i]);
                }
                if (Heal != null)
                {
                    success = Heal.Fire(sInfo, targets[i]);
                }
                if (Buff != null)
                {
                    success = Buff.Apply(sInfo, targets[i]);
                }
                if (Debuff != null)
                {
                    success = Debuff.Apply(sInfo, targets[i]);
                }
                if (success)
                {
                    OnHitTarget(sInfo, targets[i]);
                }
            }
            if (!success)
            {
                OnMissedTarget(sInfo, sInfo.SkillPawn);
            }
        }

        protected virtual void OnHitTarget(RunningSkillContext sInfo, Character target)
        {
            if (HitFXEvent != null)
            {
                //Debug.Log(string.Format("[{0}] - executing OnHitTarget (hitfxevent) in {1} of {2}", Time.time, this, sInfo.ExecutingSkill));
                HitFXEvent.Execute(sInfo);
            }
        }

        protected virtual void OnMissedTarget(RunningSkillContext sInfo, Character target)
        {
            if (MissFXEvent != null)
            {
                //Debug.Log(string.Format("[{0}] - executing OnMissedTarget (missfxevent) in {1} of {2}", Time.time, this, sInfo.ExecutingSkill));
                MissFXEvent.Execute(sInfo);
            }
        }

        protected void GatherTargets(RunningSkillContext sInfo)
        {
            //Debug.Log(string.Format("[{0}] - gathering targets in {1} of {2}", Time.time, this, sInfo.ExecutingSkill));
            targets.Clear();
            if (TargetSelf != ETargetMode.ETM_Never)
            {
                targets.Add(sInfo.SkillPawn);
                return;
            }
            if (TargetFriendlies != ETargetMode.ETM_Never)
            {
                if (Range != null)
                {
                    var potentialTargets = sInfo.SkillPawn.QueryMeleeSkillTargets(Range);
                    for (var i = 0; i < potentialTargets.Count; i++)
                    {
                        if (sInfo.SkillPawn.Faction.Likes(potentialTargets[i].Faction))
                        {
                            targets.Add(potentialTargets[i]);
                        }
                    }
                }
                else
                {
                    Debug.Log("Range null, cant query targets (?)");
                }
                return;
            }
            if (TargetEnemies != ETargetMode.ETM_Never)
            {
                targets.Clear(); //TODO check if this conflicts
                if (Range != null)
                {
                    var potentialTargets = sInfo.ExecutingSkill.paintLocation ? sInfo.SkillPawn.QueryRangedSkillTargets(sInfo.TargetPosition, Range) : sInfo.SkillPawn.QueryMeleeSkillTargets(Range);
                    for (var i = 0; i < potentialTargets.Count; i++)
                    {
                        //if (sInfo.SkillPawn.Faction.Hates(potentialTargets[i].Faction)) //disabled for testing
                        //{
                            targets.Add(potentialTargets[i]);
                        //}
                    }
                }
                else
                {
                    Debug.Log("Range null, cant query targets (?)");
                }
            }
            //Debug.Log(string.Format("[{0}] - targets found: {1} in {2} of {3}", Time.time, targets.Count, this, sInfo.ExecutingSkill));
        }

        public override void DeepClone()
        {
            base.DeepClone();
            if (Buff != null)
            {
                Buff = Instantiate(Buff);
                Buff.DeepClone();
            }
            if (Debuff != null)
            {
                Debuff = Instantiate(Debuff);
                Debuff.DeepClone();
            }
            if (MissFXEvent != null)
            {
                MissFXEvent = Instantiate(MissFXEvent);
                MissFXEvent.DeepClone();
            }
            if (HitFXEvent != null)
            {
                HitFXEvent = Instantiate(HitFXEvent);
                HitFXEvent.DeepClone();
            }
        }

        public override void Reset()
        {
            base.Reset();
            targets.Clear();
            firstExecute = true;
            lastRepeatTime = 0;
            currentRepeat = 0;
            if (Buff != null)
            {
                Buff.Reset();
            }
            if (Debuff != null)
            {
                Debuff.Reset();
            }
            if (MissFXEvent != null)
            {
                MissFXEvent.Reset();
            }
            if (HitFXEvent != null)
            {
                HitFXEvent.Reset();
            }
        }
    }
}