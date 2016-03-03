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

        public override void Execute(SkillContext sInfo, Character triggerPawn)
        {
            //needs!
            base.Execute(sInfo, triggerPawn);
            if (!sInfo.IsInCurrentTimeSpan(Delay))
            {
                return;
            }
            if (firstExecute)
            {
                GatherTargets(sInfo);
                firstExecute = false;
            }
            if (currentRepeat <= RepeatCount && sInfo.currentSkillTime - lastRepeatTime >= Interval)
            {
                ExecuteDirectEffects(sInfo, triggerPawn);
                currentRepeat++;
                lastRepeatTime = sInfo.currentSkillTime;
            }
        }

        void ExecuteDirectEffects(SkillContext sInfo, Character triggerPawn)
        {
            var success = false;
            for (var i = 0; i < Mathf.Min(targets.Count, TargetsPerRepeat); i++)
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
                OnMissedTarget(sInfo, sInfo.Caster);
            }
        }

        protected virtual void OnHitTarget(SkillContext sInfo, Character target)
        {
            if (HitFXEvent != null)
            {
                HitFXEvent.Execute(sInfo, target);
            }
        }

        protected virtual void OnMissedTarget(SkillContext sInfo, Character target)
        {
            if (MissFXEvent != null)
            {
                MissFXEvent.Execute(sInfo, target);
            }
        }

        void GatherTargets(SkillContext sInfo)
        {
            targets.Clear();
            if (TargetSelf != ETargetMode.ETM_Never)
            {
                targets.Add(sInfo.Caster);
                return;
            }
            if (TargetFriendlies != ETargetMode.ETM_Never)
            {
                if (Range != null)
                {
                    var potentialTargets = sInfo.Caster.QueryMeleeSkillTargets(Range);
                    for (var i = 0; i < potentialTargets.Count; i++)
                    {
                        if (sInfo.Caster.Faction.Likes(potentialTargets[i].Faction))
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
                    var potentialTargets = new List<Character>();
                    if (sInfo.ExecutingSkill.paintLocation)
                    {
                        //if (sInfo.Caster.Faction.Hates(potentialTargets[i].Faction)) //disabled for testing
                        //{
                        potentialTargets = sInfo.Caster.QueryRangedSkillTargets(sInfo.TargetPosition, Range);
                        //}
                    }
                    else
                    {
                        potentialTargets = sInfo.Caster.QueryMeleeSkillTargets(Range);
                    }
                    for (var i = 0; i < potentialTargets.Count; i++)
                    {
                        //if (sInfo.Caster.Faction.Hates(potentialTargets[i].Faction)) //disabled for testing
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