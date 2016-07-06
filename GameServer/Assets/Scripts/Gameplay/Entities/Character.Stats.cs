using System;
using System.Collections;
using Common;
using Network;
using UnityEngine;

namespace Gameplay.Entities
{
    public abstract partial class Character
    {
        const int MAX_ATTRIBUTE_VALUE = 100;
        const float AFFINITY_MULTIPLIER = 0.375f;

        [NonSerialized] public float AttackSpeedBonus;

        [NonSerialized] EContentClass classType = EContentClass.ECC_NoClass;

        [NonSerialized] public float DamageBonus;

        [NonSerialized] public float MagicResistance;

        [NonSerialized] public float MeleeResistance;

        [NonSerialized] public float MovementSpeedBonus;

        [NonSerialized] public float RangedResistance;

        [NonSerialized] public float RuneAffinity;

        [NonSerialized] public float SoulAffinity;

        [NonSerialized] public float SpiritAffinity;

        public EContentClass ClassType
        {
            get { return classType; }
            set { classType = value; }
        }

        public int StateRank
        {
            get
            {
                switch (PepRank)
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

        public void InitializeStats()
        {
            UpdateAffinities();
            UpdateResistances();
            //StartCoroutine(RegenRoutine(1f));
        }

        float lastRegen;
        void RegenRoutine(float tickInterval)
        {
            if (Time.time - lastRegen > tickInterval)
            {
                DeRegenerateStats(tickInterval);
                lastRegen = Time.time;
            }
            //while (true)
            //{
            //    if (combatMode != ECombatMode.CBM_Idle)
            //    {
            //        DeRegenerateStats(tickInterval);
            //    }
            //    var t = Time.time;
            //    while (Time.time - t < tickInterval) yield return null;
            //    //yield return new WaitForSeconds(tickInterval);
            //}
        }

        protected void UpdateAffinities()
        {
            RuneAffinity = Body*AFFINITY_MULTIPLIER;
            SpiritAffinity = Mind*AFFINITY_MULTIPLIER;
            SoulAffinity = Focus*AFFINITY_MULTIPLIER;
        }

        protected void UpdateResistances()
        {
            //TODO: calculate values from equipment etc
        }

        void RecalculateMovementSpeedBonus()
        {
            var prevBonus = MovementSpeedBonus;
            var newBonus = Physique*10f;
            if (!Mathf.Approximately(prevBonus, newBonus))
            {
                MovementSpeedBonus = newBonus;
                SetMoveSpeed(_groundSpeed); //just to trigger the broadcast TODO find cleaner solution
            }
        }

        public float GetCharacterStatistic(EVSCharacterStatistic statistic)
        {
            switch (statistic)
            {
                case EVSCharacterStatistic.EVSCS_Body:
                    return Body;
                case EVSCharacterStatistic.EVSCS_Mind:
                    return Mind;
                case EVSCharacterStatistic.EVSCS_Focus:
                    return Focus;
                case EVSCharacterStatistic.EVSCS_Physique:
                    return Physique;
                case EVSCharacterStatistic.EVSCS_Morale:
                    return Morale;
                case EVSCharacterStatistic.EVSCS_Concentration:
                    return Concentration;
                case EVSCharacterStatistic.EVSCS_Health:
                    return Health;
                case EVSCharacterStatistic.EVSCS_MaxHealth:
                    return MaxHealth;
                case EVSCharacterStatistic.EVSCS_FameLevel:
                    return FameLevel;
                case EVSCharacterStatistic.EVSCS_PePRank:
                    return PepRank;
                case EVSCharacterStatistic.EVSCS_RuneAffinity:
                    return RuneAffinity;
                case EVSCharacterStatistic.EVSCS_SoulAffinity:
                    return SoulAffinity;
                case EVSCharacterStatistic.EVSCS_SpiritAffinity:
                    return SpiritAffinity;
                case EVSCharacterStatistic.EVSCS_AttackSpeed:
                    return AttackSpeedBonus;
                case EVSCharacterStatistic.EVSCS_MovementSpeed:
                    return MovementSpeedBonus;
                case EVSCharacterStatistic.EVSCS_AffinitySum:
                    return RuneAffinity + SoulAffinity + SpiritAffinity;
                case EVSCharacterStatistic.EVSCS_AttributeSum:
                    return Body + Mind + Focus;
                case EVSCharacterStatistic.EVSCS_StateSum:
                    return Physique + Morale + Concentration;
                case EVSCharacterStatistic.EVSCS_MeleeResistance:
                    return MeleeResistance;
                case EVSCharacterStatistic.EVSCS_RangedResistance:
                    return RangedResistance;
                case EVSCharacterStatistic.EVSCS_MagicResistance:
                    return MagicResistance;
                case EVSCharacterStatistic.EVSCS_ResistanceSum:
                    return MeleeResistance + RangedResistance + MagicResistance;
                default:
                    return 0;
            }
        }

        public void DeRegenerateStats(float deltaTime)
        {
            var prevValue = Health;
            Health = Mathf.MoveTowards(Health, MaxHealth, HealthRegeneration*deltaTime);
            if (Health != prevValue)
            {
                OnHealthChanged();
            }
            prevValue = Physique;
            if (Physique < _physiqueDelta)
            {
                Physique = Mathf.MoveTowards(Physique, StateRank + _physiqueDelta, PhysiqueRegeneration*deltaTime);
            }
            else if (Physique > _physiqueDelta)
            {
                Physique = Mathf.MoveTowards(Physique, StateRank + _physiqueDelta, PhysiqueDegeneration*deltaTime);
            }
            if (prevValue != Physique)
            {
                OnPhysiqueChanged();
            }
            prevValue = Morale;
            if (Morale < _moraleDelta)
            {
                Morale = Mathf.MoveTowards(Morale, StateRank + _moraleDelta, MoraleRegeneration*deltaTime);
            }
            else if (Morale > _moraleDelta)
            {
                Morale = Mathf.MoveTowards(Morale, StateRank + _moraleDelta, MoraleDegeneration*deltaTime);
            }
            if (prevValue != Morale)
            {
                OnMoraleChanged();
            }
            prevValue = Concentration;
            if (Concentration < _concentrationDelta)
            {
                Concentration = Mathf.MoveTowards(Concentration, StateRank + _concentrationDelta, ConcentrationRegeneration*deltaTime);
            }
            else if (Concentration > _concentrationDelta)
            {
                Concentration = Mathf.MoveTowards(Concentration, StateRank + _concentrationDelta, ConcentrationDegeneration*deltaTime);
            }
            if (prevValue != Concentration)
            {
                OnConcentrationChanged();
            }
        }

        #region InstalledDeltas

        int _physiqueDelta = 0;
        int _moraleDelta = 0;
        int _concentrationDelta = 0;

        #endregion

        #region BaseStats

        [Header("Stats")] [ReadOnly] public int FameLevel;

        [ReadOnly] public int PepRank;

        [ReadOnly] public float Health;

        [ReadOnly] public int MaxHealth;

        [NonSerialized] public float HealthRegeneration;

        public void SetFame(int newLevel)
        {
            var previousFame = FameLevel;
            FameLevel = Mathf.Clamp(newLevel, GameConfiguration.CharacterDefaults.MinFame, GameConfiguration.CharacterDefaults.MaxFame);
            if (previousFame != FameLevel)
            {
                OnFameChanged();
            }
        }

        protected virtual void OnFameChanged()
        {
            PlayEffect(EPawnEffectType.EPET_LevelUp);
        }

        public void SetPep(int newRank)
        {
            var previousPep = PepRank;
            PepRank = Mathf.Clamp(newRank, GameConfiguration.CharacterDefaults.MinPep, GameConfiguration.CharacterDefaults.MaxPep);
            if (previousPep != PepRank)
            {
                OnPepChanged();
            }
        }

        protected virtual void OnPepChanged()
        {
            PlayEffect(EPawnEffectType.EPET_RankUp);
        }

        public float SetHealth(float newAmount)
        {
            var previousAmount = Health;
            Health = newAmount;
            Health = Mathf.Clamp(Health, 0, MaxHealth);
            if (!Mathf.Approximately(previousAmount, Health))
            {
                OnHealthChanged();
            }
            return Health - previousAmount;
        }

        protected virtual void OnHealthChanged()
        {
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEHEALTH(this));
        }

        public void SetMaxHealth(int amount)
        {
            float previousAmount = MaxHealth;
            MaxHealth = amount;
            if (MaxHealth < Health)
            {
                if (MaxHealth < 0)
                {
                    MaxHealth = 0;
                }
                SetHealth(MaxHealth);
            }
            if (!Mathf.Approximately(previousAmount, Health))
            {
                OnMaxHealthChanged();
            }
        }

        protected virtual void OnMaxHealthChanged()
        {
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMAXHEALTH(this));
        }

        #endregion

        #region Attributes

        [ReadOnly] public int Body;

        [ReadOnly] public int Mind;

        [ReadOnly] public int Focus;

        public float GetCharacterAttribute(ECharacterAttributeType type)
        {
            switch (type)
            {
                default:
                    return 0;
                case ECharacterAttributeType.ECAT_Body:
                    return Body;
                case ECharacterAttributeType.ECAT_Mind:
                    return Mind;
                case ECharacterAttributeType.ECAT_Focus:
                    return Focus;
            }
        }

        public void SetCharacterAttribute(ECharacterAttributeType type, int value)
        {
            value = Mathf.Clamp(value, 0, MAX_ATTRIBUTE_VALUE);
            switch (type)
            {
                case ECharacterAttributeType.ECAT_Body:
                    Body = value;
                    UpdateAffinities();
                    break;
                case ECharacterAttributeType.ECAT_Mind:
                    Mind = value;
                    UpdateAffinities();
                    break;
                case ECharacterAttributeType.ECAT_Focus:
                    Focus = value;
                    UpdateAffinities();
                    break;
            }
        }

        #endregion

        #region CombatStats

        [ReadOnly] public float Physique;

        [NonSerialized] public float PhysiqueRegeneration;

        [NonSerialized] public float PhysiqueDegeneration;

        [ReadOnly] public float Morale;

        [NonSerialized] public float MoraleRegeneration;

        [NonSerialized] public float MoraleDegeneration;

        [ReadOnly] public float Concentration;

        [NonSerialized] public float ConcentrationRegeneration;

        [NonSerialized] public float ConcentrationDegeneration;

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
                    change = (int) (newValue - Physique);
                    Physique = newValue;
                    if (change != 0)
                    {
                        OnPhysiqueChanged();
                        RecalculateMovementSpeedBonus();
                    }
                    break;
                case ECharacterStateHealthType.ECSTH_Morale:
                    change = (int) (newValue - Morale);
                    Morale = newValue;
                    if (change != 0)
                    {
                        OnMoraleChanged();
                    }
                    break;
                case ECharacterStateHealthType.ECSTH_Concentration:
                    change = (int) (newValue - Concentration);
                    Concentration = newValue;
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
                    return Health;
                case ECharacterStateHealthType.ECSTH_Morale:
                    return Morale;
                case ECharacterStateHealthType.ECSTH_Physique:
                    return Physique;
                case ECharacterStateHealthType.ECSTH_Concentration:
                    return Concentration;
                default:
                    return 0;
            }
        }

        protected virtual void OnConcentrationChanged()
        {
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATECONCENTRATION(this));
        }

        protected virtual void OnMoraleChanged()
        {
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMORALE(this));
        }

        protected virtual void OnPhysiqueChanged()
        {
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEPHYSIQUE(this));
        }

        #endregion
    }
}