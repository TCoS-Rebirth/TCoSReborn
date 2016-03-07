using System.Collections.Generic;
using Common;
using Database.Dynamic.Internal;
using Database.Static;
using Gameplay.Conversations;
using Gameplay.Entities.NPCs;
using Gameplay.Entities.Players;
using Gameplay.Items;
using Gameplay.Quests;
using Gameplay.Skills;
using Gameplay.Skills.Events;
using Network;
using UnityEngine;
using Utility;
using World;

namespace Gameplay.Entities
{
    public sealed class PlayerCharacter : Character
    {
        byte _moveFrame;

        public DBPlayerCharacter dbRef;

        #region Guild

        public PlayerGuild Guild;

        #endregion

        #region Team

        public PlayerTeam Team;

        #endregion

        /// <summary>
        ///     used to sync player character movement
        /// </summary>
        public byte MoveFrame
        {
            get { return _moveFrame; }
            set { _moveFrame = value; }
        }

        #region Factory

        /// <summary>
        ///     Creates a new player character instance and initializes it
        /// </summary>
        public static PlayerCharacter Create(PlayerInfo p, DBPlayerCharacter dbc)
        {
            var go = new GameObject("Player_" + dbc.Name);
            var pc = go.AddComponent<PlayerCharacter>();
            pc.RetrieveRelevanceID();
            pc.Name = dbc.Name;
            go.transform.position = dbc.Position;
            go.transform.rotation = Quaternion.Euler(dbc.Rotation);
            pc.Owner = p;
            pc.dbRef = dbc;
            pc.itemManager = new Player_ItemManager(pc.OnInventoryItemChanged);
            pc.SetupFromDBRef();
            pc.SetupCollision();
            pc.InitializeStats();
            return pc;
        }

        #endregion

        void SetupFromDBRef()
        {
            ArcheType = (ClassArcheType) dbRef.ArcheType;
            Appearance = dbRef.Appearance;
            Body = dbRef.BodyMindFocus[0];
            Mind = dbRef.BodyMindFocus[1];
            Focus = dbRef.BodyMindFocus[2];
            ExtraBodyPoints = (byte) dbRef.ExtraBodyMindFocusAttributePoints[0];
            //{ can be left out too (calculate from body,mind,focus + levelprogression->leftover-points)
            ExtraMindPoints = (byte) dbRef.ExtraBodyMindFocusAttributePoints[1];
            ExtraFocusPoints = (byte) dbRef.ExtraBodyMindFocusAttributePoints[2];
            Faction = GameData.Get.factionDB.GetFaction(dbRef.Faction);
            FameLevel = dbRef.FamePep[0];
            PepRank = dbRef.FamePep[1];
            MaxHealth = dbRef.HealthMaxHealth[1]; //TODO: calculate value instead, later (from level & items etc)
            Health = dbRef.HealthMaxHealth[0];
            LastZoneID = (MapIDs) dbRef.LastZoneID;
            Money = dbRef.Money;
            PawnState = (EPawnStates) dbRef.PawnState;
            itemManager.LoadItems(dbRef.Items);
            for (var i = 0; i < dbRef.Skills.Count; i++)
            {
                var s = GameData.Get.skillDB.GetSkill(dbRef.Skills[i].ResourceId);
                if (s != null)
                {
                    s.SigilSlots = dbRef.Skills[i].SigilSlots;
                    Skills.Add(s);
                }
            }
            ActiveSkillDeck = ScriptableObject.CreateInstance<SkillDeck>();
            ActiveSkillDeck.LoadForPlayer(dbRef.SerializedSkillDeck, this);

            QuestData = ScriptableObject.CreateInstance<QuestDataContainer>();
            QuestData.LoadForPlayer(dbRef.QuestTargets, this);
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "Player.psd");
        }

        #region Duffs

        protected override void OnDuffsChanged()
        {
            base.OnDuffsChanged();
            SendToClient(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_UPDATEDUFFS(this, Duffs));
        }

        #endregion

        #region Client specific

        /// <summary>
        ///     Sends a Message directly to the client associated with this player instance (bypasses relevance checks)
        /// </summary>
        public void SendToClient(Message m)
        {
            Owner.Connection.SendQueued(m);
        }

        #endregion

        #region InternalInfo

        public PlayerInfo Owner;
        public bool DebugMode;

        #endregion

        #region Movement

        /// <summary>
        ///     Initiated from the client, replicates client movement to this server character instance
        /// </summary>
        public void ReplicateMovement(Vector3 pos, Vector3 vel, byte frameNr)
        {
            Position = pos;
            Velocity = vel;
            MoveFrame = frameNr;
            var m = PacketCreator.S2R_PLAYERPAWN_MOVE(this);
            BroadcastRelevanceMessage(m);
        }

        /// <summary>
        ///     Initiated from the client, replicates client movement to this server character instance TODO maybe check physics to
        ///     prevent hacking?
        /// </summary>
        public void ReplicateMovementWithPhysics(Vector3 pos, Vector3 vel, EPhysics physics, byte frameNr)
        {
            Position = pos;
            Velocity = vel;
            Physics = physics;
            MoveFrame = frameNr;
            var m = PacketCreator.S2R_PLAYERPAWN_MOVE(this);
            BroadcastRelevanceMessage(m);
        }

        /// <summary>
        ///     Initiated from the client, replicates client character rotation to this server character instance
        /// </summary>
        public void ReplicateRotation(Quaternion rot)
        {
            Rotation = rot;
            BroadcastRelevanceMessage(PacketCreator.S2R_GAME_PLAYERPAWN_UPDATEROTATION(this));
        }

        /// <summary>
        ///     <see cref="Entity.TeleportTo" />
        /// </summary>
        public override void TeleportTo(Vector3 newPos, Quaternion newRot)
        {
            base.TeleportTo(newPos, newRot);
            SendToClient(PacketCreator.S2R_GAME_PAWN_SV2CLREL_TELEPORTTO(this));
        }

        /// <summary>
        ///     <see cref="Character.SetMoveSpeed" />
        /// </summary>
        public override void SetMoveSpeed(int newSpeed)
        {
            base.SetMoveSpeed(newSpeed);
            SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMOVEMENTSPEED(this));
        }

        #endregion

        #region Stats

        /// <summary>
        ///     <see cref="Character.SetPawnState" />
        /// </summary>
        public override void SetPawnState(EPawnStates newState)
        {
            base.SetPawnState(newState);
            SendToClient(PacketCreator.S2R_GAME_PAWN_SV2CLREL_UPDATENETSTATE(this));
        }

        /// <summary>
        ///     Notifies the client through combat message log, Sheates the weapon and sets the combatstate to idle TODO cleanup
        /// </summary>
        /// <param name="source"></param>
        protected override void OnDiedThroughDamage(Character source)
        {
            base.OnDiedThroughDamage(source);
            CombatMode = ECombatMode.CBM_Idle;
            SheatheWeapon();
            SendToClient(PacketCreator.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEDEATH(source));
        }

        protected override void OnHealthChanged()
        {
            base.OnHealthChanged();
            SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEHEALTH(this));
        }

        protected override void OnMaxHealthChanged()
        {
            base.OnMaxHealthChanged();
            SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMAXHEALTH(this));
        }

        protected override void OnPhysiqueChanged()
        {
            base.OnPhysiqueChanged();
            SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEPHYSIQUE(this));
        }

        protected override void OnMoraleChanged()
        {
            base.OnMoraleChanged();
            SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMORALE(this));
        }

        protected override void OnConcentrationChanged()
        {
            base.OnConcentrationChanged();
            SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATECONCENTRATION(this));
        }

        //protected override void OnMovementSpeedBonusChanged()
        //{
        //    base.OnMovementSpeedBonusChanged();
        //    SendToClient(PacketCreator.S2R_GAME_CHARACTERSTATS_SV2CLREL_UPDATEMOVEMENTSPEED(this));
        //}

        protected override void OnLeavingZone(Zone z)
        {
            base.OnLeavingZone(z);
            relevantObjects.Clear();
        }

        #endregion

        #region Personality/CharacterAppearance

        public CharacterAppearance Appearance;

        public byte[] GetPackedLOD(int index)
        {
            switch (index)
            {
                case 0:
                {
                    var lod = new LODHelper(13);
                    lod.Add(Appearance.Voice, 8);
                    lod.Add(0, 8);
                    lod.Add(0, 8);
                    lod.Add(0, 4); //4th tattoo
                    lod.Add(Appearance.TattooRight, 4);
                    lod.Add(Appearance.TattooLeft, 4);
                    lod.Add(Appearance.ChestTattoo, 4);
                    var it = itemManager.GetEquippedItem(EquipmentSlot.ES_RIGHTGAUNTLET);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //gauntletRight color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //gauntletRight color2
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_LEFTGAUNTLET);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //it color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //gauntletleft color2
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_RIGHTGLOVE);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //glovesRight color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //glovesRight color2
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_LEFTGLOVE);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //glovesLeft color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //glovesLeft color2
                    return lod.GetByteArray();
                }
                case 1:
                {
                    var lod = new LODHelper(20);
                    var it = itemManager.GetEquippedItem(EquipmentSlot.ES_RIGHTSHIN);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //shinRight color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //shinRight color2
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_LEFTSHIN);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //shinLeft color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //shinLeft color2
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_RIGHTTHIGH);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //thighRight color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //thighRight color2
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_LEFTTHIGH);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //thighLeft color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //thighLeft color2
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_BELT);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //belt color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //belt color2
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_RIGHTSHOULDER);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //shoulderRight color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //shoulderRight color2
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_LEFTSHOULDER);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //shoulderLeft color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //shoulderLeft color2
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_HELMET);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //helmet color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //helmet color2
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_SHOES);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //shoes color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //shoes color2
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_PANTS);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //pants color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //pants color2
                    return lod.GetByteArray();
                }
                case 2:
                {
                    var lod = new LODHelper(15);
                    lod.Add(0, 8); //unused
                    lod.Add(0, 4); //unused
                    var it = itemManager.GetEquippedItem(EquipmentSlot.ES_RANGEDWEAPON);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //ranged weapon id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_SHIELD);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 8); //shield id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_MELEEWEAPON);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 8); //melee weapon id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_RIGHTSHIN);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //shinRight id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_LEFTSHIN);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //shinLeft id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_RIGHTTHIGH);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //thighRight id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_LEFTTHIGH);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //thighLeft id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_BELT);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //belt id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_RIGHTGAUNTLET);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //gauntletRight id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_LEFTGAUNTLET);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //gauntletLeft id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_RIGHTSHOULDER);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //shoulderRight id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_LEFTSHOULDER);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //shoulderLeft id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_HELMET);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //helmet id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_SHOES);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 7); //shoes id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_PANTS);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 7); //pants id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_RIGHTGLOVE);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //glove right id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_LEFTGLOVE);
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //glove left id
                    return lod.GetByteArray();
                }
                case 3:
                {
                    var lod = new LODHelper(10);
                    lod.Add(0, 1); //unused
                    var it = itemManager.GetEquippedItem(EquipmentSlot.ES_CHESTARMOUR);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //chest armor color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //chest armor color2
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 6); //chest armor id
                    it = itemManager.GetEquippedItem(EquipmentSlot.ES_CHEST);
                    lod.Add(it != null ? (int)it.Color1 : 0, 8); //chest color1
                    lod.Add(it != null ? (int)it.Color2 : 0, 8); //chest color2
                    lod.Add(it != null ? GameData.Get.itemDB.GetSetIndex(it) : 0, 8); // chest cloth id
                    lod.Add(Appearance.HairColor, 8);
                    lod.Add(Appearance.HairStyle, 6);
                    lod.Add(Appearance.BodyColor, 8);
                    lod.Add(0, 1); //displayLogo
                    lod.Add(Appearance.HeadType, 6);
                    lod.Add(Appearance.BodyType, 2);
                    lod.Add((byte) Appearance.Gender, 1);
                    lod.Add(Appearance.Race, 1);
                    return lod.GetByteArray();
                }
            }
            return null;
        }

        #endregion

        #region Actions

        [SerializeField] bool isResting;

        public bool IsResting
        {
            get { return isResting; }
        }

        /// <summary>
        ///     Should initiate character sitting, does not work (no values do anything on the client) TODO fix
        /// </summary>
        public void Rest(int sitState)
        {
            //if (CombatMode == ECombatMode.CBM_Idle)
            //{
            //    isResting = sitState == 0;
            //    Message m = PacketCreator.S2C_GAME_PLAYERPAWN_SV2CL_SITDOWN(this);
            //    SendToClient(m);
            //}
        }

        /// <summary>
        ///     Respawns the character. Teleports to nearest respawn, resets health, pawnstate to alive and plays a graphical
        ///     effect
        /// </summary>
        public void Resurrect()
        {
            ActiveZone.TeleportToNearestRespawnLocation(this);
            SetHealth(MaxHealth);
            SetPawnState(EPawnStates.PS_ALIVE);
            PlayEffect(EPawnEffectType.EPET_ShapeUnshift);
        }

        #endregion

        #region Skills

        /// <summary>
        ///     Initiated by the client on Skilldeck assignments. Updates the serverside skilldeck
        /// </summary>
        public void SetSkillDeck(int[] newSkillDeck)
        {
            if (newSkillDeck.Length != 30)
            {
                Debug.LogWarning("SetSkillDeck length mismatch");
                return;
            }
            ActiveSkillDeck.Reset();
            for (var i = 0; i < newSkillDeck.Length; i++)
            {
                if (newSkillDeck[i] <= 0)
                {
                    continue;
                }
                ActiveSkillDeck[i] = GetSkill(newSkillDeck[i]);
            }
            SendToClient(PacketCreator.S2C_GAME_PLAYERSKILLS_SV2CL_SETSKILLS(this, Skills,
                ActiveSkillDeck.ToArray()));
        }

        /// <summary>
        ///     Adds a skill to this characters skill-list (if found by the given <see cref="id" />) TODO check requirements like
        ///     level, class points etc
        /// </summary>
        /// <param name="id"></param>
        public void LearnSkill(int id)
        {
            var s = GameData.Get.skillDB.GetSkill(id);
            if (s != null)
            {
                LearnSkill(s);
            }
            else
            {
                Debug.Log("(LearnSkill) missing: " + id);
            }
        }

        /// <summary>
        ///     Initiated by the client, indicates a skill use request. Executes the requested skill (if available) and handles
        ///     errors with skillcasting
        /// </summary>
        public void ClientUseSkill(int slotIndex, int targetID, Vector3 camPos, Vector3 targetPosition, float clientTime)
        {
            if (Mathf.Abs(clientTime - Time.time) > 0.5f)
            {
                SendToClient(PacketCreator.S2C_GAME_PLAYERCONTROLLER_SV2CL_UPDATESERVERTIME(Time.time));
            }
            var s = ActiveSkillDeck.GetSkillFromActiveTier(slotIndex);
            var result = ESkillStartFailure.SSF_INVALID_SKILL;
            if (s != null)
            {
                result = UseSkill(s.resourceID, targetID, targetPosition, clientTime, camPos);
            }
            if (result == ESkillStartFailure.SSF_ALLOWED)
            {
                ActiveSkillDeck.SetActiveSlot(slotIndex);
            }
            else
            {
                DebugChatMessage("Skill result: " + result);
            }
        }

        protected override void OnLearnedSkill(FSkill s)
        {
            var m = PacketCreator.S2C_GAME_SKILLS_SV2CL_LEARNSKILL(s);
            SendToClient(m);
        }

        protected override void OnStartCastSkill(SkillContext s)
        {
            base.OnStartCastSkill(s);
            SendToClient(PacketCreator.S2C_GAME_SKILLS_SV2CL_ADDACTIVESKILL(this, s));
        }

        protected override void OnEndCastSkill(SkillContext s)
        {
            base.OnEndCastSkill(s);
            SendToClient(PacketCreator.S2C_GAME_SKILLS_SV2CL_CLEARLASTSKILL(this, s.ExecutingSkill));
        }

        public override void OnDamageCaused(SkillApplyResult sap)
        {
            var m = PacketCreator.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEOUTPUTDAMAGE(sap.skillTarget.RelevanceID,
                sap.appliedSkill.resourceID, sap.damageCaused, sap.damageResisted);
            SendToClient(m);
        }

        public override void OnHealingCaused(SkillApplyResult sap)
        {
            if (sap.healCaused > 0)
            {
                var m = PacketCreator.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEOUTPUTHEAL(sap.skillTarget.RelevanceID,
                    sap.appliedSkill.resourceID, sap.healCaused);
                SendToClient(m);
            }
        }

        public override void OnStatChangeCaused(SkillApplyResult sap)
        {
            SendToClient(
                PacketCreator.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEOUTPUTSTATE(sap.skillTarget.RelevanceID,
                    sap.appliedSkill.resourceID, sap.appliedEffect.resourceID, sap.statChange));
        }

        #endregion

        #region Items

        //rework
        [SerializeField] Player_ItemManager itemManager;

        public Player_ItemManager ItemManager
        {
            get { return itemManager; }
        }

        void OnInventoryItemChanged(Game_Item item, EItemChangeNotification notificationType,
            EItemLocationType locationType, int slotID, int locationID)
        {
            if (item == null)
            {
                var m = PacketCreator.S2C_GAME_PLAYERITEMMANAGER_SV2CL_REMOVEITEM(locationType, slotID, 0);
                SendToClient(m);
            }
            else
            {
                var m = PacketCreator.S2C_GAME_PLAYERITEMMANAGER_SV2CL_SETITEM(item, notificationType);
                SendToClient(m);
            }
        }

        #endregion

        #region Currency

        int money;

        public int Money
        {
            get { return money; }
            set { money = Mathf.Abs(value); }
        }

        public void GiveMoney(int amount)
        {
            amount = Mathf.Abs(amount);
            money = money + amount;
        }

        public void TakeMoney(int amount)
        {
            amount = Mathf.Abs(amount);
            money = Mathf.Clamp(money - amount, 0, amount);
        }

        #endregion

        #region Leveling

        int famePoints;

        public int FamePoints
        {
            get { return famePoints; }
            set { famePoints = value; }
        }

        int pepPoints;

        public int PepPoints
        {
            get { return pepPoints; }
            set { pepPoints = value; }
        }

        byte extraBodyPoints;

        public byte ExtraBodyPoints
        {
            get { return extraBodyPoints; }
            set { extraBodyPoints = value; }
        }

        byte extraMindPoints;

        public byte ExtraMindPoints
        {
            get { return extraMindPoints; }
            set { extraMindPoints = value; }
        }

        byte extraFocusPoints;

        public byte ExtraFocusPoints
        {
            get { return extraFocusPoints; }
            set { extraFocusPoints = value; }
        }

        byte remainingAttributePoints;

        public byte RemainingAttributePoints
        {
            get { return remainingAttributePoints; }
            set { remainingAttributePoints = value; }
        }

        #endregion

        #region Effects

        public override void PlayEffect(EPawnEffectType effectType)
        {
            base.PlayEffect(effectType);
            var m = PacketCreator.S2R_GAME_PAWN_SV2CLREL_PLAYPAWNEFFECT(this, effectType);
            SendToClient(m);
        }

        public override void PlayEffectDirect(int effectID)
        {
            base.PlayEffectDirect(effectID);
            var m = PacketCreator.S2R_GAME_PAWN_SV2CLREL_PLAYPAWNEFFECTDIRECT(this, effectID);
            SendToClient(m);
        }

        public override void PlaySound(EPawnSound soundEffect, float volume)
        {
            base.PlaySound(soundEffect, volume);
            var m = PacketCreator.S2R_GAME_PAWN_SV2CLREL_STATICPLAYSOUND(this, soundEffect, volume);
            SendToClient(m);
        }

        public override void RunEvent(SkillContext sInfo, SkillEventFX fxEvent, Character skillPawn,
            Character triggerPawn, Character targetPawn)
        {
            base.RunEvent(sInfo, fxEvent, skillPawn, triggerPawn, targetPawn);
            SendToClient(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(this,
                sInfo.ExecutingSkill.resourceID, fxEvent.resourceID, 1, skillPawn, triggerPawn, targetPawn,
                sInfo.currentSkillTime));
        }

        public override void RunEventL(SkillContext sInfo, SkillEventFX fxEvent, Character skillPawn,
            Character triggerPawn, Vector3 location, Character targetPawn)
        {
            base.RunEventL(sInfo, fxEvent, skillPawn, triggerPawn, location, targetPawn);
            SendToClient(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_RUNEVENT(this,
                sInfo.ExecutingSkill.resourceID, fxEvent.resourceID, 1, skillPawn, triggerPawn, targetPawn, location,
                sInfo.currentSkillTime));
        }

        #endregion

        #region Combat

        public override void DrawWeapon()
        {
            base.DrawWeapon();
            SendToClient(PacketCreator.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_DRAWWEAPON(this));
        }

        public override void SheatheWeapon()
        {
            base.SheatheWeapon();
            equippedWeaponType = EWeaponCategory.EWC_None;
            SendToClient(PacketCreator.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SHEATHEWEAPON());
        }

        public override void SwitchWeapon(EWeaponCategory newWeapon)
        {
            base.SwitchWeapon(newWeapon);
            if (CombatMode != ECombatMode.CBM_Idle)
            {
                SendToClient(PacketCreator.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SETWEAPON(this));
            }
        }

        protected override void OnDamageReceived(SkillApplyResult sap)
        {
            base.OnDamageReceived(sap);
            if (sap.damageCaused != 0)
            {
                var m = PacketCreator.S2R_BASE_PAWN_SV2CLREL_DAMAGEACTIONS(this, 1f);
                SendToClient(m);
            }
        }

        #endregion

        #region Relevance

        /// <summary>
        ///     <see cref="Entity.OnEntityBecameRelevant" />
        /// </summary>
        public override void OnEntityBecameRelevant(Entity rel)
        {
            base.OnEntityBecameRelevant(rel);
            if (rel.RelevanceID == -1)
            {
                Debug.Log("Tried to sync invalid ID, ignoring");
                return;
            }
            var npc = rel as NpcCharacter;
            if (npc != null)
            {
                SendToClient(PacketCreator.S2C_NPC_ADD(npc));
                return;
            }

            var pc = rel as PlayerCharacter;
            if (pc != null)
            {
                SendToClient(PacketCreator.S2C_PLAYER_ADD(pc));
                return;
            }

            var element = rel as InteractiveLevelElement;
            if (element != null)
            {
                Debug.Log("TODO sync interactiveElements");
            }
        }

        /// <summary>
        ///     <see cref="Entity.OnEntityBecameIrrelevant" />
        /// </summary>
        public override void OnEntityBecameIrrelevant(Entity rel)
        {
            var contained = relevantObjects.Contains(rel);
            base.OnEntityBecameIrrelevant(rel);
            if (contained)
            {
                SendToClient(PacketCreator.S2C_BASE_PAWN_REMOVE(rel));
            }
        }

        /// <summary>
        ///     <see cref="Entity.ReceiveRelevanceMessage" />. Syncs messages to player (if sender is relevant)
        /// </summary>
        public override bool ReceiveRelevanceMessage(Entity rel, Message m)
        {
            if (base.ReceiveRelevanceMessage(rel, m))
            {
                SendToClient(m);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     returns a list of all players from the relevance
        /// </summary>
        public List<PlayerCharacter> GetRelevantPlayers()
        {
            var ps = new List<PlayerCharacter>();
            for (var i = relevantObjects.Count; i-- > 0;)
            {
                var p = relevantObjects[i] as PlayerCharacter;
                if (p != null)
                {
                    ps.Add(p);
                }
            }
            return ps;
        }

        #endregion

        #region Chat

        /// <summary>
        ///     Sends a message to the client chatbox (unlike ReceiveRelevanceMessage, is guaranteed to reach the client, even if
        ///     the sender is not in relevance radius)
        /// </summary>
        public void ReceiveChatMessage(string sender, string message, EGameChatRanges channelID)
        {
            ReceiveRelevanceMessage(null, PacketCreator.S2C_GAME_CHAT_SV2CL_ONMESSAGE(sender, message, channelID));
        }

        /// <summary>
        ///     Sends a debug message to the player if <see cref="AccountPrivilege" /> is higher than player and
        ///     <see cref="DebugMode" /> is turned on (<see cref="ChatCommandHandler" />)
        /// </summary>
        public void DebugChatMessage(string msg)
        {
            if (DebugMode && (int) Owner.Account.Level > (int) AccountPrivilege.Player)
            {
                ReceiveChatMessage("Debug", msg, EGameChatRanges.GCR_SYSTEM);
            }
        }

        #endregion

        #region Conversations

        public CurrentConv currentConv;

        public class CurrentConv
        {
            public ConversationNode curNode;
            public ConversationTopic curTopic;
            public NpcCharacter partner; 

            public CurrentConv(ConversationTopic t, ConversationNode n, NpcCharacter p)
            {
                curTopic = t;
                curNode = n;
                partner = p;
            }
        }

        #endregion

        #region Quests

        [SerializeField]
        public QuestDataContainer QuestData;

        public bool IsEligibleForQuest(Quest_Type quest)
        {
            //Verify requirements list
            foreach (var req in quest.requirements)
            {
                if (!req.isMet(this))   //If requirement not met by player
                {
                    return false;       //return false
                }
            }

            //Verify prequests
            foreach (SBResource preQuest in quest.preQuests)
            {
                if (!CompletedQuest(preQuest.ID))  //If prequest not complete
                {
                    return false;                       //return false
                }
            }

            return true;
        }
        public bool HasQuest(int questID)
        {
            foreach (var questProgress in QuestData.curQuests)
            {
                if (questProgress.questID == questID) { return true; }
            }
            return false;
        }
        public bool CompletedQuest(int questID)
        {
            foreach (var questSBR in QuestData.completedQuestIDs)
            {
                if (questSBR == questID) { return true; }
            }
            return false;
        }
        public void RemoveQuest(int questID)
        {
            //int numTargets = QuestData.getNumTargets(questID);

            //Remove quest on game server
            QuestData.RemoveQuest(questID);

            //Send message to remove from player log
            var m = PacketCreator.S2C_GAME_PLAYERQUESTLOG_SV2CL_REMOVEQUEST(questID);
            SendToClient(m);
            return;
        }

        public bool HasUnfinishedTargets(Quest_Type quest)
        {
            PlayerQuestProgress questProgress = null;

            //get the quest ID's progress values
            foreach (var qP in QuestData.curQuests)
            {
                if (qP.questID == quest.resourceID) { questProgress = qP; }
            }

            if (questProgress == null)
            {
                Debug.Log("Player.hasUnfinishedTargets : Player doesn't currently have the parameter quest - returning TRUE for now");
                return true;
            }

            Debug.Log("Player.hasUnfinishedTargets : TODO - Compare each target progress to what its completed value should be");
            //TODO: Placeholder - returns true if any targets have progress value 0, returns false otherwise
            foreach (var targetValue in questProgress.targetProgress)
            {
                if (targetValue == 0) { return true; }
            }

            return false;
        }
        #endregion
    }
}