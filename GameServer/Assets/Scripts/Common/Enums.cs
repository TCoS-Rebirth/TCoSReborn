namespace Common
{
    public enum ClassArcheType
    {
        Rogue = 0,
        Warrior = 1,
        Spellcaster = 2
    }

    public enum EContentClass
    {
        ECC_NoClass = 0,
        ECC_Rogue = 1,
        ECC_Warrior = 2,
        ECC_Spellcaster = 3,
        ECC_Trickster = 4,
        ECC_SkinShifter = 5,
        ECC_DeathHand = 6,
        ECC_Bloodwarrior = 7,
        ECC_FuryHammer = 8,
        ECC_WrathGuard = 9,
        ECC_RuneMage = 10,
        ECC_VoidSeer = 11,
        ECC_AncestralMage = 12,
        ECC_Gadgeteer = 13,
        ECC_Entertainer = 14,
        ECC_Assassin = 15,
        ECC_ShapeChanger = 16,
        ECC_Consumer = 17,
        ECC_Alchemist = 18,
        ECC_Bodyguard = 19,
        ECC_Flagellant = 20,
        ECC_Visionary = 21,
        ECC_MartialArtist = 22,
        ECC_PossessedOne = 23,
        ECC_FrontMan = 24,
        ECC_Nuker = 25,
        ECC_RuneMaster = 26,
        ECC_Priest = 27,
        ECC_AntiMage = 28,
        ECC_Summoner = 29,
        ECC_Infuser = 30,
        ECC_AnyClass = 31
    }

    public enum NPCRace
    {
        ENR_Human = 0,
        ENR_Daevi = 1,
        ENR_Monster = 2,
        ENR_Arionite = 3,
        ENR_SpeyrFolk = 4,
        ENR_DemonArmy = 5,
        ENR_BotG = 6,
        ENR_ForgeOfWisdom = 7,
        ENR_Ousted = 8,
        ENR_Urvhail = 9,
        ENR_Vhuul = 10,
        ENR_Urgarut = 11,
        ENR_Shunned = 12
    }

    public enum NPCGender
    {
        ENG_Male,
        ENG_Female,
        ENG_Neuter,
        ENG_Hermaphrodite
    }

    public enum ENPCClassType
    {
        CT_HeavyMelee = 0,
        CT_HeavyRanged = 1,
        CT_ModerateMelee = 2,
        CT_ModerateRanged = 3,
        CT_LightMelee = 4,
        CT_LightRanged = 5,
        CT_DOT = 6,
        CT_Healer = 7,
        CT_Slower = 8,
        CT_Buffer = 9,
        CT_Alerter = 10,
        CT_Supporter = 11,
        CT_Rezzer = 12,
        CT_Debuffer = 13,
        CT_Blinder = 14
    }

    public enum EContentOperator
    {
        ECO_Equals,
        ECO_NotEquals,
        ECO_Less,
        ECO_More,
        ECO_EqualOrLess,
        ECO_EqualOrMore,
        ECO_Mask,
        ECO_NotMask
    }

    public enum eZoneWorldTypes
    {
        eZoneWorldTypes_RESERVED_0,
        ZWT_PERSISTENT,
        ZWT_INSTANCED,
        eZoneWorldTypes_RESERVED_3,
        ZWT_SUBINSTANCED
    }

    public enum EAccessLevel
    {
        AL_ArenaPVP,
        AL_ArenaPVE,
        AL_DeadSpellTravel
    }

    public enum EResourceType
    {
        ERT_Wood,
        ERT_Metal
    }

    public enum MapIDs
    {
        LOGIN = 0,
        CHARACTER_SELECTION = 1,
        PT_HAWKSMOUTH = 100,
        PT_ALDENVAULT = 101,
        PT_SORROWMIST = 102,
        PT_BRIGHTVALE = 103,
        PT_GRAVESBOW = 104,
        PT_HOGGSRIDGE = 105,

        QS_ORACLE = 200,
        QS_MINES = 201,
        QS_GREEN = 202,
        QS_ARENA = 203,
        QS_FOUNTAIN = 204,
        QS_STATUE = 205,
        QS_PIT = 206,

        RF_HEARTH = 300,
        RF_STONEDEEP = 301,
        RF_GARMINHOLM = 302,
        RF_DESOLATE = 303,
        RF_SLYWOOD = 304,
        RF_SHORATHMESA = 305,

        MH_NORTH = 400,
        MH_MINES = 401,
        MH_SOUTH = 402,
        MH_SOUREDHILLS = 403,

        DS_RAWHEADLANDING = 600,
        DS_SILENTCHILD = 602,
        DS_HORATUSHOPE = 603,
        DS_ATHENAEUM = 604,

        IA_VAULT_LVL10 = 1000,
        IA_VAULT_LVL15 = 1001,
        IA_VAULT_LVL25 = 1002,
        IA_VAULT_LVL35 = 1003,
        IA_VAULT_LVL45 = 1004,
        IA_VAULT_LVL50 = 1005,
        IA_ANCESTRALFORGE = 1006,

        IA_ARENA_TYPE1_MAP01 = 1500,
        IA_ARENA_TYPE2_Map01 = 1501,
        IA_ARENA_TYPE3_MAP01 = 1502,
        IA_ARENA_TYPE4_MAP01 = 1503,
        IA_ARENA_TYPE5_MAP01 = 1504,
        IA_ARENA_TYPE1_MAP02 = 1505,

        DS_SHARDSHIPQUESTS = 2000,

        DS_SHARDSHIPTRAVEL_SHORT = 3000,
        DS_SHARDSHIPTRAVEL2_SHORT = 3001,

        IA_EXARCHYON = 4000,
        IA_TOTA = 4001,
        IA_CITADELOFAIL = 4002,
        AQ_ORMOBURU = 4003,
        IA_CHAMBEROFWHISPERS = 4004,
        AQ_DAF_CAMP = 4005,
        IA_ANCESTRAL_FORGE = 4006,

        GM_SHARD = 7000,

        LIVE_TEST_SCRIPT_TEST = 8000
    }

    public enum SBLanguage
    {
        English = 0,
        German = 1,
        French = 2,
        Korean = 3,
        Japanese = 4,
        Chinese = 5
    }

    public enum EControllerStates
    {
        CPS_PAWN_NONE = 0,
        CPS_PAWN_ALIVE = 1,
        CPS_PAWN_DEAD = 2,
        CPS_AI_ALERT = 3,
        CPS_AI_AGGRO = 4,
        CPS_AI_FOLLOW = 5,
        CPS_AI_IDLE = 6,
        CPS_AI_REGROUP = 7,
        CPS_MOVE_PAWN = 8,
        CPS_ROTATE_PAWN = 9,
        CPS_PAWN_SITTING = 10,
        CPS_PAWN_FROZEN = 11
    }

    public enum EWeaponTracerType
    {
        EWTT_Custom = 0,
        EWTT_NoTracer = 1,
        EWTT_Sword_sh = 2,
        EWTT_Sword_dh = 3,
        EWTT_Axe_sh = 4,
        EWTT_Axe_dh = 5,
        EWTT_Mace_sh = 6,
        EWTT_Mace_dh = 7,
        EWTT_Dag_sh = 8,
        EWTT_Pole_sh = 9
    }

    public enum ECharacterStatsCharacterState
    {
        CSCS_IDLE = 0,
        CSCS_COMBATREADY = 1,
        CSCS_INCOMBAT = 2,
        ECharacterStatsCharacterState_RESERVED_3 = 3,
        CSCS_SITTING = 4
    }

    public enum SkillLearnResult
    {
        Invalid,
        AlreadyKnown,
        WrongArchetype,
        Success
    }

    public enum AccountPrivilege
    {
        Player,
        Error,
        GM,
        Admin
    }

    public enum ENPC_Category
    {
        ENPC_Human,
        ENPC_Wildlife,
        ENPC_Boss
    }

    public enum CharacterGender
    {
        Male,
        Female
    }

    public enum ECombatMode
    {
        CBM_Idle = 0,
        CBM_Melee = 1,
        CBM_Ranged = 2,
        CBM_Cast = 3
    }

    public enum EPawnEffectType
    {
        EPET_LevelUp,
        EPET_RankUp,
        EPET_QuestCompleted,
        EPET_FadeIn,
        EPET_FadeOut,
        EPET_Visible,
        EPET_Invisible,
        EPET_PetSpawn,
        EPET_PetDestroy,
        EPET_ShapeShift,
        EPET_ShapeUnshift,
        EPET_ArenaTeam0,
        EPET_ArenaTeam1,
        EPET_SimpleCameraShake
    }

    public enum EConversationType
    {
        ECT_None = 0,
        ECT_Free = 1,
        ECT_Greeting = 2,
        ECT_Provide = 3,
        ECT_Mid = 4,
        ECT_Finish = 5,
        ECT_Talk = 6,
        ECT_LastWords = 7,
        ECT_Victory = 8,
        ECT_Thanks = 9,
        ECT_Deliver = 10
    }

    public enum ECollisionType
    {
        COL_NoCollision = 0,
        COL_Colliding = 1,
        COL_Blocking = 2
    }

    public enum EContentEmote
    {
        ECE_None = 0,
        ECE_wave = 1,
        ECE_salute = 2,
        ECE_great = 3,
        ECE_lol = 4,
        ECE_huh = 5,
        ECE_dance = 6,
        ECE_enemies = 7,
        ECE_getready = 8,
        ECE_charge = 9,
        ECE_attack = 10,
        ECE_retreat = 11,
        ECE_follow = 12,
        ECE_wait = 13,
        ECE_comeon = 14,
        ECE_assistance = 15,
        ECE_overhere = 16,
        ECE_backoff = 17,
        ECE_north = 18,
        ECE_east = 19,
        ECE_west = 20,
        ECE_south = 21,
        ECE_flank = 22,
        ECE_goround = 23,
        ECE_no = 24,
        ECE_yes = 25,
        ECE_greet = 26,
        ECE_bye = 27,
        ECE_thanks = 28,
        ECE_pony = 29,
        ECE_pwnie = 30,
        ECE_trade = 31,
        ECE_excuse = 32,
        ECE_waitup = 33,
        ECE_veto = 34,
        ECE_sarcasm = 35,
        ECE_hey = 36,
        ECE_oldskool = 37,
        ECE_outfit = 38,
        ECE_fashionpolice = 39,
        ECE_jazz = 40,
        ECE_clap = 41,
        ECE_kiss = 42,
        ECE_sigh = 43,
        ECE_bored = 44,
        ECE_pain = 45,
        ECE_pst = 46,
        ECE_angry = 47,
        ECE_cry = 48,
        ECE_maniacal = 49,
        ECE_laugh = 50,
        ECE_cough = 51,
        ECE_cheer = 52,
        ECE_whistlehappy = 53,
        ECE_whistleattention = 54,
        ECE_whistlemusic = 55,
        ECE_whistlenote = 56,
        ECE_ahh = 57,
        ECE_gasp = 58,
        ECE_stretch = 59,
        ECE_huf = 60,
        ECE_bah = 61,
        ECE_oracle = 62,
        ECE_battle = 63,
        ECE_praise = 64,
        ECE_mock = 65,
        ECE_attention = 66,
        ECE_death = 67,
        ECE_stop = 68,
        ECE_admireroom = 69,
        ECE_victory = 70,
        ECE_survive = 71,
        ECE_again = 72,
        ECE_try = 73,
        ECE_letsgo = 74,
        ECE_rtfm = 75,
        ECE_unique = 76
    }

    public enum EPawnSound
    {
        EPS_Command_Enemies = 0,
        EPS_Command_GetReady = 1,
        EPS_Command_Charge = 2,
        EPS_Command_Attack = 3,
        EPS_Command_Retreat = 4,
        EPS_Command_Follow = 5,
        EPS_Command_Wait = 6,
        EPS_Command_ComeOn = 7,
        EPS_Command_Assistance = 8,
        EPS_Command_OverHere = 9,
        EPS_Command_BackOff = 10,
        EPS_Command_North = 11,
        EPS_Command_East = 12,
        EPS_Command_West = 13,
        EPS_Command_South = 14,
        EPS_Command_Flank = 15,
        EPS_Command_GoRound = 16,
        EPS_Action = 17,
        EPS_CriticalWound = 18,
        EPS_Death = 19,
        EPS_Interaction_No = 20,
        EPS_Interaction_Yes = 21,
        EPS_Interaction_Greet = 22,
        EPS_Interaction_Bye = 23,
        EPS_Interaction_Thanks = 24,
        EPS_Interaction_PwnieQues = 25,
        EPS_Interaction_PwnieExcl = 26,
        EPS_Interaction_Trade = 27,
        EPS_Interaction_Excuse = 28,
        EPS_Interaction_Wait = 29,
        EPS_Interaction_Veto = 30,
        EPS_Interaction_Sarcasm = 31,
        EPS_Interaction_Hey = 32,
        EPS_Interaction_Oldskool = 33,
        EPS_Interaction_Outfit = 34,
        EPS_Interaction_FashionPolice = 35,
        EPS_Interaction_Jazz = 36,
        EPS_Sound_Clap = 37,
        EPS_Sound_Kiss = 38,
        EPS_Sound_Sigh = 39,
        EPS_Sound_Bored = 40,
        EPS_Sound_Pain = 41,
        EPS_Sound_Pst = 42,
        EPS_Sound_Angry = 43,
        EPS_Sound_Cry = 44,
        EPS_Sound_Maniacal = 45,
        EPS_Sound_Laugh = 46,
        EPS_Sound_Cough = 47,
        EPS_Sound_Cheer = 48,
        EPS_Sound_WhistleHappy = 49,
        EPS_Sound_WhistleAttention = 50,
        EPS_Sound_WhistleMusic = 51,
        EPS_Sound_WhistleNote = 52,
        EPS_Sound_Ahh = 53,
        EPS_Sound_Gasp = 54,
        EPS_Sound_Stretch = 55,
        EPS_Sound_Huf = 56,
        EPS_Sound_Bah = 57,
        EPS_Sound_Dismiss = 58,
        EPS_Taunt_Oracle = 59,
        EPS_Taunt_Battle = 60,
        EPS_Taunt_Praise = 61,
        EPS_Taunt_Mock = 62,
        EPS_Taunt_Attention = 63,
        EPS_Taunt_Death = 64,
        EPS_Taunt_Stop = 65,
        EPS_Taunt_AdmireRoom = 66,
        EPS_Taunt_Victory = 67,
        EPS_Taunt_Survive = 68,
        EPS_Taunt_Again = 69,
        EPS_Taunt_Try = 70,
        EPS_Taunt_LetsGo = 71,
        EPS_Taunt_RTFM = 72,
        EPS_Taunt_Unique = 73,
        EPS_Wound = 74,
        EPS_Goodbye = 75,
        EPS_Greet = 76,
        EPS_Thanks = 77,
        EPS_Yay = 78,
        EPS_Weee = 79,
        EPS_NONE = 80
    }

    public enum eLoginRequestResult
    {
        LRR_NONE,
        LRR_INVALID_REVISION,
        LRR_INVALID_USERNAME,
        LRR_INVALID_PASSWORD,
        LRR_AUTHENTICATE_FAILED,
        LRR_LOGIN_ADD_FAILED,
        LRR_LOGIN_CONNECT_FAILED,
        LRR_INVALID_ACTIVE_CODE,
        LRR_BANNED_ACCOUNT,
        LRR_SUSPENDED_ACCOUNT,
        LRR_DISABLED_ACCOUNT
    }

    public enum MessageStatusCode
    {
        NO_ERROR = 0,
        UNKNOWN_ERROR = 1
    }

    public enum EPawnStates
    {
        PS_NONE,
        PS_ALIVE,
        PS_DEAD
    }

    public enum EPvPTypes
    {
        PVP_None = 0,
        PVP_Off = 1,
        PVP_Guildwars = 2,
        PVP_HouseVSHouse = 3,
        PVP_GuildVSGuild = 4,
        PVP_Deprecated_DO_NOT_USE = 5,
        PVP_FFA = 6
    }

    public enum EWeaponClassification
    {
        EWC_Undetermined,
        EWC_Axe,
        EWC_DoubleHandedAxe,
        EWC_Sword,
        EWC_DoubleHandedSword,
        EWC_Mace,
        EWC_DoubleHandedMace,
        EWC_Hammer,
        EWC_DoubleHandedHammer,
        EWC_Dagger,
        EWC_Bow,
        EWC_Shields
    }

    public enum EDebugDrawFilters
    {
        EDD_Position,
        EDD_Location,
        EDD_Cell,
        EDD_Move,
        EDD_Path,
        EDD_Target,
        EDD_Tactical,
        EDD_Relevant,
        EDD_History,
        EDD_Skill,
        EDD_Threat,
        EDD_Astar,
        EDD_Max
    }

    public enum SBAnimWeaponFlags
    {
        AnimWeapon_None = 0,
        AnimWeapon_Unarmed = 1,
        AnimWeapon_SingleHanded = 2,
        AnimWeapon_DoubleHanded = 3,
        AnimWeapon_DualWielding = 4,
        AnimWeapon_Bow = 5,
        AnimWeapon_Armed = 6,
        AnimWeapon_SingleShield = 7
    }

    public enum SBAnimDirectionFlags
    {
        AnimDirection_None = 0,
        AnimDirection_Forwards = 1,
        AnimDirection_Backwards = 2,
        AnimDirection_Left = 3,
        AnimDirection_Right = 4,
        AnimDirection_Up = 5,
        AnimDirection_Down = 6
    }

    public enum SBAnimActionFlags
    {
        AnimAction_None = 0,
        AnimAction_Aimed = 1,
        AnimAction_Allies = 2,
        AnimAction_Area = 3,
        AnimAction_Carriage = 4,
        AnimAction_Carry = 5,
        AnimAction_Cast = 6,
        AnimAction_Casual = 7,
        AnimAction_Chair = 8,
        AnimAction_Combat = 9,
        AnimAction_Contact = 10,
        AnimAction_Crash = 11,
        AnimAction_Crawl = 12,
        AnimAction_DanceOfBlades = 13,
        AnimAction_Dazed = 14,
        AnimAction_Death = 15,
        AnimAction_DefensiveSkill = 16,
        AnimAction_DoubleScratch = 17,
        AnimAction_DrawWeapon = 18,
        AnimAction_Emote = 19,
        AnimAction_End = 20,
        AnimAction_ExtensiveHack = 21,
        AnimAction_Fall = 22,
        AnimAction_FlickFlack = 23,
        AnimAction_Fly = 24,
        AnimAction_FocusSkill = 25,
        AnimAction_Gallop = 26,
        AnimAction_GetUp = 27,
        AnimAction_Glide = 28,
        AnimAction_Ground = 29,
        AnimAction_Hack = 30,
        AnimAction_HighKick = 31,
        AnimAction_Horse = 32,
        AnimAction_Idle = 33,
        AnimAction_ImpaleSelf = 34,
        AnimAction_Jump = 35,
        AnimAction_Kick = 36,
        AnimAction_KickHooves = 37,
        AnimAction_Knockdown = 38,
        AnimAction_Land = 39,
        AnimAction_LowKick = 40,
        AnimAction_Mid = 41,
        AnimAction_OffensiveSkill = 42,
        AnimAction_Parry = 43,
        AnimAction_Pull = 44,
        AnimAction_Ranged = 45,
        AnimAction_ReverseHack = 46,
        AnimAction_Ride = 47,
        AnimAction_Run = 48,
        AnimAction_Scratch = 49,
        AnimAction_SheatheWeapon = 50,
        AnimAction_Shoot = 51,
        AnimAction_Sit = 52,
        AnimAction_Slash = 53,
        AnimAction_Special = 54,
        AnimAction_Start = 55,
        AnimAction_Step = 56,
        AnimAction_Summon = 57,
        AnimAction_Swing = 58,
        AnimAction_Takeoff = 59,
        AnimAction_Thrust = 60,
        AnimAction_ToIdle = 61,
        AnimAction_Touch = 62,
        AnimAction_Turn = 63,
        AnimAction_Vaylarian = 64,
        AnimAction_Vomit = 65,
        AnimAction_Walk = 66,
        AnimAction_Hit = 67,
        AnimAction_Headbutt = 68,
        AnimAction_Throw = 69,
        AnimAction_Tornado = 70,
        AnimAction_Descend = 71,
        AnimAction_Climb = 72,
        AnimAction_Scared = 73,
        AnimAction_Swim = 74,
        AnimAction_Statue = 75,
        AnimAction_Submerge = 76,
        AnimAction_Emerge = 77,
        AnimAction_Dodge = 78
    }

    public enum EPhysics
    {
        PHYS_None = 0,
        PHYS_Walking = 1,
        PHYS_Falling = 2,
        PHYS_Swimming = 3,
        PHYS_Flying = 4,
        PHYS_Rotating = 5,
        PHYS_Projectile = 6,
        PHYS_Interpolating = 7,
        PHYS_MovingBrush = 8,
        PHYS_Spider = 9,
        PHYS_Trailer = 10,
        PHYS_Ladder = 11,
        PHYS_RootMotion = 12,
        PHYS_Karma = 13,
        PHYS_KarmaRagDoll = 14,
        PHYS_Hovering = 15,
        PHYS_CinMotion = 16,
        PHYS_DragonFlying = 17,
        PHYS_Jumping = 18,
        PHYS_SitGround = 19,
        PHYS_SitChair = 20,
        PHYS_Submerged = 21,
        PHYS_Turret = 22
    }

    public enum EquipmentSlot
    {
        ES_CHEST = 0,
        ES_LEFTGLOVE = 1,
        ES_RIGHTGLOVE = 2,
        ES_PANTS = 3,
        ES_SHOES = 4,
        ES_HELMET = 5,
        ES_HELMETDETAIL = 6,
        ES_RIGHTSHOULDER = 7,
        ES_RIGHTSHOULDERDETAIL = 8,
        ES_LEFTSHOULDER = 9,
        ES_LEFTSHOULDERDETAIL = 10,
        ES_RIGHTGAUNTLET = 11,
        ES_LEFTGAUNTLET = 12,
        ES_CHESTARMOUR = 13,
        ES_CHESTARMOURDETAIL = 14,
        ES_BELT = 15,
        ES_LEFTTHIGH = 16,
        ES_LEFTSHIN = 17,
        ES_MELEEWEAPON = 18,
        ES_RANGEDWEAPON = 19,
        ES_LEFTRING = 20,
        ES_RIGHTRING = 21,
        ES_NECKLACE = 22,
        ES_SHIELD = 23,
        ES_RIGHTTHIGH = 24,
        ES_RIGHTSHIN = 25,
        ES_SLOTCOUNT = 26,
        ES_NO_SLOT = 27
    }

    public enum EItemLocationType
    {
        ILT_Unknown = 0,
        ILT_Inventory = 1,
        ILT_Equipment = 2, //equip and place in slot
        ILT_BodySlot = 3, //equip but dont place (visual, probably for NPC's or preview dye whatever similar)
        ILT_Item = 4,
        ILT_Mail = 5,
        ILT_Auction = 6,
        ILT_Skill = 7,
        ILT_Trade = 8,
        ILT_BodySlotInventory = 9,
        ILT_SendMail = 10,
        ILT_ShopBuy = 11,
        ILT_ShopSell = 12,
        ILT_ShopPaint = 13,
        ILT_ShopCrafting = 14
    }

    public enum EItemType
    {
        IT_BodySlot = 0,
        IT_JewelryRing = 1,
        IT_JewelryNecklace = 2,
        IT_JewelryQualityToken = 3,
        IT_WeaponQualityToken = 4,
        IT_SkillToken = 5,
        IT_QuestItem = 6,
        IT_Trophy = 7,
        IT_ContainerSuitBag = 8,
        IT_ContainerExtraInventory = 9,
        IT_Resource = 10,
        IT_WeaponOneHanded = 11,
        IT_WeaponDoublehanded = 12,
        IT_WeaponDualWielding = 13,
        IT_WeaponRanged = 14,
        IT_WeaponShield = 15,
        IT_ArmorHeadGear = 16,
        IT_ArmorLeftShoulder = 17,
        IT_ArmorRightShoulder = 18,
        IT_ArmorLeftGauntlet = 19,
        IT_ArmorRightGauntlet = 20,
        IT_ArmorChest = 21,
        IT_ArmorBelt = 22,
        IT_ArmorLeftThigh = 23,
        IT_ArmorLeftShin = 24,
        IT_ClothChest = 25,
        IT_ClothLeftGlove = 26,
        IT_ClothRightGlove = 27,
        IT_ClothPants = 28,
        IT_ClothShoes = 29,
        IT_MiscellaneousTickets = 30,
        IT_MiscellaneousKey = 31,
        IT_MiscellaneousLabyrinthKey = 32,
        IT_Recipe = 33,
        IT_ArmorRightThigh = 34,
        IT_ArmorRightShin = 35,
        IT_ItemToken = 36,
        IT_Consumable = 37,
        IT_Broken = 38
    }

    public enum EItemRarity
    {
        IR_Trash,
        IR_Resource,
        IR_Common,
        IR_Uncommon,
        IR_Rare,
        IR_Ancestral,
        IR_Mumian
    }

    public enum EItemChangeNotification
    {
        ICN_Added,
        ICN_Removed,
        ICN_RemovedByType,
        ICN_Moved,
        ICN_Swapped,
        ICN_Stacked,
        ICN_Used,
        ICN_Split,
        ICN_Painted,
        ICN_Attuned
    }

    public enum ECharacterCreationState
    {
        CCS_SELECT_CHARACTER = 0,
        CCS_CREATE_CHARACTER = 1,
        CCS_ENTER_WORLD = 2,
        CCS_PREPARE_UNIVERSE_ENTRY = 3
    }

    public enum EItemAddRemoveType
    {
        IART_Shop = 0,
        IART_Quest = 1,
        IART_Cheat = 2,
        IART_Breakdown = 3
    }

    public enum EItemMoveError
    {
        IME_NoError = 0,
        IME_InternalError = 1,
        IME_InsufficientSpace = 2,
        IME_MoveToSelf = 3,
        IME_ItemIsAttuned = 4,
        IME_NotEquipable = 5,
        IME_CombatReady = 6,
        IME_LevelTooLow = 7,
        IME_EquipmentDataError = 8,
        IME_UnequipShield = 9,
        IME_UnequipMeleeWeapon = 10,
        IME_NotABodySlot = 11,
        IME_BodySlotDataError = 12,
        IME_BodySlotWrongClass = 13,
        IME_BodySlotFakeSkill = 14,
        IME_NotASkillToken = 15,
        IME_IllegalSkillToken = 16,
        IME_DuplicateSkillToken = 17,
        IME_CantMoveSigil = 18,
        IME_MoveToMail = 19
    }

    public enum EScrollingCombatTextType
    {
        ESCT_None = 0,
        ESCT_InputDamage = 1,
        ESCT_OutputDamage = 2,
        ESCT_InputPhysique = 3,
        ESCT_OutputPhysique = 4,
        ESCT_InputMorale = 5,
        ESCT_OutputMorale = 6,
        ESCT_InputConcentration = 7,
        ESCT_OutputConcentration = 8,
        ESCT_InputHeal = 9,
        ESCT_OutputHeal = 10,
        ESCT_InputDuffApply = 11,
        ESCT_OutputDuffApply = 12,
        ESCT_OutputMiss = 13,
        ESCT_OutputImmune = 14,
        ESCT_OutputEvade = 15,
        ESCT_PetError = 16,
        ESCT_PetStandard = 17
    }

    public enum ELootSource
    {
        LS_NPC = 0,
        LS_ILE = 1,
        LS_PVP = 2
    }

    public enum ELootMessageType
    {
        LMT_Received = 0,
        LMT_Rolled = 1,
        LMT_Won = 2,
        LMT_Need = 3,
        LMT_Greed = 4,
        LMT_Pass = 5
    }

    public enum ELootChoice
    {
        LC_NEED = 0,
        LC_GREED = 1,
        LC_PASS = 2,
        LC_LOOT = 3
    }

    public enum ELootMode
    {
        LM_GROUP = 0,
        LM_MASTER = 1,
        LM_FREE_FOR_ALL = 2,
        LM_SINGLE_PLAYER = 3
    }

    public enum EBodySlotMode
    {
        BSM_None = 0,
        BSM_PetSelectSystem = 1,
        BSM_PetControlSystem = 2,
        BSM_SkillUseItems = 3,
        BSM_PlayerUseItems = 4
    }

    public enum ETableType
    {
        ETT_Loot = 0,
        ETT_Scavenge = 1,
        ETT_ShopStock = 2
    }

    public enum EDeadSpellPhase
    {
        EDSP_None = 0,
        EDSP_Intro = 1,
        EDSP_Wait = 2,
        EDSP_Trip = 3,
        EDSP_Outro = 4,
        EDSP_Done = 5
    }

    public enum EServerTradeState
    {
        STS_REQUESTING = 0,
        STS_TRADING = 1,
        STS_OFFERED = 2,
        STS_FINALIZING = 3,
        STS_DONE = 4
    }

    public enum ESigilSlotType
    {
        SST_None = 0,
        SST_Weapon_1 = 1,
        SST_Weapon_2 = 2,
        SST_Weapon_3 = 3,
        SST_Weapon_PVP = 4,
        SST_Weapon_Ranged = 5,
        SST_Armor_1 = 6,
        SST_Armor_2 = 7,
        SST_Armor_3 = 8,
        SST_Jewelry_Exclusive = 9
    }

    public enum IC_BodySlotType
    {
        ICBS_Spirit = 0,
        ICBS_Soul = 1,
        ICBS_Rune = 2
    }

    public enum EAnnotationType
    {
        EAnnotation_None = 0,
        EAnnotation_Walk = 1,
        EAnnotation_Fly = 2,
        EAnnotation_Narrow = 3,
        EAnnotation_Wide = 4,
        EAnnotation_Fall = 5,
        EAnnotation_Jump = 6,
        EAnnotation_ResDef0 = 7,
        EAnnotation_ResDef1 = 8,
        EAnnotation_Script = 9,
        EAnnotation_Patrol = 10,
        EAnnotation_Road = 11,
        EAnnotation_Closed = 12
    }

    public enum EPingState
    {
        PIS_Idle = 0,
        PIS_WaitingPing = 1,
        PIS_WaitingPong = 2
    }

    public enum eSBNetworkRoles
    {
        sbROLE_None = 0,
        sbROLE_Server = 1,
        sbROLE_Proxy = 2,
        sbROLE_DBProxy = 3,
        sbROLE_Client = 4,
        sbROLE_RelevantLod0 = 5,
        sbROLE_RelevantLod1 = 6,
        sbROLE_RelevantLod2 = 7,
        sbROLE_RelevantLod3 = 8,
        sbROLE_ServerLocal = 9,
        sbROLE_ClientLocal = 10
    }

    public enum EQuestArea
    {
        QA_Tech = 0,
        QA_Carnyx = 1,
        QA_Quarterstone = 2,
        QA_Ringfell = 3,
        QA_MountOfHeroes = 4,
        QA_Parliament = 5,
        QA_DeadspellStorm = 6,
        QA_Ancestral = 7
    }

    public enum eTeamRequestResult
    {
        TRR_NONE = 0,
        TRR_ACCEPT = 1,
        TRR_DECLINE = 2,
        TRR_BUSY = 3,
        TRR_FULL = 4,
        TRR_INVITE_SUCCESS = 5,
        TRR_MEMBER_IN_OTHER_TEAM = 6,
        TRR_MEMBER_ON_TRAVELING = 7,
        TRR_SELF_INVITE = 8,
        TRR_INSUFFICIENT_RIGHTS = 9,
        TRR_IGNORED_ME = 10,
        TRR_UNKNOWN_CHARACTER = 11,
        TRR_UNKNOWN_MEMBER_WORLD = 12,
        TRR_UNKNOWN_TEAM = 13,
        TRR_EMPTY_TEAM = 14,
        TRR_CREATE_FAILED = 15,
        TRR_INCORRECT_INVITER = 16,
        TRR_KICK_FAILED = 17,
        TRR_LEAVE_FAILED = 18,
        TRR_DISBAND_FAILED = 19,
        TRR_CHANGE_LEADER_FAILED = 20,
        TRR_CHANGE_LOOTMODE_FAILED = 21,
        TRR_GET_TEAM_INFO_FAILED = 22
    }

    public enum eTeamRemoveMemberCode
    {
        TRMC_NONE = 0,
        TRMC_KICK = 1,
        TRMC_LEAVE = 2,
        TRMC_OFFLINE = 3,
        TRMC_DISBAND = 4
    }

    public enum eGuildRequestResult
    {
        GRR_NONE = 0,
        GRR_BUSY = 1,
        GRR_ACCEPT = 2,
        GRR_DECLINE = 3,
        GRR_INVITE_SUCCESS = 4,
        GRR_MEMBER_IN_OTHER_GUILD = 5,
        GRR_MEMBER_ON_TRAVELING = 6,
        GRR_IGNORED_ME = 7,
        GRR_INSUFFICIENT_RIGHTS = 8,
        GRR_INCORRECT_INVITER = 9,
        GRR_UNKNOWN_CHARACTER = 10,
        GRR_UNKNOWN_MEMBER_WORLD = 11,
        GRR_UNKNOWN_GUILD = 12,
        GRR_EMPTY_GUILD = 13,
        GRR_UNKNOWN_RANK = 14,
        GRR_DEFAULT_RANK = 15,
        GRR_CREATE_FAILED = 16,
        GRR_NOT_ENOUGH_COST = 17,
        GRR_ALREADY_EXIST_GUILD_NAME = 18,
        GRR_DISBAND_FAILED = 19,
        GRR_ADD_MEMBER_FAILED = 20,
        GRR_REMOVE_MEMBER_FAILED = 21,
        GRR_MEMBER_RANK_SET_FAILED = 22,
        GRR_INVITE_FAILED = 23,
        GRR_KICK_FAILED = 24,
        GRR_LEAVE_FAILED = 25,
        GRR_PROMOTE_FAILED = 26,
        GRR_DEMOTE_FAILED = 27,
        GRR_RANK_SET_FAILED = 28,
        GRR_RANK_UPDATE_FAILED = 29,
        GRR_RANK_DELETE_FAILED = 30,
        GRR_RANK_RIGHTS_FAILED = 31,
        GRR_SET_MOTD_FAILED = 32,
        GRR_GET_GUILD_INFO_FAILED = 33,
        GRR_GUILD_DATA_DB_FAILED = 34,
        GRR_GUILD_RANK_DB_FAILED = 35,
        GRR_GUILD_MEMBER_DB_FAILED = 36
    }

    public enum eGuildRemoveMemberCode
    {
        GRMC_NONE = 0,
        GRMC_KICK = 1,
        GRMC_LEAVE = 2,
        GRMC_REMOVE_CHARACTER = 3,
        GRMC_DISBAND = 4
    }

    public enum eFriendsResultCode
    {
        FRC_NONE = 0,
        FRC_ACCEPT = 1,
        FRC_DECLINE = 2,
        FRC_OFFLINE = 3,
        FRC_BUSY = 4,
        FRC_IGNORED_ME = 5,
        FRC_INVITE_SUCCESS = 6,
        FRC_MEMBER_ON_TRAVELING = 7,
        FRC_RELATIONSHIP_ALREADY = 8,
        FRC_UNKNOWN_CHARACTER = 9,
        FRC_INCORRECT_INVITER = 10,
        FRC_ADD_RELATIONSHIP_FAILED = 11,
        FRC_REMOVE_RELATIONSHIP_FAILED = 12,
        FRC_SET_RELATIONSHIP_FAILED = 13,
        FRC_GET_RELATIONSHIP_INFO_FAILED = 14
    }

    public enum eFriendsListFlag
    {
        FLF_UNKNOWN = 0,
        FLF_FRIEND = 1,
        FLF_FRIEND_READY = 2,
        FLF_IGNORE = 3
    }

    public enum EOutOfRangeState
    {
        OORS_InRange = 0,
        OORS_TooFar = 1,
        OORS_TooClose = 2
    }

    public enum EWeaponCategory
    {
        EWC_None = 0,
        EWC_Melee = 1,
        EWC_Ranged = 2,
        EWC_Unarmed = 3,
        EWC_MeleeOrUnarmed = 4
    }

    public enum EAppMainWeaponType
    {
        EMW_Undetermined = 0,
        EMW_SingleHanded = 1,
        EMW_DoubleHanded = 2,
        EMW_DualWielding = 3,
        EMW_Ranged = 4
    }

    public enum EGameChatRanges
    {
        GCR_LOCAL = 0,
        GCR_WORLD = 1,
        GCR_TRADE = 2,
        GCR_TEAM = 3,
        GCR_GUILD = 4,
        GCR_PRIVATE = 5,
        GCR_COMBAT = 6,
        GCR_SYSTEM = 7,
        GCR_BROADCAST = 8
    }

    public enum ERadialMenuOptions
    {
        NULL = -1,
        RMO_MAIN = 0,
        RMO_STATS = 1,
        RMO_NOTHING = 2,
        RMO_HELP = 3,
        RMO_USE = 4,
        RMO_OPENDOOR = 5,
        RMO_SIT = 6,
        RMO_TRADE = 7,
        RMO_LOOT = 8,
        RMO_INTERACT = 9,
        RMO_CONVERSATION = 10,
        RMO_TEAM_KICK = 11,
        RMO_TEAM_INVITE = 12,
        RMO_FRIEND_INVITE = 13,
        RMO_GUILD_INVITE = 14,
        RMO_TRAVEL = 15,
        RMO_MAIL = 16,
        RMO_ARENA = 17,
        RMO_MINIGAME_INVITE = 18,
        RMO_WHISPER = 19,
        RMO_SHOP_BUY_FORGE = 20,
        RMO_SHOP_BUY_TAILOR = 21,
        RMO_SHOP_BUY_SOUL = 22,
        RMO_SHOP_BUY_RUNE = 23,
        RMO_SHOP_BUY_SPIRIT = 24,
        RMO_SHOP_BUY_TAVERN = 25,
        RMO_SHOP_BUY_GENERAL = 26,
        RMO_SHOP_CRAFT_FORGE = 27,
        RMO_SHOP_CRAFT_SOUL = 28,
        RMO_SHOP_CRAFT_RUNE = 29,
        RMO_SHOP_CRAFT_SPIRIT = 30,
        RMO_SHOP_CRAFT_TAVERN = 31,
        RMO_SHOP_CRAFT_GENERAL = 32,
        RMO_SHOP_PAINTING = 33,
        RMO_SHOP_SIGIL_FORGING = 34,
        RMO_SHOP_DRAGON = 35,
        RMO_MAX = 36
    }

    public enum ENPCMovementFlags
    {
        ENMF_Normal = 0,
        ENMF_Walking = 1,
        ENMF_Backwards = 2,
        ENPCMovementFlags_RESERVED_3 = 3,
        ENMF_Strafing = 4,
        ENPCMovementFlags_RESERVED_5 = 5,
        ENPCMovementFlags_RESERVED_6 = 6,
        ENPCMovementFlags_RESERVED_7 = 7,
        ENMF_Jumping = 8,
        ENPCMovementFlags_RESERVED_9 = 9,
        ENPCMovementFlags_RESERVED_10 = 10,
        ENPCMovementFlags_RESERVED_11 = 11,
        ENPCMovementFlags_RESERVED_12 = 12,
        ENPCMovementFlags_RESERVED_13 = 13,
        ENPCMovementFlags_RESERVED_14 = 14,
        ENPCMovementFlags_RESERVED_15 = 15,
        ENMF_Crouching = 16,
        ENPCMovementFlags_RESERVED_17 = 17,
        ENPCMovementFlags_RESERVED_18 = 18,
        ENPCMovementFlags_RESERVED_19 = 19,
        ENPCMovementFlags_RESERVED_20 = 20,
        ENPCMovementFlags_RESERVED_21 = 21,
        ENPCMovementFlags_RESERVED_22 = 22,
        ENPCMovementFlags_RESERVED_23 = 23,
        ENPCMovementFlags_RESERVED_24 = 24,
        ENPCMovementFlags_RESERVED_25 = 25,
        ENPCMovementFlags_RESERVED_26 = 26,
        ENPCMovementFlags_RESERVED_27 = 27,
        ENPCMovementFlags_RESERVED_28 = 28,
        ENPCMovementFlags_RESERVED_29 = 29,
        ENPCMovementFlags_RESERVED_30 = 30,
        ENPCMovementFlags_RESERVED_31 = 31,
        ENMF_Sitting = 32,
        ENPCMovementFlags_RESERVED_33 = 33,
        ENPCMovementFlags_RESERVED_34 = 34,
        ENPCMovementFlags_RESERVED_35 = 35,
        ENPCMovementFlags_RESERVED_36 = 36,
        ENPCMovementFlags_RESERVED_37 = 37,
        ENPCMovementFlags_RESERVED_38 = 38,
        ENPCMovementFlags_RESERVED_39 = 39,
        ENPCMovementFlags_RESERVED_40 = 40,
        ENPCMovementFlags_RESERVED_41 = 41,
        ENPCMovementFlags_RESERVED_42 = 42,
        ENPCMovementFlags_RESERVED_43 = 43,
        ENPCMovementFlags_RESERVED_44 = 44,
        ENPCMovementFlags_RESERVED_45 = 45,
        ENPCMovementFlags_RESERVED_46 = 46,
        ENPCMovementFlags_RESERVED_47 = 47,
        ENPCMovementFlags_RESERVED_48 = 48,
        ENPCMovementFlags_RESERVED_49 = 49,
        ENPCMovementFlags_RESERVED_50 = 50,
        ENPCMovementFlags_RESERVED_51 = 51,
        ENPCMovementFlags_RESERVED_52 = 52,
        ENPCMovementFlags_RESERVED_53 = 53,
        ENPCMovementFlags_RESERVED_54 = 54,
        ENPCMovementFlags_RESERVED_55 = 55,
        ENPCMovementFlags_RESERVED_56 = 56,
        ENPCMovementFlags_RESERVED_57 = 57,
        ENPCMovementFlags_RESERVED_58 = 58,
        ENPCMovementFlags_RESERVED_59 = 59,
        ENPCMovementFlags_RESERVED_60 = 60,
        ENPCMovementFlags_RESERVED_61 = 61,
        ENPCMovementFlags_RESERVED_62 = 62,
        ENPCMovementFlags_RESERVED_63 = 63,
        ENMF_MovingTurn = 64,
        ENPCMovementFlags_RESERVED_65 = 65,
        ENPCMovementFlags_RESERVED_66 = 66,
        ENPCMovementFlags_RESERVED_67 = 67,
        ENPCMovementFlags_RESERVED_68 = 68,
        ENPCMovementFlags_RESERVED_69 = 69,
        ENPCMovementFlags_RESERVED_70 = 70,
        ENPCMovementFlags_RESERVED_71 = 71,
        ENPCMovementFlags_RESERVED_72 = 72,
        ENPCMovementFlags_RESERVED_73 = 73,
        ENPCMovementFlags_RESERVED_74 = 74,
        ENPCMovementFlags_RESERVED_75 = 75,
        ENPCMovementFlags_RESERVED_76 = 76,
        ENPCMovementFlags_RESERVED_77 = 77,
        ENPCMovementFlags_RESERVED_78 = 78,
        ENPCMovementFlags_RESERVED_79 = 79,
        ENPCMovementFlags_RESERVED_80 = 80,
        ENPCMovementFlags_RESERVED_81 = 81,
        ENPCMovementFlags_RESERVED_82 = 82,
        ENPCMovementFlags_RESERVED_83 = 83,
        ENPCMovementFlags_RESERVED_84 = 84,
        ENPCMovementFlags_RESERVED_85 = 85,
        ENPCMovementFlags_RESERVED_86 = 86,
        ENPCMovementFlags_RESERVED_87 = 87,
        ENPCMovementFlags_RESERVED_88 = 88,
        ENPCMovementFlags_RESERVED_89 = 89,
        ENPCMovementFlags_RESERVED_90 = 90,
        ENPCMovementFlags_RESERVED_91 = 91,
        ENPCMovementFlags_RESERVED_92 = 92,
        ENPCMovementFlags_RESERVED_93 = 93,
        ENPCMovementFlags_RESERVED_94 = 94,
        ENPCMovementFlags_RESERVED_95 = 95,
        ENPCMovementFlags_RESERVED_96 = 96,
        ENPCMovementFlags_RESERVED_97 = 97,
        ENPCMovementFlags_RESERVED_98 = 98,
        ENPCMovementFlags_RESERVED_99 = 99,
        ENPCMovementFlags_RESERVED_100 = 100,
        ENPCMovementFlags_RESERVED_101 = 101,
        ENPCMovementFlags_RESERVED_102 = 102,
        ENPCMovementFlags_RESERVED_103 = 103,
        ENPCMovementFlags_RESERVED_104 = 104,
        ENPCMovementFlags_RESERVED_105 = 105,
        ENPCMovementFlags_RESERVED_106 = 106,
        ENPCMovementFlags_RESERVED_107 = 107,
        ENPCMovementFlags_RESERVED_108 = 108,
        ENPCMovementFlags_RESERVED_109 = 109,
        ENPCMovementFlags_RESERVED_110 = 110,
        ENPCMovementFlags_RESERVED_111 = 111,
        ENPCMovementFlags_RESERVED_112 = 112,
        ENPCMovementFlags_RESERVED_113 = 113,
        ENPCMovementFlags_RESERVED_114 = 114,
        ENPCMovementFlags_RESERVED_115 = 115,
        ENPCMovementFlags_RESERVED_116 = 116,
        ENPCMovementFlags_RESERVED_117 = 117,
        ENPCMovementFlags_RESERVED_118 = 118,
        ENPCMovementFlags_RESERVED_119 = 119,
        ENPCMovementFlags_RESERVED_120 = 120,
        ENPCMovementFlags_RESERVED_121 = 121,
        ENPCMovementFlags_RESERVED_122 = 122,
        ENPCMovementFlags_RESERVED_123 = 123,
        ENPCMovementFlags_RESERVED_124 = 124,
        ENPCMovementFlags_RESERVED_125 = 125,
        ENPCMovementFlags_RESERVED_126 = 126,
        ENPCMovementFlags_RESERVED_127 = 127,
        ENMF_Submerged = 128
    }

    public enum ESkillTokenEffect
    {
        SSE_None = 0,
        SSE_Target_MaxTargets = 1,
        SSE_Target_PaintLocationMinDistance = 2,
        SSE_Target_PaintLocationMaxDistance = 3,
        SSE_Target_MaxRadius = 4,
        SSE_Damage_RearFactor = 5,
        SSE_Damage_AbsoluteMinimum = 6,
        SSE_Damage_AbsoluteMaximum = 7,
        SSE_Damage_ConstantMinimum = 8,
        SSE_Damage_ConstantMaximum = 9,
        SSE_Damage_CharStatMinimumMultiplier = 10,
        SSE_Damage_CharStatMaximumMultiplier = 11,
        SSE_Damage_TargetCountMinimumMultiplier = 12,
        SSE_Damage_TargetCountMaximumMultiplier = 13,
        SSE_Health_AbsoluteMinimum = 14,
        SSE_Health_AbsoluteMaximum = 15,
        SSE_Health_ConstantMinimum = 16,
        SSE_Health_ConstantMaximum = 17,
        SSE_Health_CharStatMinimumMultiplier = 18,
        SSE_Health_CharStatMaximumMultiplier = 19,
        SSE_Health_TargetCountMinimumMultiplier = 20,
        SSE_Health_TargetCountMaximumMultiplier = 21,
        SSE_StatePhysique_AbsoluteMinimum = 22,
        SSE_StatePhysique_AbsoluteMaximum = 23,
        SSE_StatePhysique_ConstantMinimum = 24,
        SSE_StatePhysique_ConstantMaximum = 25,
        SSE_StatePhysique_CharStatMinimumMultiplier = 26,
        SSE_StatePhysique_CharStatMaximumMultiplier = 27,
        SSE_StatePhysique_TargetCountMinimumMultiplier = 28,
        SSE_StatePhysique_TargetCountMaximumMultiplier = 29,
        SSE_StateMorale_AbsoluteMinimum = 30,
        SSE_StateMorale_AbsoluteMaximum = 31,
        SSE_StateMorale_ConstantMinimum = 32,
        SSE_StateMorale_ConstantMaximum = 33,
        SSE_StateMorale_CharStatMinimumMultiplier = 34,
        SSE_StateMorale_CharStatMaximumMultiplier = 35,
        SSE_StateMorale_TargetCountMinimumMultiplier = 36,
        SSE_StateMorale_TargetCountMaximumMultiplier = 37,
        SSE_StateConcentration_AbsoluteMinimum = 38,
        SSE_StateConcentration_AbsoluteMaximum = 39,
        SSE_StateConcentration_ConstantMinimum = 40,
        SSE_StateConcentration_ConstantMaximum = 41,
        SSE_StateConcentration_CharStatMinimumMultiplier = 42,
        SSE_StateConcentration_CharStatMaximumMultiplier = 43,
        SSE_StateConcentration_TargetCountMinimumMultiplier = 44,
        SSE_StateConcentration_TargetCountMaximumMultiplier = 45,
        SSE_AttributeBody_AbsoluteMinimum = 46,
        SSE_AttributeBody_AbsoluteMaximum = 47,
        SSE_AttributeBody_ConstantMinimum = 48,
        SSE_AttributeBody_ConstantMaximum = 49,
        SSE_AttributeBody_CharStatMinimumMultiplier = 50,
        SSE_AttributeBody_CharStatMaximumMultiplier = 51,
        SSE_AttributeBody_TargetCountMinimumMultiplier = 52,
        SSE_AttributeBody_TargetCountMaximumMultiplier = 53,
        SSE_AttributeMind_AbsoluteMinimum = 54,
        SSE_AttributeMind_AbsoluteMaximum = 55,
        SSE_AttributeMind_ConstantMinimum = 56,
        SSE_AttributeMind_ConstantMaximum = 57,
        SSE_AttributeMind_CharStatMinimumMultiplier = 58,
        SSE_AttributeMind_CharStatMaximumMultiplier = 59,
        SSE_AttributeMind_TargetCountMinimumMultiplier = 60,
        SSE_AttributeMind_TargetCountMaximumMultiplier = 61,
        SSE_AttributeFocus_AbsoluteMinimum = 62,
        SSE_AttributeFocus_AbsoluteMaximum = 63,
        SSE_AttributeFocus_ConstantMinimum = 64,
        SSE_AttributeFocus_ConstantMaximum = 65,
        SSE_AttributeFocus_CharStatMinimumMultiplier = 66,
        SSE_AttributeFocus_CharStatMaximumMultiplier = 67,
        SSE_AttributeFocus_TargetCountMinimumMultiplier = 68,
        SSE_AttributeFocus_TargetCountMaximumMultiplier = 69,
        SSE_ResistanceMelee_AbsoluteMinimum = 70,
        SSE_ResistanceMelee_AbsoluteMaximum = 71,
        SSE_ResistanceMelee_ConstantMinimum = 72,
        SSE_ResistanceMelee_ConstantMaximum = 73,
        SSE_ResistanceMelee_CharStatMinimumMultiplier = 74,
        SSE_ResistanceMelee_CharStatMaximumMultiplier = 75,
        SSE_ResistanceMelee_TargetCountMinimumMultiplier = 76,
        SSE_ResistanceMelee_TargetCountMaximumMultiplier = 77,
        SSE_ResistanceRanged_AbsoluteMinimum = 78,
        SSE_ResistanceRanged_AbsoluteMaximum = 79,
        SSE_ResistanceRanged_ConstantMinimum = 80,
        SSE_ResistanceRanged_ConstantMaximum = 81,
        SSE_ResistanceRanged_CharStatMinimumMultiplier = 82,
        SSE_ResistanceRanged_CharStatMaximumMultiplier = 83,
        SSE_ResistanceRanged_TargetCountMinimumMultiplier = 84,
        SSE_ResistanceRanged_TargetCountMaximumMultiplier = 85,
        SSE_ResistanceMagic_AbsoluteMinimum = 86,
        SSE_ResistanceMagic_AbsoluteMaximum = 87,
        SSE_ResistanceMagic_ConstantMinimum = 88,
        SSE_ResistanceMagic_ConstantMaximum = 89,
        SSE_ResistanceMagic_CharStatMinimumMultiplier = 90,
        SSE_ResistanceMagic_CharStatMaximumMultiplier = 91,
        SSE_ResistanceMagic_TargetCountMinimumMultiplier = 92,
        SSE_ResistanceMagic_TargetCountMaximumMultiplier = 93,
        SSE_AffinitySoul_AbsoluteMinimum = 94,
        SSE_AffinitySoul_AbsoluteMaximum = 95,
        SSE_AffinitySoul_ConstantMinimum = 96,
        SSE_AffinitySoul_ConstantMaximum = 97,
        SSE_AffinitySoul_CharStatMinimumMultiplier = 98,
        SSE_AffinitySoul_CharStatMaximumMultiplier = 99,
        SSE_AffinitySoul_TargetCountMinimumMultiplier = 100,
        SSE_AffinitySoul_TargetCountMaximumMultiplier = 101,
        SSE_AffinityRune_AbsoluteMinimum = 102,
        SSE_AffinityRune_AbsoluteMaximum = 103,
        SSE_AffinityRune_ConstantMinimum = 104,
        SSE_AffinityRune_ConstantMaximum = 105,
        SSE_AffinityRune_CharStatMinimumMultiplier = 106,
        SSE_AffinityRune_CharStatMaximumMultiplier = 107,
        SSE_AffinityRune_TargetCountMinimumMultiplier = 108,
        SSE_AffinityRune_TargetCountMaximumMultiplier = 109,
        SSE_AffinitySpirit_AbsoluteMinimum = 110,
        SSE_AffinitySpirit_AbsoluteMaximum = 111,
        SSE_AffinitySpirit_ConstantMinimum = 112,
        SSE_AffinitySpirit_ConstantMaximum = 113,
        SSE_AffinitySpirit_CharStatMinimumMultiplier = 114,
        SSE_AffinitySpirit_CharStatMaximumMultiplier = 115,
        SSE_AffinitySpirit_TargetCountMinimumMultiplier = 116,
        SSE_AffinitySpirit_TargetCountMaximumMultiplier = 117,
        SSE_EventDuff_Duration = 118,
        SSE_ReturnReflect_Multiplier = 119,
        SSE_Misc_Cooldown = 120,
        SSE_Chain_MaxJumps = 121,
        SSE_EnhanceHealingInputAlteringEffects = 122,
        SSE_EnhanceHealingOutputAlteringEffects = 123,
        SSE_EnhanceDamageInputAlteringEffects = 124,
        SSE_EnhanceDamageOutputAlteringEffects = 125,
        SSE_EnhancePhysiqueInputAlteringEffects = 126,
        SSE_EnhancePhysiqueOutputAlteringEffects = 127,
        SSE_EnhanceMoraleInputAlteringEffects = 128,
        SSE_EnhanceMoraleOutputAlteringEffects = 129,
        SSE_EnhanceConcentrationInputAlteringEffects = 130,
        SSE_EnhanceConcentrationOutputAlteringEffects = 131,
        SSE_EnhanceAllStateInputAlteringEffects = 132,
        SSE_EnhanceAllStateOutputAlteringEffects = 133
    }

    //Valshaaran : custom enum
    public enum EILECategory { ILE_Base, ILE_Chair, ILE_Mailbox, ILE_Quest, ILE_Shop };
}