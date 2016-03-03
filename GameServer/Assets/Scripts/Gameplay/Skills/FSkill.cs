using System;
using System.Collections.Generic;
using Common;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills
{
    public class FSkill : ScriptableObject
    {
        [ReadOnly] public byte animation;

        [ReadOnly] public byte animation2;

        [ReadOnly] public float animationMovementForward;

        [ReadOnly] public float animationMovementLeft;

        [ReadOnly] public float animationSpeed;

        [ReadOnly] public float animationTweenTime;

        [ReadOnly] public int animationVariation;

        [ReadOnly] public float attackSpeed = 1f;

        [ReadOnly] public EAttackType attackType;

        [ReadOnly] public ESkillCategory category;

        [ReadOnly] public ESkillClassification classification;

        [ReadOnly] public float cooldown;

        [ReadOnly] int deckSlot;

        [ReadOnly] public string description;

        [ReadOnly] public bool freezePawnMovement;

        [ReadOnly] public bool freezePawnRotation;

        [ReadOnly] public string group;

        /*
        #define UCONST_Game_Skills_MAX_TOKEN_SLOTS  3
        #define UCONST_Game_Skills_MAX_STACK_COUNT  10
        #define UCONST_Game_Skills_MAX_AIMING_DESYNC  1.0
        #define UCONST_Game_Skills_COMBO_FINISHING_MOVE_MINIMUM 	2
        #define UCONST_Game_Skills_COMBO_TIMEFRAME 	10.0
        #define UCONST_Game_Skills_COMBO_VERSUS_TIMEFRAME 	5.0
        #define UCONST_Game_Skills_COMBO_MAX_STRING_LENGTH 	9
        */

        [ReadOnly] public string internalName;

        [ReadOnly] public List<SkillKeyFrame> keyFrames = new List<SkillKeyFrame>();

        double lastCast;

        [ReadOnly] public float leetnessRating;

        [ReadOnly] public bool legalSkillTokensUpdate;

        [ReadOnly] public ECharacterAttributeType linkedAttribute;

        [ReadOnly] public EMagicType magicType;

        [ReadOnly] public float maxDistance;

        [ReadOnly] public float minDistance;

        [ReadOnly] public int minSkillTier;

        [ReadOnly] public bool paintLocation;

        [ReadOnly] public float paintLocationMaxDistance;

        [ReadOnly] public float paintLocationMinDistance;

        [ReadOnly] public ESkillTarget requiredTarget;

        [ReadOnly] public EWeaponCategory requiredWeapon;

        [ReadOnly] public int resourceID;

        [SerializeField, ReadOnly] int sigilSlots;

        [ReadOnly] public EComboType skillComboType;

        [ReadOnly] public string skillname; //name

        [ReadOnly] public bool skillRequiresEquippedWeapon;

        [ReadOnly] public bool skillRollsCombatBar;

        [ReadOnly] public int stackCount;

        [ReadOnly] public EStackType stackType;

        [ReadOnly] public float targetCone;

        [ReadOnly] public float targetDelay;

        [ReadOnly] public List<string> temporaryLegalSkillTokens = new List<string>();

        [ReadOnly] public List<EContentClass> usableByClass = new List<EContentClass>();

        [ReadOnly] public bool weaponTracer;

        public int SigilSlots
        {
            get { return sigilSlots; }
            set { sigilSlots = value; }
        }

        public int DeckSlot
        {
            get { return deckSlot; }
            set { deckSlot = value; }
        }

        public double LastCast
        {
            get { return lastCast; }
            set { lastCast = value; }
        }

        public bool IsCooldownReady(float nowTime)
        {
            return nowTime - lastCast >= cooldown;
        }

        public float GetSkillDuration(Character ch)
        {
            return ch.GetDurationForAnimation(animationVariation);
        }

        public void RunEvents(SkillContext sInfo)
        {
            foreach (var kf in keyFrames)
            {
                if (kf.Time <= sInfo.currentSkillTime)
                {
                    var ev = kf.EventGroup;
                    if (ev != null)
                    {
                        foreach (var se in ev.events)
                        {
                            se.Execute(sInfo, sInfo.Caster);
                        }
                    }
                }
            }
        }

        public void Reset()
        {
            foreach (var kf in keyFrames)
            {
                kf.Reset();
            }
        }

        public void DeepClone()
        {
            foreach (var kf in keyFrames)
            {
                kf.DeepClone();
            }
        }

        [Serializable]
        public class SkillKeyFrame
        {
            public SkillEventGroup EventGroup;
            public string Name;
            public float Time;

            public void DeepClone()
            {
                if (EventGroup != null)
                {
                    EventGroup = Instantiate(EventGroup);
                    EventGroup.DeepClone();
                }
            }

            public void Reset()
            {
                if (EventGroup != null)
                {
                    EventGroup.Reset();
                }
            }
        }
    }
}