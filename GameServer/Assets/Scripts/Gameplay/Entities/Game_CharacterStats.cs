using System;
using Common;
using Network;
using UnityEngine;

namespace Gameplay.Entities
{
    public abstract class Game_CharacterStats : ScriptableObject
    {

        Character Owner;

        const float REGEN_UPDATE_INTERVAL = 1f;

        const int MAX_ATTRIBUTE_VALUE = 100;
        const float AFFINITY_MULTIPLIER = 0.375f;

        const byte EFF_Stats = 8;
        const byte EFF_Animation = 4;
        const byte EFF_Rotation = 2;
        const byte EFF_Movement = 1;

        bool _freezePosition;

        bool _freezeRotation;

        //
        public struct CharacterStatsRecord
        {
            public int Body;
            public int Mind;
            public int Focus;
            public float Physique;
            public float Morale;
            public float Concentration;
            public int FameLevel;
            public int PePRank;
            public float RuneAffinity;
            public float SpiritAffinity;
            public float SoulAffinity;
            public float MeleeResistance;
            public float RangedResistance;
            public float MagicResistance;
            public int MaxHealth;
            public float PhysiqueRegeneration;
            public float PhysiqueDegeneration;
            public float MoraleRegeneration;
            public float MoraleDegeneration;
            public float ConcentrationRegeneration;
            public float ConcentrationDegeneration;
            public float HealthRegeneration;
            public float AttackSpeedBonus;
            public float MovementSpeedBonus;
            public float DamageBonus;
            public float CopyHealth;
        }

        protected int mBaseBody;
        protected int mBaseMind;
        protected int mBaseFocus;
        protected int mBaseMaxHealth;
        protected float mBaseRuneAffinity;
        protected float mBaseSpiritAffinity;
        protected float mBaseSoulAffinity;
        protected int mExtraBodyPoints;
        protected int mExtraMindPoints;
        protected int mExtraFocusPoints;
        protected float mHealth;
        protected ECharacterStatsCharacterState mState;
        protected byte mFrozenFlags; //EFF_X
        protected int mFreezeMovementCount;
        protected int mFreezeRotationCount;
        protected int mFreezeAnimationCount;
        protected int mFreezeStatsCount;
        protected int mBaseMovementSpeed;
        protected int mMovementSpeed;
        protected float mRearDamageIncrease;
        protected float mFrontDamageIncrease;
        protected float mConcentrationAttackSpeedBonus;
        public CharacterStatsRecord mRecord;
        protected EContentClass mCharacterClass;
        protected int mStateRankShift;
        protected int mRegenPointShift;
        protected int mPhysiqueLevel;
        protected int mMoraleLevel;
        protected int mConcentrationLevel;
        protected int mBodyDelta;
        protected int mMindDelta;
        protected int mFocusDelta;
        protected int mAttributesDeltaInternal;
        protected float mRuneAffinityDelta;
        protected float mSpiritAffinityDelta;
        protected float mSoulAffinityDelta;
        protected int mMaxHealthDelta;
        protected float mPhysiqueRegenerationDelta;
        protected float mPhysiqueDegenerationDelta;
        protected float mMoraleRegenerationDelta;
        protected float mMoraleDegenerationDelta;
        protected float mConcentrationRegenerationDelta;
        protected float mConcentrationDegenerationDelta;
        protected float mHealthRegenerationDelta;
        protected float mMeleeResistanceDelta;
        protected float mRangedResistanceDelta;
        protected float mMagicResistanceDelta;
        protected int mPePRankDelta;
        protected float mAttackSpeedBonusDelta;
        protected float mMovementSpeedBonusDelta;
        protected float mDamageBonusDelta;

        public byte FrozenFlags { get { return mFrozenFlags; } }

        public void SetConcentration(float Value)
        {
            mConcentrationLevel = (int) Value;
            mRecord.Concentration = mConcentrationLevel;
        }

        public void SetMorale(float Value)
        {
            mMoraleLevel = (int) Value;
            mRecord.Morale = mMoraleLevel;
        }

        public void SetPhysique(float Value)
        {
            mPhysiqueLevel = (int) Value;
        }

        public EContentClass GetCharacterClass()
        {
            return mCharacterClass;
        }

        public void SetCharacterClass(byte ClassId)
        {
            mCharacterClass = (EContentClass) ClassId;
        }

        public byte GetArchetype()
        {
            throw new NotImplementedException();
        }

        public virtual int GetPePRank()
        {
            return mRecord.PePRank;
        }

        public virtual int GetFameLevel()
        {
            return mRecord.FameLevel;
        }

        public int GetPrevFameLevelPoints(int aCurrentLevel)
        {
            throw new NotImplementedException("Use the Levelprogression data");
        }

        public int GetNextFameLevelPoints(int currentLevel)
        {
            throw new NotImplementedException("Use the Levelprogression data");
        }

        public bool AreStatsFrozen()
        {
            return (mFrozenFlags & EFF_Stats) == EFF_Stats;
        }

        public bool IsMovementLimited()
        {
            throw new NotImplementedException();
        }

        public bool IsAnimationFrozen()
        {
            return (mFrozenFlags & EFF_Animation) == EFF_Animation;
        }

        public bool IsRotationFrozen()
        {
            return (mFrozenFlags & EFF_Rotation) == EFF_Rotation;
        }

        public bool IsMovementFrozen()
        {
            return (mFrozenFlags & EFF_Movement) == EFF_Movement;
        }

        public void FreezeStatsTimed(float aDuration)
        {
            throw new NotImplementedException();
        }

        public void FreezeStats(bool aFreeze)
        {
            mFrozenFlags |= EFF_Stats;
        }

        public void FreezeAnimationTimed(float aDuration)
        {
            throw new NotImplementedException();
        }

        public void FreezeAnimation(bool aFreeze)
        {
            if (aFreeze)
            {
                mFrozenFlags |= EFF_Animation;
            }
            else
            {
                mFrozenFlags = (byte) (mFrozenFlags & ~EFF_Animation);
            }
        }

        public void FreezeRotationTimed(float aDuration)
        {
            throw new NotImplementedException();
        }

        public void FreezeRotation(bool aFreeze)
        {
            if (aFreeze)
            {
                mFrozenFlags |= EFF_Rotation;
            }
            else
            {
                mFrozenFlags = (byte)(mFrozenFlags & ~EFF_Rotation);
            }
        }

        public void FreezeMovementTimed(float aDuration)
        {
            throw new NotImplementedException();
        }

        public void FreezeMovement(bool aFreeze)
        {
            if (aFreeze)
            {
                mFrozenFlags |= EFF_Movement;
            }
            else
            {
                mFrozenFlags = (byte)(mFrozenFlags & ~EFF_Movement);
            }
        }

        public void ResetAttributes()
        {
            throw new NotImplementedException();
        }

        public void SetAttributes(int Body, int Mind, int Focus)
        {
            throw new NotImplementedException();
        }

        public void UnsetStatsState(ECharacterStatsCharacterState aNewState)
        {
            mState = aNewState;
        }

        public void SetStatsState(ECharacterStatsCharacterState aNewState)
        {
            mState = aNewState;
        }

        public void ForceCalculationUpdate()
        {
            throw new NotImplementedException();
        }

        public void IncreaseMeleeResistanceDelta(float f)
        {
            mMeleeResistanceDelta += f;
        }

        public void IncreaseRangedResistanceDelta(float f)
        {
            mRangedResistanceDelta += f;
        }

        public void IncreaseMagicResistanceDelta(float f)
        {
            mMagicResistanceDelta += f;
        }

        public void IncreaseHealthRegenerationDelta(float aDelta)
        {
            mHealthRegenerationDelta += aDelta;
        }

        public void IncreaseConcentrationDegenerationDelta(float aDelta)
        {
            mConcentrationDegenerationDelta += aDelta;
        }

        public void IncreaseConcentrationRegenerationDelta(float aDelta)
        {
            mConcentrationRegenerationDelta += aDelta;
        }

        public void IncreaseMoraleDegenerationDelta(float aDelta)
        {
            mMoraleDegenerationDelta += aDelta;
        }

        public void IncreaseMoraleRegenerationDelta(float aDelta)
        {
            mMoraleRegenerationDelta += aDelta;
        }

        public void IncreasePhysiqueDegenerationDelta(float aDelta)
        {
            mPhysiqueDegenerationDelta += aDelta;
        }

        public void IncreasePhysiqueRegenerationDelta(float aDelta)
        {
            mPhysiqueRegenerationDelta += aDelta;
        }

        public void IncreaseDamageBonusDelta(float aDelta)
        {
            mDamageBonusDelta += aDelta;
        }

        public void IncreaseMovementSpeedBonusDelta(float aDelta)
        {
            mMovementSpeedBonusDelta += aDelta;
        }

        public void IncreaseAttackSpeedBonusDelta(float aDelta)
        {
            mAttackSpeedBonusDelta += aDelta;
        }

        public void IncreaseConcentration(float aDelta)
        {
            Debug.Log("correct?");
            mConcentrationLevel += (int)aDelta;
        }

        public void IncreaseMorale(float aDelta)
        {
            Debug.Log("correct?");
            mMoraleLevel += (int)aDelta;
        }

        public void IncreasePhysique(float aDelta)
        {
            Debug.Log("correct?");
            mPhysiqueLevel += (int)aDelta;
        }

        public void IncreasePePRankDelta(int aDelta)
        {
            mPePRankDelta += aDelta;
        }

        public void IncreaseMaxHealthDelta(int aDelta)
        {
            mMaxHealthDelta += aDelta;
        }

        public void IncreaseSoulAffinityDelta(float aDelta)
        {
            mSoulAffinityDelta += aDelta;
        }

        public void IncreaseSpiritAffinityDelta(float aDelta)
        {
            mSpiritAffinityDelta += aDelta;
        }

        public void IncreaseRuneAffinityDelta(float aDelta)
        {
            mRuneAffinityDelta += aDelta;
        }

        public void IncreaseFocusDelta(int aDelta)
        {
            mFocusDelta += aDelta;
        }

        public void IncreaseMindDelta(int aDelta)
        {
            mMindDelta += aDelta;
        }

        public void IncreaseBodyDelta(int aDelta)
        {
            mBodyDelta += aDelta;
        }

        public void IncreaseFrontDamageIncrease(float aDelta)
        {
            mFrontDamageIncrease += aDelta;
        }

        public void IncreaseRearDamageIncrease(float aDelta)
        {
            mRearDamageIncrease += aDelta;
        }

        public int GetAttributePoints(ECharacterAttributeType aAttribute)
        {
            throw new NotImplementedException();
        }

        /*

  protected native function sv2clrel_UpdateStateRankShift_CallStub();


  protected native event sv2clrel_UpdateStateRankShift(int aStateRankShift);


  protected native function sv2clrel_UpdateMovementSpeed_CallStub();


  protected native event sv2clrel_UpdateMovementSpeed(int aMovementSpeed);


  protected native function sv2clrel_UpdateFrozenFlags_CallStub();


  protected native event sv2clrel_UpdateFrozenFlags(byte aFrozenFlags);


  protected native function sv2clrel_UpdateMaxHealth_CallStub();


  protected native event sv2clrel_UpdateMaxHealth(int aMaxHealth);


  protected native function sv2clrel_UpdateHealth_CallStub();


  protected native event sv2clrel_UpdateHealth(float aHealth);


  protected native function sv2clrel_UpdateConcentration_CallStub();


  protected native event sv2clrel_UpdateConcentration(float aConcentration);


  protected native function sv2clrel_UpdateMorale_CallStub();


  protected native event sv2clrel_UpdateMorale(float aMorale);


  protected native function sv2clrel_UpdatePhysique_CallStub();


  protected native event sv2clrel_UpdatePhysique(float aPhysique);


  protected native function sv2clrel_UpdateStateVariables_CallStub();


  protected native event sv2clrel_UpdateStateVariables(float aPhysique,float aMorale,float aConcentration);


  protected native function sv2cl_UpdateMagicResistance_CallStub();


  protected native event sv2cl_UpdateMagicResistance(float aMagicResistance);


  protected native function sv2cl_UpdateRangedResistance_CallStub();


  protected native event sv2cl_UpdateRangedResistance(float aRangedResistance);


  protected native function sv2cl_UpdateMeleeResistance_CallStub();


  protected native event sv2cl_UpdateMeleeResistance(float aMeleeResistance);


  protected native function sv2cl_UpdateFocusDelta_CallStub();


  protected native event sv2cl_UpdateFocusDelta(int aFocusDelta);


  protected native function sv2cl_UpdateMindDelta_CallStub();


  protected native event sv2cl_UpdateMindDelta(int aMindDelta);


  protected native function sv2cl_UpdateBodyDelta_CallStub();


  protected native event sv2cl_UpdateBodyDelta(int aBodyDelta);


  static function float sv_GetMoraleLevelBonus(int MoraleLevel) {
    return Class'Game_CharacterStats'.default.mMoraleLevelBonus[MoraleLevel + 5];//0000 : 04 1A 92 00 C0 91 1C 11 2C 05 16 12 20 C8 28 5F 01 05 00 2C 02 88 92 1C 11 
    //04 1A 92 00 C0 91 1C 11 2C 05 16 12 20 C8 28 5F 01 05 00 2C 02 88 92 1C 11 04 0B 47 
  }


  native function sv_ResetFreezeStats();


  native function sv_ResetFreezeAnimation();


  native function sv_ResetFreezeRotation();


  native function sv_ResetFreezeMovement();


  protected native function sv2clrel_FreezeAnimation_CallStub();


  protected event sv2clrel_FreezeAnimation(bool aFreeze) {
    if (aFreeze) {                                                              //0000 : 07 1F 00 2D 00 58 51 1F 11 
      mFrozenFlags = mFrozenFlags | 4;                                          //0009 : 0F 01 68 52 1F 11 39 3D 9E 39 3A 01 68 52 1F 11 2C 04 16 
    } else {                                                                    //001C : 06 36 00 
      mFrozenFlags = mFrozenFlags & 255 - 4;                                    //001F : 0F 01 68 52 1F 11 39 3D 9C 39 3A 01 68 52 1F 11 93 2C FF 2C 04 16 16 
    }
    Outer.PauseAnim(aFreeze);                                                   //0036 : 19 01 00 E4 6B 0F 0C 00 00 1B 32 0D 00 00 2D 00 58 51 1F 11 16 
    //07 1F 00 2D 00 58 51 1F 11 0F 01 68 52 1F 11 39 3D 9E 39 3A 01 68 52 1F 11 2C 04 16 06 36 00 0F 
    //01 68 52 1F 11 39 3D 9C 39 3A 01 68 52 1F 11 93 2C FF 2C 04 16 16 19 01 00 E4 6B 0F 0C 00 00 1B 
    //32 0D 00 00 2D 00 58 51 1F 11 16 04 0B 47 
  }


  native function sv_Resurrect();
        */
        //

        //byte extraBodyPoints;

        //public byte ExtraBodyPoints
        //{
        //    get { return extraBodyPoints; }
        //    set { extraBodyPoints = value; }
        //}

        //byte extraMindPoints;

        //public byte ExtraMindPoints
        //{
        //    get { return extraMindPoints; }
        //    set { extraMindPoints = value; }
        //}

        //byte extraFocusPoints;

        //public byte ExtraFocusPoints
        //{
        //    get { return extraFocusPoints; }
        //    set { extraFocusPoints = value; }
        //}

        public int StateRank
        {
            get
            {
                switch (GetPePRank())
                {
                    case 1:
                        return 1;
                    case 2:
                        return 1;
                    case 3:
                        return 2;
                    case 4:
                        return 2;
                    case 5:
                        return 3;
                    default:
                        return 0;
                }
            }
        }

        public virtual void Init(Character character)
        {
            Owner = character;
            //UpdateAffinities();
            //UpdateResistances();
            //StartCoroutine(RegenRoutine(1f));
        }

        public virtual void OnFrame()
        {
            //RegenRoutine();
        }

        //float lastRegen;

        //void RegenRoutine()
        //{
        //    if (Time.time - lastRegen > REGEN_UPDATE_INTERVAL)
        //    {
        //        DeRegenerateStats(REGEN_UPDATE_INTERVAL);
        //        lastRegen = Time.time;
        //    }
        //}

        //void RecalculateMovementSpeedBonus()
        //{
            //var prevBonus = MovementSpeedBonus;
            //var newBonus = Physique*10f;
            //if (!Mathf.Approximately(prevBonus, newBonus))
            //{
            //    MovementSpeedBonus = newBonus;
            //    Owner.SetMoveSpeed(Owner.GroundSpeed); //just to trigger the broadcast TODO find cleaner solution
            //}
        //}

        public virtual void GiveFame(int amount)
        {
        }

        public float GetCharacterStatistic(EVSCharacterStatistic statistic)
        {
            switch (statistic)
            {
                case EVSCharacterStatistic.EVSCS_Body:
                    return mRecord.Body;
                case EVSCharacterStatistic.EVSCS_Mind:
                    return mRecord.Mind;
                case EVSCharacterStatistic.EVSCS_Focus:
                    return mRecord.Focus;
                case EVSCharacterStatistic.EVSCS_Physique:
                    return mRecord.Physique;
                case EVSCharacterStatistic.EVSCS_Morale:
                    return mRecord.Morale;
                case EVSCharacterStatistic.EVSCS_Concentration:
                    return mRecord.Concentration;
                case EVSCharacterStatistic.EVSCS_Health:
                    return mRecord.CopyHealth;
                case EVSCharacterStatistic.EVSCS_MaxHealth:
                    return mRecord.MaxHealth;
                case EVSCharacterStatistic.EVSCS_FameLevel:
                    return GetFameLevel();
                case EVSCharacterStatistic.EVSCS_PePRank:
                    return GetPePRank();
                case EVSCharacterStatistic.EVSCS_RuneAffinity:
                    return mRecord.RuneAffinity;
                case EVSCharacterStatistic.EVSCS_SoulAffinity:
                    return mRecord.SoulAffinity;
                case EVSCharacterStatistic.EVSCS_SpiritAffinity:
                    return mRecord.SpiritAffinity;
                case EVSCharacterStatistic.EVSCS_AttackSpeed:
                    return mRecord.AttackSpeedBonus;
                case EVSCharacterStatistic.EVSCS_MovementSpeed:
                    return mRecord.MovementSpeedBonus;
                case EVSCharacterStatistic.EVSCS_AffinitySum:
                    return mRecord.RuneAffinity + mRecord.SoulAffinity + mRecord.SpiritAffinity;
                case EVSCharacterStatistic.EVSCS_AttributeSum:
                    return mRecord.Body + mRecord.Mind + mRecord.Focus;
                case EVSCharacterStatistic.EVSCS_StateSum:
                    return mRecord.Physique + mRecord.Morale + mRecord.Concentration;
                case EVSCharacterStatistic.EVSCS_MeleeResistance:
                    return mRecord.MeleeResistance;
                case EVSCharacterStatistic.EVSCS_RangedResistance:
                    return mRecord.RangedResistance;
                case EVSCharacterStatistic.EVSCS_MagicResistance:
                    return mRecord.MagicResistance;
                case EVSCharacterStatistic.EVSCS_ResistanceSum:
                    return mRecord.MeleeResistance + mRecord.RangedResistance + mRecord.MagicResistance;
                default:
                    return 0;
            }
        }

        //public void DeRegenerateStats(float deltaTime)
        //{
        //    var prevValue = Health;
        //    Health = Mathf.MoveTowards(Health, MaxHealth, HealthRegeneration*deltaTime);
        //    if (Health != prevValue)
        //    {
        //        OnHealthChanged();
        //    }
        //    prevValue = Physique;
        //    if (Physique < _physiqueDelta)
        //    {
        //        Physique = Mathf.MoveTowards(Physique, StateRank + _physiqueDelta, PhysiqueRegeneration*deltaTime);
        //    }
        //    else if (Physique > _physiqueDelta)
        //    {
        //        Physique = Mathf.MoveTowards(Physique, StateRank + _physiqueDelta, PhysiqueDegeneration*deltaTime);
        //    }
        //    if (prevValue != Physique)
        //    {
        //        OnPhysiqueChanged();
        //    }
        //    prevValue = Morale;
        //    if (Morale < _moraleDelta)
        //    {
        //        Morale = Mathf.MoveTowards(Morale, StateRank + _moraleDelta, MoraleRegeneration*deltaTime);
        //    }
        //    else if (Morale > _moraleDelta)
        //    {
        //        Morale = Mathf.MoveTowards(Morale, StateRank + _moraleDelta, MoraleDegeneration*deltaTime);
        //    }
        //    if (prevValue != Morale)
        //    {
        //        OnMoraleChanged();
        //    }
        //    prevValue = Concentration;
        //    if (Concentration < _concentrationDelta)
        //    {
        //        Concentration = Mathf.MoveTowards(Concentration, StateRank + _concentrationDelta, ConcentrationRegeneration*deltaTime);
        //    }
        //    else if (Concentration > _concentrationDelta)
        //    {
        //        Concentration = Mathf.MoveTowards(Concentration, StateRank + _concentrationDelta, ConcentrationDegeneration*deltaTime);
        //    }
        //    if (prevValue != Concentration)
        //    {
        //        OnConcentrationChanged();
        //    }
        //}

        #region InstalledDeltas

        //int _physiqueDelta = 0;
        //int _moraleDelta = 0;
        //int _concentrationDelta = 0;

        #endregion

        #region BaseStats

        //[Header("Stats")] [ReadOnly] public int FameLevel;

        //[ReadOnly] public int PepRank;

        //[ReadOnly] public float Health;

        //[ReadOnly] public int MaxHealth;

        //[NonSerialized] public float HealthRegeneration;

        //public void SetFame(int newLevel)
        //{
        //    var previousFame = FameLevel;
        //    FameLevel = Mathf.Clamp(newLevel, GameConfiguration.CharacterDefaults.MinFame, GameConfiguration.CharacterDefaults.MaxFame);
        //    if (previousFame != FameLevel)
        //    {
        //        OnFameChanged();
        //    }
        //}

        protected virtual void OnFameChanged()
        {
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_PLAYERSTATS_SV2CLREL_ONLEVELUP(Owner));
            Owner.PlayEffect(EPawnEffectType.EPET_LevelUp);
        }

        //public void SetPep(int newRank)
        //{
        //    var previousPep = PepRank;
        //    PepRank = Mathf.Clamp(newRank, GameConfiguration.CharacterDefaults.MinPep, GameConfiguration.CharacterDefaults.MaxPep);
        //    if (previousPep != PepRank)
        //    {
        //        OnPepChanged();
        //    }
        //}

        protected virtual void OnPepChanged()
        {
            Owner.PlayEffect(EPawnEffectType.EPET_RankUp);
        }

        public float SetHealth(float newAmount)
        {
            Debug.Log("TODO fix");
            var previousAmount = mHealth;
            mHealth = newAmount;
            mHealth = Mathf.Clamp(mHealth, 0, mBaseMaxHealth);
            mRecord.CopyHealth = mHealth;
            if (!Mathf.Approximately(previousAmount, mHealth))
            {
                OnHealthChanged();
            }
            return mHealth - previousAmount;
        }

        protected virtual void OnHealthChanged()
        {
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEHEALTH(Owner));
        }

        //public void SetMaxHealth(int amount)
        //{
        //    float previousAmount = MaxHealth;
        //    MaxHealth = amount;
        //    if (MaxHealth < Health)
        //    {
        //        if (MaxHealth < 0)
        //        {
        //            MaxHealth = 0;
        //        }
        //        SetHealth(MaxHealth);
        //    }
        //    if (!Mathf.Approximately(previousAmount, Health))
        //    {
        //        OnMaxHealthChanged();
        //    }
        //}

        protected virtual void OnMaxHealthChanged()
        {
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMAXHEALTH(Owner));
        }

        #endregion

        #region Attributes

        //[ReadOnly] public int Body;

        //[ReadOnly] public int Mind;

        //[ReadOnly] public int Focus;

        //public float GetCharacterAttribute(ECharacterAttributeType type)
        //{
        //    switch (type)
        //    {
        //        default:
        //            return 0;
        //        case ECharacterAttributeType.ECAT_Body:
        //            return Body;
        //        case ECharacterAttributeType.ECAT_Mind:
        //            return Mind;
        //        case ECharacterAttributeType.ECAT_Focus:
        //            return Focus;
        //    }
        //}

        //public void SetCharacterAttribute(ECharacterAttributeType type, int value)
        //{
        //    value = Mathf.Clamp(value, 0, MAX_ATTRIBUTE_VALUE);
        //    switch (type)
        //    {
        //        case ECharacterAttributeType.ECAT_Body:
        //            Body = value;
        //            UpdateAffinities();
        //            break;
        //        case ECharacterAttributeType.ECAT_Mind:
        //            Mind = value;
        //            UpdateAffinities();
        //            break;
        //        case ECharacterAttributeType.ECAT_Focus:
        //            Focus = value;
        //            UpdateAffinities();
        //            break;
        //    }
        //}

        #endregion

        #region CombatStats

        //[ReadOnly] public float Physique;

        //[NonSerialized] public float PhysiqueRegeneration;

        //[NonSerialized] public float PhysiqueDegeneration;

        //[ReadOnly] public float Morale;

        //[NonSerialized] public float MoraleRegeneration;

        //[NonSerialized] public float MoraleDegeneration;

        //[ReadOnly] public float Concentration;

        //[NonSerialized] public float ConcentrationRegeneration;

        //[NonSerialized] public float ConcentrationDegeneration;

        public int SetCharacterStat(ECharacterStateHealthType type, float newValue)
        {
            if (newValue < -5)
            {
                return 0;
            }
            var change = 0;
            switch (type)
            {
                case ECharacterStateHealthType.ECSTH_Physique:
                    change = (int)(newValue - mPhysiqueLevel);
                    mPhysiqueLevel = (int)newValue;
                    if (change != 0)
                    {
                        OnPhysiqueChanged();
                        //RecalculateMovementSpeedBonus();
                    }
                    break;
                case ECharacterStateHealthType.ECSTH_Morale:
                    change = (int)(newValue - mMoraleLevel);
                    mMoraleLevel = (int)newValue;
                    if (change != 0)
                    {
                        OnMoraleChanged();
                    }
                    break;
                case ECharacterStateHealthType.ECSTH_Concentration:
                    change = (int)(newValue - mConcentrationLevel);
                    mConcentrationLevel = (int)newValue;
                    if (change != 0)
                    {
                        OnConcentrationChanged();
                    }
                    break;
            }
            return change;
        }

        public float GetCharacterStat(ECharacterStateHealthType type)
        {
            switch (type)
            {
                case ECharacterStateHealthType.ECSTH_Health:
                    return mRecord.CopyHealth;
                case ECharacterStateHealthType.ECSTH_Morale:
                    return mMoraleLevel;
                case ECharacterStateHealthType.ECSTH_Physique:
                    return mPhysiqueLevel;
                case ECharacterStateHealthType.ECSTH_Concentration:
                    return mConcentrationLevel;
                default:
                    return 0;
            }
        }

        protected virtual void OnConcentrationChanged()
        {
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATECONCENTRATION(Owner));
        }

        protected virtual void OnMoraleChanged()
        {
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMORALE(Owner));
        }

        protected virtual void OnPhysiqueChanged()
        {
            Owner.BroadcastRelevanceMessage(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEPHYSIQUE(Owner));
        }

        #endregion
    }
}