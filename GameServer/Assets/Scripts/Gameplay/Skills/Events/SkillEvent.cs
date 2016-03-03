using Common;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills.Events
{
    public class SkillEvent : ScriptableObject
    {
        public float Delay;
        //var export transient editinline FSkill_Type Skill;
        //var transient int flags;
        //var transient Game_Pawn SkillPawn;
        //var transient CharacterStatsRecord SkillPawnState;
        //var transient Game_Pawn TriggerPawn;
        //var transient Vector Location;
        //var transient float StartTime;
        //var transient float ElapsedTime;
        //var transient Rotator TriggerRotation;
        //var const export transient editinline FSkill_Event OriginalEvent;
        //var transient int SessionID;
        //var transient AimingInfo SkillPawnAimingInfo;
        //var transient byte OriginDuffCondition;

        protected ESkillEventState eventState = ESkillEventState.SES_INITIALIZING;
        public string internalName;
        public float[] PerEffectFameLevelBonus = new float[32];
        public float[] PerEffectPepLevelBonus = new float[32];
        public int resourceID;
        public bool TargetCountValueSpecifier;

        public virtual void Execute(SkillContext sInfo, Character triggerPawn)
        {
        }

        public virtual void DeepClone()
        {
        }

        public virtual void Reset()
        {
            eventState = ESkillEventState.SES_WAITING_FOR_DELAY;
        }
    }
}