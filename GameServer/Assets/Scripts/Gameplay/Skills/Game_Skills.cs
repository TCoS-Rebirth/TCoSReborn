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

        protected int mTiers;
        protected int mTierSlots;
        protected float mTierTimeout;
        protected float mTierTimeoutStartTime;
        protected int mCurrentTier;
        protected int mLastSkillIndex;

        public List<FSkill_Type> CharacterSkills = new List<FSkill_Type>();
        public FSkill_Type[] SkilldeckSkills = new FSkill_Type[30];

        public virtual int GetTokenSlots(FSkill_Type skill)
        {
            return 0;
        }

        public virtual void AddTokenSlot(FSkill_Type skill)
        {
        }

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
            public float lastTick;
        }

        readonly List<RunningSkillData> ActiveSkills = new List<RunningSkillData>();
        readonly List<FSkill_Event> RunningEvents = new List<FSkill_Event>();

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

        public ESkillStartFailure UseSkill(int skillID, int targetID, Vector3 targetPosition, float time, Vector3 camPos = default(Vector3))
        {
            var s = GetSkill(skillID);
            return UseSkill(s, Owner.GetRelevantEntity<Character>(targetID), targetPosition, time, camPos);
        }

        public ESkillStartFailure UseSkillIndex(int index, int targetID, Vector3 targetPosition, float time, Vector3 camPos = default(Vector3))
        {
            throw new NotImplementedException("FixMe");
            //var s = activeSkillDeck.GetSkillFromActiveTier(index);
            //return UseSkill(s, Owner.GetRelevantEntity<Character>(targetID), targetPosition, time, camPos);
        }

        ESkillStartFailure UseSkill(FSkill_Type s, Character target, Vector3 targetPosition, float time, Vector3 camPos = default(Vector3))
        {
            if (Owner.PawnState == EPawnStates.PS_DEAD)
            {
                return ESkillStartFailure.SSF_DEAD;
            }
            if (LastSkill != null)
            {
                return ESkillStartFailure.SSF_STILL_EXECUTING_SKILL;
            }
            if (s == null)
            {
                return ESkillStartFailure.SSF_INVALID_SKILL;
            }
            if (!s.IsCooldownReady(time))
            {
                return ESkillStartFailure.SSF_COOLING_DOWN;
            }
            return ESkillStartFailure.SSF_ALLOWED;
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

        /*
        final native function FireCondition(Game_Pawn aOriginPawn,byte aCondition,optional byte aAttackType,optional byte aMagicType,optional byte aEffectType);


  final native function TriggerFireCondition(array<Game_Pawn> aConditionTriggerPawn,Game_Pawn aOriginPawn,byte aCondition,optional byte aAttackType,optional byte aMagicType,optional byte aEffectType);
        */

        RunningSkillData LastSkill;
        Coroutine skillbarResetRoutine;

        protected virtual void cl_AddActiveSkill(int aSkillID, float aStartTime, float aDuration, float aSkillSpeed, bool aFreezeMovement, bool aFreezeRotation, int aTokenItemID, int AnimVarNr, Vector3 aLocation, Quaternion aRotation)
        {
            LastSkill.Skill = GetSkill(aSkillID);
            LastSkill.StartTime = aStartTime;                                            
            LastSkill.Duration = aDuration;                                            
            LastSkill.SkillSpeed = aSkillSpeed;                                       
            LastSkill.LockedMovement = aFreezeMovement;                                 
            LastSkill.LockedRotation = aFreezeRotation;                               
            LastSkill.SpecificTarget = null;
            ActiveSkills.Add(LastSkill);
        }

        public virtual void sv2cl_ClearLastSkill()
        {
            var oldSkill = LastSkill.Skill;
            LastSkill.StartTime = 0;                                    
            LastSkill.Duration = 0;                                      
            LastSkill.Skill = null;                                              
            LastSkill.SpecificTarget = null;        
            if (oldSkill.skillRollsCombatBar)
            {                                        
                AdvanceToNextTier();
            }
        }

        protected void AdvanceToNextTier()
        {
            mCurrentTier++;
            if (mCurrentTier > mTiers) RollbackToFirstTier();
        }

        protected void RollbackToFirstTier()
        {
            mCurrentTier = 0;
        }

        public virtual void RunEvent(FSkill_Type skill, FSkillEventFx fxEvent, int flags, Character skillPawn, Character triggerPawn, Character targetPawn)
        {
            var clone = Instantiate(fxEvent);
            clone.DeepClone();
            clone.Initialize(flags, skill, skillPawn, triggerPawn, targetPawn, Owner.Position, 0, Time.time, EDuffCondition.EDC_NoCondition); //TODO duffcondition
            RunningEvents.Add(clone);
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(Owner,
                skill.resourceID, clone.resourceID, flags, skillPawn, triggerPawn, targetPawn,
                clone.ElapsedTime));
        }

        public virtual void RunEventL(FSkill_Type skill, FSkillEventFx fxEvent, int flags, Character skillPawn, Character triggerPawn, Vector3 location, Character targetPawn)
        {
            var clone = Instantiate(fxEvent);
            clone.DeepClone();
            clone.Initialize(flags, skill, skillPawn, triggerPawn, targetPawn, location, 0, Time.time, EDuffCondition.EDC_NoCondition); //TODO duffcondition
            RunningEvents.Add(clone);
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(Owner,
                skill.resourceID, clone.resourceID, flags, skillPawn, triggerPawn, targetPawn, location,
                clone.ElapsedTime));   
        }

        #endregion
    }

}
