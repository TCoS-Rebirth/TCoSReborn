using System;
using System.Collections.Generic;
using Common;
using Gameplay.Entities;
using UnityEngine;

namespace Gameplay.Skills
{
    public class FSkill_Type : ScriptableObject
    {

        public const int MaxTokenSlots = 3;
        public const int MaxStackCount = 10;
        public const float MaxAimingDesync = 1f;
        public const int ComboFinishingMoveMinimum = 2;
        public const float ComboTimeframe = 10f;
        public const float ComboVersusTimeframe = 5f;
        public const int ComboMaxStringLength = 9;

        //[ReadOnly] public byte animation;

        [ReadOnly] public SBAnimActionFlags animation;

        [ReadOnly] public SBAnimActionFlags animation2;

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

        [ReadOnly] public string description;

        [ReadOnly] public bool freezePawnMovement;

        [ReadOnly] public bool freezePawnRotation;

        [ReadOnly] public string group;

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

        [ReadOnly] public EComboType skillComboType;

        [ReadOnly] public string skillname; //name

        [ReadOnly] public bool skillRequiresEquippedWeapon;

        [ReadOnly] public bool skillRollsCombatBar = true;

        [ReadOnly] public int stackCount;

        [ReadOnly] public EStackType stackType;

        [ReadOnly] public float targetCone;

        [ReadOnly] public float targetDelay;

        [ReadOnly] public List<string> temporaryLegalSkillTokens = new List<string>();

        [ReadOnly] public List<EContentClass> usableByClass = new List<EContentClass>();

        [ReadOnly] public bool weaponTracer;

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

        public void Reset()
        {
            for (var i = 0; i < keyFrames.Count; i++)
            {
                keyFrames[i].Reset();
            }
        }

        public void DeepClone()
        {
            for (var i = 0; i < keyFrames.Count; i++)
            {
                keyFrames[i].DeepClone();
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
                if (EventGroup == null) return;
                EventGroup = Instantiate(EventGroup);
                EventGroup.DeepClone();
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