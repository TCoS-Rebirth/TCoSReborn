namespace Common
{

    public enum EMagicType
    {
        EMT_Soul = 0,
        EMT_Rune = 1,
        EMT_Spirit = 2,
        EMT_None = 3
    }

    public enum EDuffMagicType
    {
        EDMT_Soul = 0,
        EDMT_Rune = 1,
        EDMT_Spirit = 2,
        EDMT_None = 3,
        EDMT_All = 4
    }

    public enum EAttackType
    {
        EAT_Melee = 0,
        EAT_Ranged = 1,
        EAT_Magic = 2
    }

    public enum EDuffPriority
    {
        EDP_Lowest,
        EDP_Low,
        EDP_Normal,
        EDP_High,
        EDP_Highest
    }

    public enum EDuffAttackType
    {
        EDAT_Melee = 0,
        EDAT_Ranged = 1,
        EDAT_Magic = 2,
        EDAT_All = 3
    }

    public enum ETargetSortingMethod
    {
        ETSM_Aimed = 0,
        ETSM_Vicinity = 1,
        ETSM_Random = 2
    }

    public enum EReturnReflectMode
    {
        ERRM_Return = 0,
        ERRM_Reflect = 1
    }

    public enum ERotationMode
    {
        ERM_Unchanged,
        ERM_Facing,
        ERM_FacingOpposite,
        ERM_Random
    }

    public enum EShareMode
    {
        ESHM_ApplicantToTarget = 0,
        ESHM_TargetToApplicant = 1,
        ESHM_Both = 2
    }

    public enum EShareType
    {
        ESHT_ShareDivide = 0,
        ESHT_ShareReturn = 1
    }

    public enum ECharacterStateHealthType
    {
        ECSTH_Physique = 0,
        ECSTH_Morale = 1,
        ECSTH_Concentration = 2,
        ECSTH_Health = 3
    }

    public enum ECharacterAttributeType
    {
        ECAT_Body = 0,
        ECAT_Mind = 1,
        ECAT_Focus = 2
    }

    public enum EValueMode
    {
        EVM_Absolute = 0,
        EVM_Relative = 1
    }

    public enum ETargetingBase
    {
        ETB_TriggerPawn,
        ETB_SkillPawn
    }

    public enum ESkillEventState
    {
        SES_INITIALIZING = 0,
        SES_WAITING_FOR_DELAY = 1,
        SES_RUNNING = 2,
        SES_FINISHED = 3,
        SES_ABORTED = 4,
        SES_ABORTING = 5
    }

    public enum ETargetMode
    {
        ETM_Never,
        ETM_Auto,
        ETM_RangeCheck
    }

    public enum ESkillTarget
    {
        FST_None = 0,
        FST_Self = 1,
        FST_Ally = 2,
        FST_Enemy = 3,
        FST_Location = 4
    }

    public enum EEmitterOverwrite
    {
        EEO_Auto = 0,
        EEO_SkillPawn = 1,
        EEO_PaintLocation = 2
    }

    public enum EVSSource
    {
        EVSS_TriggerPawn = 0,
        EVSS_TargetPawn = 1,
        EVSS_TriggerMinusTarget = 2,
        EVSS_TargetMinusTrigger = 3
    }

    public enum EStackType
    {
        EST_Disabled,
        EST_ExposeMelee,
        EST_ExposeMagic,
        EST_ExposeRanged,
        EST_InfusedMelee,
        EST_InfusedMagic,
        EST_InfusedRanged,
        EST_ResistantMelee,
        EST_ResistantMagic,
        EST_ResistantRanged,
        EST_Immune,
        EST_ReturnMelee,
        EST_ReturnMagic,
        EST_ReturnRanged,
        EST_ReflectMelee,
        EST_Burning,
        EST_Haunted,
        EST_Doom,
        EST_Scared,
        EST_Distracted,
        EST_Crippled,
        EST_Dazed,
        EST_Hamstring,
        EST_Paralysed,
        EST_Evasive,
        EST_SteadFast,
        EST_Nightmare,
        EST_LifeTap,
        EST_Fury,
        EST_Wound,
        EST_Shock,
        EST_Backfire,
        EST_Acid,
        EST_Corrupted,
        EST_Cursed,
        EST_Decay,
        EST_BloodLink,
        EST_Formation,
        EST_ReflectMagic,
        EST_ReflectRanged,
        EST_EnhancedBody,
        EST_EnhancedMind,
        EST_EnhancedFocus,
        EST_DiminishedBody,
        EST_DiminishedMind,
        EST_DiminishedFocus,
        EST_EnhancedRune,
        EST_EnhancedSpirit,
        EST_EnhancedSoul,
        EST_DiminishedRune,
        EST_DiminishedSpirit,
        EST_DiminishedSoul,
        EST_Flame,
        EST_BloodFury,
        EST_Poison,
        EST_CorrodedResistance,
        EST_TaintedAffinities,
        EST_DecayedAttributes,
        EST_WarriorOpener1,
        EST_WarriorOpener2,
        EST_WarriorOpener3,
        EST_CasterOpener1,
        EST_CasterOpener2,
        EST_CasterOpener3,
        EST_RogueOpener1,
        EST_RogueOpener2,
        EST_RogueOpener3,
        EST_Dispirited,
        EST_Judge,
        EST_Wrath,
        EST_Disconcerted,
        EST_Chosen,
        EST_Venom,
        EST_Incubate,
        EST_Infest,
        EST_TempestTouch,
        EST_BS_Selfbuff,
        EST_BS_Squires_grasp,
        EST_BS_Squires_aura,
        EST_BS_Man_Outlook,
        EST_BS_Man_Shield,
        EST_BS_Knight_Bandage,
        EST_BS_Knight_Pugnac,
        EST_BS_Knight_Rage,
        EST_BS_Knight_Boost,
        EST_BS_Knight_Supp,
        EST_BS_Comm_Mind,
        EST_Consumable_1,
        EST_Consumable_2,
        EST_Consumable_3,
        EST_Expose,
        EST_Infuse,
        EST_Resistant,
        EST_Enhance,
        EST_Diminish,
        EST_Relentless,
        EST_Frozen,
        EST_Physique,
        EST_Morale,
        EST_Concentration,
        EST_Reflect,
        EST_Return,
        EST_None
    }

    public enum EVSCharacterStatistic
    {
        EVSCS_Body = 0,
        EVSCS_Mind = 1,
        EVSCS_Focus = 2,
        EVSCS_Physique = 3,
        EVSCS_Morale = 4,
        EVSCS_Concentration = 5,
        EVSCS_FameLevel = 6,
        EVSCS_PePRank = 7,
        EVSCS_RuneAffinity = 8,
        EVSCS_SpiritAffinity = 9,
        EVSCS_SoulAffinity = 10,
        EVSCS_MeleeResistance = 11,
        EVSCS_RangedResistance = 12,
        EVSCS_MagicResistance = 13,
        EVSCS_MaxHealth = 14,
        EVSCS_PhysiqueRegen = 15,
        EVSCS_PhysiqueDegen = 16,
        EVSCS_MoraleRegen = 17,
        EVSCS_MoraleDegen = 18,
        EVSCS_ConcentrationRegen = 19,
        EVSCS_ConcentrationDegen = 20,
        EVSCS_HealthRegen = 21,
        EVSCS_AttackSpeed = 22,
        EVSCS_MovementSpeed = 23,
        EVSCS_AffinitySum = 24,
        EVSCS_StateSum = 25,
        EVSCS_AttributeSum = 26,
        EVSCS_ResistanceSum = 27,
        EVSCS_Health = 28
    }

    public enum EDuffCondition
    {
        EDC_OnHitBy = 0,
        EDC_OnHitWith = 1,
        EDC_OnMissWith = 2,
        EDC_OnSheatheWeapon = 3,
        EDC_OnDrawWeapon = 4,
        EDC_OnMove = 5,
        EDC_OnDeath = 6,
        EDC_OnUnshift = 7,
        EDC_NoCondition = 8
    }

    public enum EAVEffectState
    {
        AVES_Init = 0,
        AVES_InitialDelay = 1,
        AVES_Intro = 2,
        AVES_Running = 3,
        AVES_Outro = 4,
        AVES_DeInit = 5
    }

    public enum ESkillStartFailure
    {
        SSF_ALLOWED = 0,
        SSF_INVALID_SKILL = 1,
        SSF_FINISHERS_NOT_ALLOWED = 2,
        SSF_OPENERS_NOT_ALLOWED = 3,
        SSF_COOLING_DOWN = 4,
        SSF_DEBUFF_DISABLED = 5,
        SSF_INVALID_PAWN = 6,
        SSF_STILL_EXECUTING_SKILL = 7,
        SSF_FROZEN = 8,
        SSF_DEAD = 9,
        SSF_OUTOFRANGE = 10
    }

    public enum ESkillClassification
    {
        ESC_None = 0,
        ESC_Heal = 1,
        ESC_Damage = 2,
        ESC_AttackSpeedBuff = 3,
        ESC_AttackSpeedDebuff = 4,
        ESC_DamageProtection = 5,
        ESC_SoulProtection = 6,
        ESC_SpiritProtection = 7,
        ESC_RuneProtection = 8,
        ESC_MeleeProtection = 9,
        ESC_RangedProtection = 10,
        ESC_MagicProtection = 11,
        ESC_PhysiqueBuff = 12,
        ESC_PhysiqueDebuff = 13,
        ESC_ConcentrationBuff = 14,
        ESC_ConcentrationDebuff = 15,
        ESC_MoraleBuff = 16,
        ESC_MoraleDebuff = 17,
        ESC_KnockDown = 18,
        ESC_Summon = 19
    }

    public enum EAlterEffectMode
    {
        EAM_Input = 0,
        EAM_Output = 1
    }

    public enum EDirectionDamageMode
    {
        EDDM_Front = 0,
        EDDM_Rear = 1
    }

    public enum ESkillEffectCategory
    {
        ESEC_Player = 0,
        ESEC_NPC = 1,
        ESEC_Event = 2,
        ESEC_Item = 3,
        ESEC_PlayerAV = 4,
        ESEC_NPCAV = 5,
        ESEC_EventAV = 6,
        ESEC_ItemAV = 7,
        ESEC_NPCTypeAV = 8,
        ESEC_Misc = 9,
        ESEC_Test = 10,
        ESEC_TestAV = 11
    }

    public enum EEffectType
    {
        EET_Damage,
        EET_Heal,
        EET_Physique,
        EET_Morale,
        EET_Concentration,
        EET_D1,
        EET_Body,
        EET_Mind,
        EET_Focus,
        EET_D2,
        EET_PhysiqueRegen,
        EET_MoraleRegen,
        EET_ConcentrationRegen,
        EET_HealthRegen,
        EET_D3,
        EET_PhysiqueDegen,
        EET_MoraleDegen,
        EET_ConcentrationDegen,
        EET_D4,
        EET_PePRank,
        EET_D5,
        EET_D6,
        EET_MaxHealth,
        EET_AttackSpeed,
        EET_MeleeResistance,
        EET_RangedResistance,
        EET_MagicResistance,
        EET_SoulAffinity,
        EET_RuneAffinity,
        EET_SpiritAffinity,
        EET_MovementSpeed,
        EET_All,
        EET_None
    }

    public enum EComboType
    {
        ECMT_None = 0,
        ECMT_Normal = 1,
        ECMT_Finisher = 2,
        ECMT_RogueOpener1 = 3,
        ECMT_RogueOpener2 = 4,
        ECMT_RogueOpener3 = 5,
        ECMT_RogueOpener4 = 6,
        ECMT_RogueOpener5 = 7,
        ECMT_RogueOpener6 = 8,
        ECMT_RogueOpener7 = 9,
        ECMT_RogueOpener8 = 10,
        ECMT_RogueOpener9 = 11,
        ECMT_RogueOpener10 = 12,
        ECMT_CasterOpener1 = 13,
        ECMT_CasterOpener2 = 14,
        ECMT_CasterOpener3 = 15,
        ECMT_CasterOpener4 = 16,
        ECMT_CasterOpener5 = 17,
        ECMT_CasterOpener6 = 18,
        ECMT_CasterOpener7 = 19,
        ECMT_CasterOpener8 = 20,
        ECMT_CasterOpener9 = 21,
        ECMT_CasterOpener10 = 22,
        ECMT_WarriorOpener1 = 23,
        ECMT_WarriorOpener2 = 24,
        ECMT_WarriorOpener3 = 25,
        ECMT_WarriorOpener4 = 26,
        ECMT_WarriorOpener5 = 27,
        ECMT_WarriorOpener6 = 28,
        ECMT_WarriorOpener7 = 29,
        ECMT_WarriorOpener8 = 30,
        ECMT_WarriorOpener9 = 31,
        ECMT_WarriorOpener10 = 32
    }

    public enum ETeleportMode
    {
        ETM_Free = 0,
        ETM_SkillUserToTargetFront = 1,
        ETM_SkillUserToTargetBehind = 2,
        ETM_TargetToSkillUserFront = 3,
        ETM_TargetToSkillUserBehind = 4,
        ETM_SkillUserToTargetLocation = 5
    }

    public enum ESkillCategory
    {
        ESC_Player = 0,
        ESC_NPC = 1,
        ESC_Event = 2,
        ESC_Item = 3,
        ESC_Test = 4,
        ESC_Combo = 5
    }

}