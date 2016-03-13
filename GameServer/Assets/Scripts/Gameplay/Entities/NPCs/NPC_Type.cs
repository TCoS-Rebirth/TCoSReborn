using System;
using System.Collections.Generic;
using Common;
using Database.Static;
using Gameplay.Conversations;
using Gameplay.Loot;
using Gameplay.Skills;
using Gameplay.Skills.Effects;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Gameplay.Entities.NPCs
{
    [Serializable]
    public class NPC_Type : ScriptableObject
    {
        public enum ConsolidatedNPCType
        {
            NPC_NPNC,
            NPC_Boss,
            NPC_Character,
            NPC_Civilian,
            NPC_Class,
            NPC_Humanoid,
            NPC_Monster,
            NPC_MonsterClass,
            NPC_Quest
        }

        public float AccelRate;
        public float AirControl;
        public float AirMinSpeed;
        public float AirSpeed;
        public bool Arena;
        public bool bAlignToFloorPitch;
        public bool bAlignToFloorRoll;
        public bool bForceAttachmentUpdates;

        [Header("Spawning")] public int BossPriority;

        public bool CanRest;
        public bool CanStrafe;
        public bool CanWalkBackwards;
        public ENPC_Category Category;
        public int ClassRank;

        [FormerlySerializedAs("classTypes")] public List<ENPCClassType> ClassTypes = new List<ENPCClassType>();

        public float ClimbSpeed;
        public float CombatSpeed;
        public float CorpseTimeout;
        public float CreditMultiplier;

        [Header("CharacterAppearance")]
        //public NPC_Appearance CharacterAppearance; //irrelevant
        public List<AudioVisualSkillEffect> Effects;

        [Header("Sheet")] public int FameLevel;

        public float GroundSpeed;
        public bool IndividualKillCredit;

        [ReadOnly] public ConsolidatedNPCType internalType;

        [Header("Combat")] public bool Invulnerable;

        public float JumpSpeed;

        [Header("Basics")] public string LongName;

        //public Content_Inventory Inventory; //irrelevant
        public List<LootTable> Loot;

        [Header("Movement")] public byte Movement;

        public string Note;
        public EContentClass NPCClassClassification;
        public int PePRank;
        //public List<string> temporaryTopicsNames = new List<string>();
        public List<SBResource> QuestTopics;

        [Header("Internal")] [ReadOnly] public int resourceID;

        public string ShortName;
        //public byte NPCClassClassification;
        public SkillDeck SkillDeck;
        public NPC_StatTable Stats;
        public float StrollSpeed;

        [SerializeField] public Taxonomy TaxonomyFaction;

        //public NPC_AttackSheet AttackSheet; //needed? HACK: build own AI
        public string temporaryAttackSheetName;

        [Header("Items")]
        //public  NPC_Equipment Equipment; //irrelevant, except for weapons maybe, just work around that via skill required weapon or similar
        public string temporaryEquipmentName;

        public List<string> temporaryLootTableaNames = new List<string>();
        //public Shop_Base Shop; //TODO: later extract shop
        public string temporaryShopBaseName;
        public float TerminalVelocity;

        [Header("Conversation")] public List<SBResource> Topics;

        [Header("Services")] public bool Travel;

        //public List<string> temporaryQuestTopicsName = new List<string>();
        public bool TriggersKilledHook;
        public float TurnSpeed;
        public float WaterSpeed;

        public void InitializeStats(int aFameLevel, int aPePRank, out int oMaxHp, out int oLevelHp, out int oBody, out int oMind, out int oFocus)
        {
            if (Stats != null)
            {
                oMaxHp = Stats.GetBaseHitpoints(aFameLevel);
                oLevelHp = Stats.GetHitpointsPerLevel(aFameLevel);
                oBody = Stats.GetBody(aFameLevel);
                oMind = Stats.GetMind(aFameLevel);
                oFocus = Stats.GetFocus(aFameLevel);
            }
            else
            {
                oMaxHp = 100;
                oLevelHp = 10;
                oBody = 7 + aFameLevel/9;
                oMind = 7 + aFameLevel/9;
                oFocus = 7 + aFameLevel/9;
            }
        }

    }
}