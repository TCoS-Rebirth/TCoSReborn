using System.Collections.Generic;
using Common;
using Gameplay.Entities;
using Gameplay.Skills.Effects;
using UnityEngine;

namespace Gameplay.Skills.Events
{
    public class FSkillEventDirect : FSkillEventTarget
    {
        [ReadOnly] public SkillEffectDirectState _State;

        [ReadOnly] public FSkillEventDuff Buff;

        //int currentRepeat;

        [ReadOnly] public SkillEffectDirectDamage Damage;

        [ReadOnly] public float DamageMoraleBonus;

        [ReadOnly] public FSkillEventDuff Debuff;

        [ReadOnly] public SkillEffectDirectDrain Drain;

        [ReadOnly] public SkillEffectDirectFireBodySlot FireBodySlot;

        //bool firstExecute = true;

        [ReadOnly] public SkillEffectDirectHeal Heal;

        [ReadOnly] public FSkillEventFx HitFXEvent;

        [ReadOnly] public float Interval;

        [ReadOnly] public bool KeepTargets;

        //float lastRepeatTime;

        [ReadOnly] public FSkillEventFx MissFXEvent;

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

        //protected List<Character> targets = new List<Character>();

        [ReadOnly] public int TargetsPerRepeat;

        [ReadOnly] public SkillEffectDirectTeleport Teleport;

        public override bool Execute()
        {
            base.Execute();
            ExecuteDirectEffects(GatherTargets());
            return true;
        }

        public void ExecuteDirectEffects(List<Character> targets)
        {
            var success = false;
            for (var i = 0; i < targets.Count; i++) //TODO targetsperrepeat etc?
            {
                if (_State != null)
                {
                    success = _State.Apply(Skill, SkillPawn, targets[i]);
                }
                if (Damage != null)
                {
                    success = Damage.Apply(Skill, SkillPawn, targets[i]);
                }
                if (Heal != null)
                {
                    success = Heal.Apply(Skill, SkillPawn, targets[i]);
                }
                if (Buff != null)
                {
                    success = Buff.Apply(Skill, SkillPawn, targets[i]);
                }
                if (Debuff != null)
                {
                    success = Debuff.Apply(Skill, SkillPawn, targets[i]);
                }
                if (success)
                {
                    OnHitTarget(Skill, SkillPawn, targets[i]);
                }
            }
            if (!success)
            {
                OnMissedTarget(Skill, SkillPawn);
            }
        }

        public List<Character> GatherTargets()
        {
            var targets = new List<Character>();
            if (TargetSelf != ETargetMode.ETM_Never)
            {
                targets.Add(SkillPawn);
            }
            if (Range == null)
            {
                Debug.Log("skill Range undefined, cant query other targets");
                return targets;
            }
            if (TargetFriendlies != ETargetMode.ETM_Never)
            {
                var potentialTargets = SkillPawn.Skills.QueryMeleeSkillTargets(Range);
                for (var i = 0; i < potentialTargets.Count; i++)
                {
                    if (SkillPawn.Faction.Likes(potentialTargets[i].Faction))
                    {
                        targets.Add(potentialTargets[i]);
                    }
                }
            }
            if (TargetEnemies != ETargetMode.ETM_Never)
            {
                targets.Clear(); //TODO check if this conflicts
                if (Range != null)
                {
                    var potentialTargets = Skill.paintLocation ? SkillPawn.Skills.QueryRangedSkillTargets(Location, Range) : SkillPawn.Skills.QueryMeleeSkillTargets(Range);
                    for (var i = 0; i < potentialTargets.Count; i++)
                    {
                        if (!SkillPawn.Faction.Likes(potentialTargets[i].Faction))
                        {
                            targets.Add(potentialTargets[i]);
                        }
                    }
                }
                else
                {
                    Debug.Log("Range null, cant query targets (?)");
                }
            }
            return targets;
        }

        protected virtual void OnHitTarget(FSkill_Type skill, Character skillPawn, Character target)
        {
            if (HitFXEvent != null)
            {
                RunClientEvents(HitFXEvent);
            }
        }

        protected virtual void OnMissedTarget(FSkill_Type Skill, Character skillPawn)
        {
            if (MissFXEvent != null)
            {
                RunClientEvents(MissFXEvent);
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
            //firstExecute = true;
            //lastRepeatTime = 0;
            //currentRepeat = 0;
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