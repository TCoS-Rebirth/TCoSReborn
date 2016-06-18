using System;
using Common;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills.Events
{
    public abstract class SkillEvent : ScriptableObject
    {

        public const int CF_VS_RECURSIVE = 268435456;
        public const int CF_VS_REQUIRED_BITS = 299;
        public const int CF_PERTARGET_BITS = 8128;
        public const int CF_BASE_BITS = 63;
        public const int CF_UNUSED_BITS = 57344;
        public const int CF_12a_FINAL_CAP = 4096;
        public const int CF_12_RESISTANCE_AFFINITY = 2048;
        public const int CF_11_ALTER_EFFECT_INPUT = 1024;
        public const int CF_10_SHARE = 512;
        public const int CF_09_TARGET_TYPE_INCREASE = 256;
        public const int CF_08_REFLECT = 128;
        public const int CF_07_IMMUNITY = 64;
        public const int CF_06_DIVIDE = 32;
        public const int CF_05_ALTER_EFFECT_OUTPUT = 16;
        public const int CF_04_MISC_BONUS = 8;
        public const int CF_03_SKILLTOKEN_BONUS = 4;
        public const int CF_02_ABSOLUTE_CAP = 2;
        public const int CF_01_CONSTANT_VALUE = 1;
        public const int VCV_CONTEXT_ALL = 67043328;
        public const int VCV_CONTEXT_GAIN = 67108864;
        public const int VCV_CONTEXT_CHAIN_REST = 33554432;
        public const int VCV_CONTEXT_CHAIN_1ST = 16777216;
        public const int VCV_CONTEXT_TRIGGERED = 8388608;
        public const int VCV_CONTEXT_REFLECT = 4194304;
        public const int VCV_CONTEXT_RETURN = 2097152;
        public const int VCV_CONTEXT_SHARERETURN = 1048576;
        public const int VCV_CONTEXT_SHAREDIVIDE = 524288;
        public const int VCV_CONTEXT_DUFFREPEAT = 262144;
        public const int VCV_CONTEXT_DUFF = 131072;
        public const int VCV_CONTEXT_DIRECT = 65536;
        public const int VCV_NO_TARGET_COUNTING = 16;
        public const int VCV_COMBO_EVENT = 8;
        public const int VCV_NOSKILL_EVENT = 4;
        public const int VCV_LOCATION = 2;
        public const int VCV_SKILLPAWN = 1;

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
        //var int export transient editinline FSkill_Event OriginalEvent;
        //var transient int SessionID;
        //var transient AimingInfo SkillPawnAimingInfo;
        //var transient byte OriginDuffCondition;

        //protected ESkillEventState eventState = ESkillEventState.SES_WAITING_FOR_DELAY;
        bool _delayDone = false;
        public string internalName;
        public float[] PerEffectFameLevelBonus = new float[32];
        public float[] PerEffectPepLevelBonus = new float[32];
        public int resourceID;
        public bool TargetCountValueSpecifier;

        public abstract bool Execute(RunningSkillContext context);

        protected bool HasDelayPassed(RunningSkillContext context)
        {
            if (!_delayDone)
            {
                _delayDone = context.IsDelayDone(Delay);
            }
            return _delayDone;
        }

        public virtual void DeepClone()
        {
        }

        public virtual void Reset()
        {
            _delayDone = false;
        }
    }
}