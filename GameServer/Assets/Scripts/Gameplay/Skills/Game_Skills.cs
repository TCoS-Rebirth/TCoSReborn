using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Gameplay.Entities;
using Gameplay.Skills.Effects;
using Gameplay.Skills.Events;
using Network;
using UnityEngine;
using Utility;

namespace Gameplay.Skills
{
    public class Game_Skills: ScriptableObject
    {

        Character Owner;

        public const int MAX_TOKEN_SLOTS = 3;
        public const int MAX_STACK_COUNT = 10;
        public const float MAX_AIMING_DESYNC = 1f;
        public const int COMBO_FINISHING_MOVE_MINIMUM = 2;
        public const float COMBO_TIMEFRAME = 10f;
        public const float COMBO_VERSUS_TIMEFRAME = 5f;
        public const int COMBO_MAX_STRING_LENGTH = 9;

        protected const int MAX_TIERS = 6;
        protected const int MAX_TIERSLOTS = 5;

        protected int mTiers;
        protected int mTierSlots;
        protected float mTierTimeout;
        protected float mTierTimeoutStartTime;
        protected int mCurrentTier;
        protected int mLastSkillIndex;

        public List<FSkill_Type> CharacterSkills = new List<FSkill_Type>();
        public FSkill_Type[] SkilldeckSkills = new FSkill_Type[30];
        protected RunningSkillData LastSkill;
        //readonly List<RunningSkillData> ActiveSkills = new List<RunningSkillData>();
        readonly List<FSkill_Event> RunningEvents = new List<FSkill_Event>();
        Coroutine simulatedSkillAnimation;

        public class RunningSkillData
        {
            public float StartTime;
            public float Duration;
            public float EndTime;
            public float SkillSpeed;
            public FSkill_Type Skill;
            public bool LockedMovement;
            public bool LockedRotation;
            public bool ComboRelevant;
            public Character SpecificTarget;
        }

        public virtual void Init(Character character)
        {
            Owner = character;
        }

        protected FSkill_Type GetSkill(int id)
        {
            for (var i = 0; i < CharacterSkills.Count; i++)
            {
                if (CharacterSkills[i].resourceID == id)
                {
                    return CharacterSkills[i];
                }
            }
            return null;
        }

        protected bool HasSkill(int id)
        {
            return GetSkill(id) != null;
        }

        public bool IsCasting { get { return LastSkill != null; } }

        public virtual int GetTokenSlots(FSkill_Type skill) { return 0; }

        public virtual void AddTokenSlot(FSkill_Type skill) { }

        public virtual int GetSkilldeckColumnCount()
        {
            return mTierSlots;
        }

        public virtual int GetSkilldeckRowCount()
        {
            return mTiers;
        }

        public SkillLearnResult LearnSkill(FSkill_Type skill)
        {
            if (skill == null)
            {
                return SkillLearnResult.Invalid;
            }
            if (HasSkill(skill.resourceID))
            {
                return SkillLearnResult.AlreadyKnown;
            }
            CharacterSkills.Add(skill);
            return SkillLearnResult.Success;
        }

        public List<Character> QueryMeleeSkillTargets(SkillEffectRange range)
        {
            var queriedTargets = new List<Character>();
            var cols = Physics.OverlapSphere(Owner.transform.position + UnitConversion.ToUnity(range.locationOffset), range.maxRadius * UnitConversion.UnrUnitsToMeters);
            foreach (var col in cols)
            {
                var c = col.GetComponent<Character>();
                if (c == null || c == this || c.PawnState == EPawnStates.PS_DEAD || Owner.Faction.Likes(c.Faction) || Owner.GetRelevantObject(c.RelevanceID) == null)
                    continue;
                if (Owner.IsFacing(c.Position, range.angle))
                    queriedTargets.Add(c);
            }
            return queriedTargets;
        }

        public List<Character> QueryRangedSkillTargets(Vector3 point, SkillEffectRange range)
        {
            var queriedTargets = new List<Character>();
            var cols = Physics.OverlapSphere(point, range.maxRadius * UnitConversion.UnrUnitsToMeters);
            foreach (var col in cols)
            {
                var c = col.GetComponent<Character>();
                if (c == null || c == this || c.PawnState == EPawnStates.PS_DEAD || Owner.Faction.Likes(c.Faction) || Owner.GetRelevantObject(c.RelevanceID) == null)
                    continue;
                if (Owner.IsFacing(c.Position, range.angle))
                    queriedTargets.Add(c);
            }
            return queriedTargets;
        }

        #region Casting/Execution


        public void FireCondition(Character aOriginPawn, EDuffCondition aCondition, EDuffAttackType aAttackType = EDuffAttackType.EDAT_Melee, EDuffMagicType aMagicType = EDuffMagicType.EDMT_None, EEffectType aEffectType = EEffectType.EET_Damage)
        {
            
        }

        //final native function TriggerFireCondition(array<Game_Pawn> aConditionTriggerPawn, Game_Pawn aOriginPawn, byte aCondition, optional byte aAttackType, optional byte aMagicType, optional byte aEffectType);


        public void OnFrame()
        {
            for (var i = RunningEvents.Count; i-- > 0;)
            {
                RunningEvents[i].ElapsedTime += Time.deltaTime;
                if (RunningEvents[i].ElapsedTime >= RunningEvents[i].Delay)
                {
                    if (RunningEvents[i].Execute())
                    {
                        RunningEvents.RemoveAt(i);
                    }
                }
            }
            if (LastSkill != null)
            {
                if (Time.time >= LastSkill.EndTime)
                {
                    sv2cl_ClearLastSkill();
                }
            }
        }

        IEnumerator SimulateAnimationEvents(FSkill_Type skill)
        {
            var keyFrames = skill.keyFrames;
            var lastKeyFrameIndex = 0;
            while (lastKeyFrameIndex < keyFrames.Count)
            {
                yield return new WaitForSeconds(keyFrames[lastKeyFrameIndex].Time);
                if (keyFrames[lastKeyFrameIndex].EventGroup != null)
                {
                    for (var j = 0; j < keyFrames[lastKeyFrameIndex].EventGroup.events.Count; j++)
                    {
                        if (keyFrames[lastKeyFrameIndex].EventGroup.events[j] != null)
                        {
                            RunSkillEvent(skill, keyFrames[lastKeyFrameIndex].EventGroup.events[j], Owner, null, Owner.Position);
                        }
                    }
                }
                lastKeyFrameIndex++;
            }
        }

        void RunSkillEvent(FSkill_Type skill, FSkill_Event ev, Character triggerPawn, Character targetPawn, Vector3 location)
        {
            var instance = Instantiate(ev);
            instance.DeepClone();
            instance.Initialize(0, skill, Owner, triggerPawn, targetPawn, location, 0, Time.time, EDuffCondition.EDC_NoCondition);
            RunningEvents.Add(instance);
        }

        public ESkillStartFailure Execute(int skillID, int targetID, Vector3 targetPosition, float time)
        {
            var s = GetSkill(skillID);
            return Execute(s, Owner.GetRelevantEntity<Character>(targetID), targetPosition, time);
        }

        public virtual ESkillStartFailure ExecuteIndex(int index, int targetID, Vector3 targetPosition, float time)
        {
            mLastSkillIndex = index;
            var s = GetActiveTierSlotSkill(index);
            return s == null ? ESkillStartFailure.SSF_INVALID_SKILL : Execute(s, Owner.GetRelevantEntity<Character>(targetID), targetPosition, time);
        }

        ESkillStartFailure Execute(FSkill_Type skill, Character target, Vector3 targetPosition, float time)
        {
            if (Owner.PawnState == EPawnStates.PS_DEAD)
            {
                return ESkillStartFailure.SSF_DEAD;
            }
            if (LastSkill != null)
            {
                return ESkillStartFailure.SSF_STILL_EXECUTING_SKILL;
            }
            if (skill == null)
            {
                return ESkillStartFailure.SSF_INVALID_SKILL;
            }
            if (!skill.IsCooldownReady(time))
            {
                return ESkillStartFailure.SSF_COOLING_DOWN;
            }
            AddActiveSkill(skill, target, time, skill.GetSkillDuration(Owner), skill.animationSpeed, skill.freezePawnMovement, skill.freezePawnRotation, 0,
                skill.animationVariation, targetPosition, Owner.Rotation);
            return ESkillStartFailure.SSF_ALLOWED;
        }

        protected virtual void AddActiveSkill(FSkill_Type aSkill, Character target, float aStartTime, float aDuration, float aSkillSpeed, bool aFreezeMovement, bool aFreezeRotation, int aTokenItemID, int AnimVarNr, Vector3 aLocation, Quaternion aRotation)
        {
            LastSkill = new RunningSkillData
            {
                Skill = aSkill,
                StartTime = aStartTime,
                Duration = aDuration,
                EndTime = aStartTime + aDuration,
                SkillSpeed = aSkillSpeed,
                LockedMovement = aFreezeMovement,
                LockedRotation = aFreezeRotation,
                SpecificTarget = target
            };
            simulatedSkillAnimation = Owner.StartCoroutine(SimulateAnimationEvents(aSkill));
        }

        protected virtual void sv2cl_ClearLastSkill()
        {
            if (simulatedSkillAnimation != null) Owner.StopCoroutine(simulatedSkillAnimation);
            //var oldSkill = LastSkill.Skill;
            LastSkill = null;
            //if (oldSkill.skillRollsCombatBar)
            //{                                        
                AdvanceToNextTier();
            //}
        }

        protected void AdvanceToNextTier()
        {
            mCurrentTier++;
            if (mCurrentTier > mTiers) RollbackToFirstTier();
            CheckSwitchWeapon();
        }

        protected void RollbackToFirstTier()
        {
            mCurrentTier = 0;
            CheckSwitchWeapon();
        }

        void CheckSwitchWeapon()
        {
            Debug.Log("TODO Refactor to combatstats");
            //var s = GetActiveTierSlotSkill(mLastSkillIndex);
            //Owner.CombatState.SwitchWeapon(s != null ? s.requiredWeapon : EWeaponCategory.EWC_MeleeOrUnarmed);
            //Owner.CombatState
        }

        public FSkill_Type GetActiveTierSlotSkill(int aSlot)
        {
            if (aSlot < 0 || aSlot >= MAX_TIERSLOTS) return null;
            var flatIndex = mCurrentTier * MAX_TIERSLOTS + aSlot;
            if (flatIndex > MAX_TIERS*MAX_TIERSLOTS) return null;
            return SkilldeckSkills[flatIndex];
        }

        public virtual void sv2clrel_RunEvent(FSkill_Type skill, FSkillEventFx fxEvent, int flags, Character skillPawn, Character triggerPawn, Character targetPawn)
        {
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(Owner,
                skill.resourceID, fxEvent.resourceID, flags, skillPawn, triggerPawn, targetPawn,
                fxEvent.ElapsedTime));
        }

        public virtual void sv2clrel_RunEventL(FSkill_Type skill, FSkillEventFx fxEvent, int flags, Character skillPawn, Character triggerPawn, Vector3 location, Character targetPawn)
        {
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(Owner,
                skill.resourceID, fxEvent.resourceID, flags, skillPawn, triggerPawn, targetPawn, location,
                fxEvent.ElapsedTime));   
        }

        #endregion
    }

}
