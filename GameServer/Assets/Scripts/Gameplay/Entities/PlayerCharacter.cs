using System;
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
using Gameplay.Quests.QuestTargets;
using Gameplay.Entities.Interactives;
using Gameplay.Quests.QuestConditions;
using Gameplay.Loot;

namespace Gameplay.Entities
{
    public sealed class PlayerCharacter : Character
    {

        public PlayerInfo Owner;
        public bool DebugMode;

        byte _moveFrame;

        public DBPlayerCharacter dbRef;

        public PlayerGuild Guild;

        public PlayerTeam Team;

        /// <summary>
        ///     used to sync player character movement
        /// </summary>
        public byte MoveFrame
        {
            get { return _moveFrame; }
            set { _moveFrame = value; }
        }

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
            pc.LastZoneID = (MapIDs)dbc.LastZoneID;
            pc.PawnState = (EPawnStates)dbc.PawnState;
            pc.ArcheType = (ClassArcheType)dbc.ArcheType;
            pc.Money = dbc.Money;
            pc.Faction = GameData.Get.factionDB.GetFaction(dbc.Faction);
            var it = ScriptableObject.CreateInstance<Game_PlayerItemManager>();
            it.Init(pc);
            pc.Items = it;
            var app = ScriptableObject.CreateInstance<Game_PlayerAppearance>();
            app.Init(pc);
            pc.Appearance = app;
            pc.Stats = ScriptableObject.CreateInstance<Game_PlayerStats>();
            pc.Stats.Init(pc);
            var sk =  ScriptableObject.CreateInstance<Game_PlayerSkills>();
            sk.Init(pc);
            pc.Skills = sk;
            var cs = ScriptableObject.CreateInstance<Game_PlayerCombatState>();
            cs.Init(pc);
            pc.CombatState = cs;
            pc.questData = ScriptableObject.CreateInstance<QuestDataContainer>();
            pc.questData.LoadForPlayer(dbc.QuestTargets, pc);
            pc.persistentVars = ScriptableObject.CreateInstance<PersistentVarsContainer>();
            pc.persistentVars.LoadForPlayer(dbc.PersistentVars, pc);
            pc.SetupCollision();
            pc.InitEnabled = true;
            pc.InitColl = ECollisionType.COL_Colliding;
            return pc;
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "Player.psd");
        }

        /// <summary>
        ///     Sends a Message directly to the client associated with this player instance (bypasses relevance checks)
        /// </summary>
        public void SendToClient(Message m)
        {
            Owner.Connection.SendMessage(m);
        }

        public void ResyncClientTime()
        {
            SendToClient(PacketCreator.S2C_GAME_PLAYERCONTROLLER_SV2CL_UPDATESERVERTIME(Time.time));
        }

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

            //kill player if below zone killY
            if (ActiveZone && ActiveZone.killY != 0 
                &&  pos.y < ActiveZone.killY 
                &&  PawnState != EPawnStates.PS_DEAD)
            {
                Stats.SetHealth(0);
                SetPawnState(EPawnStates.PS_DEAD);
                OnDiedThroughDamage(null);                
            }
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

        public override bool SitDown(bool onChairFlag = false)
        {
            if (base.SitDown(onChairFlag))
            {

                var mMove = PacketCreator.S2C_GAME_PLAYERPAWN_SV2CL_FORCEMOVEMENT(Position, Velocity, Physics);
                SendToClient(mMove);

                var mSit = PacketCreator.S2C_GAME_PLAYERPAWN_SV2CL_SITDOWN(onChairFlag);
                SendToClient(mSit);

                return true;
            }
            return false;
        }

        #endregion

        protected override void OnDuffsChanged()
        {
            base.OnDuffsChanged();
            SendToClient(PacketCreator.S2R_GAME_SKILLS_SV2CLREL_UPDATEDUFFS(this, Duffs));
        }

        protected override void OnLeavingZone(Zone z)
        {
            base.OnLeavingZone(z);
            relevantObjects.Clear();
        }

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
            Stats.SetHealth(Stats.mRecord.MaxHealth);
            SetPawnState(EPawnStates.PS_ALIVE);
            PlayEffect(EPawnEffectType.EPET_ShapeUnshift);
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
            if (amount == 0) return;
            amount = Mathf.Abs(amount);
            money = money + amount;
            Message m = PacketCreator.S2C_GAME_PLAYERCHARACTER_SV2CL_UPDATEMONEY(money);
            SendToClient(m);
        }

        public void TakeMoney(int amount)
        {
            amount = Mathf.Abs(amount);
            money = Mathf.Clamp(money - amount, 0, amount);
            Message m = PacketCreator.S2C_GAME_PLAYERCHARACTER_SV2CL_UPDATEMONEY(money);
            SendToClient(m);
        }

        #endregion

        #region Leveling

        public void GiveQuestFame(int points, int qpFrac)
        {
            

            if (qpFrac <= 0) { Stats.GiveFame(points); }
            else {
                //TODO
                //Valshaaran - experimental quest XP formula : (quest points value) * (qpFrac / player fame level)?
                float multFactor = qpFrac / Stats.GetFameLevel();
                Stats.GiveFame((int)(points * multFactor));

                //This version just gives the unmodified points amount
                //GiveFame(points);
            }
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

        #endregion

        #region Combat

        //public override void SwitchWeapon(EWeaponCategory newWeapon)
        //{
        //    Debug.Log("weapon change request: " + newWeapon);
        //    //var prevWeapon = equippedWeaponType;
        //    base.SwitchWeapon(newWeapon);
        //    if (CombatMode != ECombatMode.CBM_Idle)
        //    {
        //        SendToClient(PacketCreator.S2C_GAME_PLAYERCOMBATSTATE_SV2CL_SETWEAPON(this));
        //    }
        //}

        public override void SetPawnState(EPawnStates newState)
        {
            base.SetPawnState(newState);
            SendToClient(PacketCreator.S2R_GAME_PAWN_SV2CLREL_UPDATENETSTATE(this));
        }

        /// <summary>
        ///     Notifies the client through combat message log, Sheathes the weapon and sets the combatstate to idle TODO cleanup
        /// </summary>
        /// <param name="source"></param>
        protected override void OnDiedThroughDamage(Character source)
        {
            base.OnDiedThroughDamage(source);
            CombatState.sv_SheatheWeapon();
            //CombatMode = ECombatMode.CBM_Idle;
            //SheatheWeapon();
            SendToClient(PacketCreator.S2C_GAME_PAWN_SV2CL_COMBATMESSAGEDEATH(source));
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


        public void OnNPCKill(NpcCharacter npc)
        {
            var npct = npc.Type;

            #region Quest progress
            //Quest progress                
            foreach (var curQuest in questData.curQuests)
            {
                var questObj = GameData.Get.questDB.GetQuest(curQuest.questID);

                //Look for Destroy/Exterminate/Hunt/Kill targets
                for (int n = 0; n < questObj.targets.Count; n++)
                {
                    var target = questObj.targets[n];
                    //Break if target completed...
                    if (QuestTargetIsComplete(questObj, n)) continue;

                    //...or if  pretargets unfulfilled
                    if (!PreTargetsComplete(target, questObj)) continue;

                    if (target is QT_Destroy)
                    {
                        var tDest = (QT_Destroy)target;
                        if (npct.resourceID == tDest.Target.resourceID)
                        {
                            if (!TryAdvanceTarget(questObj, target))
                            {
                                Debug.Log("PlayerCharacter.OnNPCKill : QT_Destroy progress advancement failed");
                            }
                        }
                    }
                    else if (target is QT_Hunt)
                    {
                        var tHunt = (QT_Hunt)target;
                        if (npct.resourceID == tHunt.NpcTargetID.ID)
                        {
                            if (!TryAdvanceTarget(questObj, target))
                            {
                                Debug.Log("PlayerCharacter.OnNPCKill : QT_Hunt progress advancement failed");
                            }
                        }
                    }

                    else if (target is QT_Exterminate)
                    {
                        var tExt = (QT_Exterminate)target;
                        if (npct.TaxonomyFaction.ID == tExt.FactionID.ID)
                        {
                            if (!TryAdvanceTarget(questObj, target))
                            {
                                Debug.Log("PlayerCharacter.OnNPCKill : QT_Exterminate progress advancement failed");
                            }
                        }
                    }
                    else if (target is QT_Kill)
                    {
                        var tKill = (QT_Kill)target;
                        foreach (var killType in tKill.NpcTargetIDs)
                        {
                            if (npct.resourceID == killType.ID)
                            {
                                if (!TryAdvanceQTKill(questObj, target, npct))
                                {
                                    Debug.Log("PlayerCharacter.OnNPCKill : QT_Kill progress advancement failed");
                                }
                                else break; //Ensures only 1 subtarget advances at a time
                            }
                        }
                    }
                }
            }
            #endregion

            #region Grant Fame, PEP
            //Valshaaran - placeholder formula, feel free to improve
            int nFame = npc.Stats.GetFameLevel();
            int baseKillPoints = 10;
            float weightedKillPoints = (baseKillPoints * nFame * nFame) / Stats.GetFameLevel();
            Stats.GiveFame((int)weightedKillPoints);

            //TODO: PEP

            #endregion

            #region Loot

            var lootTables = npc.Faction.Loot;
            lootTables.AddRange(npc.Type.Loot);

            if (Team == null)
            {
                #region Single player
                var receivers = new List<PlayerCharacter>();
                receivers.Add(this);

                LootManager.Get.CreateTransaction(lootTables, receivers);
                GiveMoney(npc.DropMoney());
                #endregion
            }
            else
            {
                #region Team
                var creditMembers = new List<PlayerCharacter>();
                creditMembers.Add(this);
                foreach (var tm in Team.Members)
                {
                    if (tm != this && tm.ObjectIsRelevant(npc))
                    {
                        creditMembers.Add(tm);
                    }
                }
                LootManager.Get.CreateTransaction(lootTables, creditMembers, Team.CurLootMode);

                foreach (var cm in creditMembers)
                {
                    cm.GiveMoney(npc.DropMoney() / creditMembers.Count);
                }

                #endregion                
            }

            #endregion
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
                SendToClient(PacketCreator.S2C_INTERACTIVELEVELELEMENT_ADD(element));
                return;
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
                var ile = rel as InteractiveLevelElement;
                if (ile) SendToClient(PacketCreator.S2C_LEVELOBJECT_REMOVE(ile));
                else SendToClient(PacketCreator.S2C_BASE_PAWN_REMOVE(rel));
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

        [SerializeField,ReadOnly]
        public QuestDataContainer questData;

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
                if (!QuestIsComplete(preQuest.ID))  //If prequest not complete
                {
                    return false;                       //return false
                }
            }

            return true;
        }
        public bool HasQuest(int questID)
        {
            foreach (var questProgress in questData.curQuests)
            {
                if (questProgress.questID == questID) { return true; }
            }
            return false;
        }
        public bool QuestIsComplete(int questID)
        {
            foreach (var questSBR in questData.completedQuestIDs)
            {
                if (questSBR == questID) { return true; }
            }
            return false;
        }
        public bool PreTargetsComplete(QuestTarget target, Quest_Type quest)
        {
            foreach (var pretarget in target.Pretargets)
            {
                int targetIndex = quest.getPretargetIndex(target.resource.ID, pretarget.ID);

                if (!QuestTargetIsComplete(quest, targetIndex))
                {
                    return false; //If any target remains incomplete, return false
                }

            }
            return true;
        }
        public bool QuestTargetIsComplete(Quest_Type quest, int targetIndex)
        {
            int completeThreshold = quest.targets[targetIndex].GetCompletedProgressValue();

            foreach (var questProgress in questData.curQuests)
            {
                if (questProgress.questID == quest.resourceID) {

                    if (questProgress.targetProgress[targetIndex] >= completeThreshold) return true;
                    else return false;
                }
            }
            return false;
        }
        public bool HasUnfinishedTargets(Quest_Type quest)
        {
            PlayerQuestProgress questProgress = null;

            //get the quest ID's progress values
            foreach (var qP in questData.curQuests)
            {
                if (qP.questID == quest.resourceID) { questProgress = qP; }
            }

            if (questProgress == null)
            {
                Debug.Log("Player.hasUnfinishedTargets : Player doesn't currently have the parameter quest - returning TRUE for now");
                return true;
            }

            //Returns true if any target has a value less than its completed progress value
            for (int n = 0; n < questProgress.targetProgress.Count;n++)
            {
                var targetValue = questProgress.targetProgress[n];
                if (targetValue < quest.targets[n].GetCompletedProgressValue()) return true;
            }

            return false;
        }
        public void RemoveQuest(int questID)
        {
            //int numTargets = QuestData.getNumTargets(questID);

            //Remove quest on game server
            questData.RemoveQuest(questID);

            //Send message to remove from player log
            var m = PacketCreator.S2C_GAME_PLAYERQUESTLOG_SV2CL_REMOVEQUEST(questID);
            SendToClient(m);
            return;
        }
        public void FinishQuest(Quest_Type quest)
        {
            //Set game server quest data to finished                
            questData.FinishQuest(quest.resourceID);

            //Give quest rewards to player

            //Quest points
            GiveQuestFame(quest.questPoints.QP, quest.questPoints.QPFrac);

            //Money
            GiveMoney(quest.money);

            //Item rewards (Content_Inventory)
            Items.GiveInventory(quest.rewardItems);         

            //Send complete quest packet            
            Message mQuestFinish = PacketCreator.S2C_GAME_PLAYERQUESTLOG_SV2CL_FINISHQUEST(quest.resourceID);
           // Message mQuestRemove = PacketCreator.S2C_GAME_PLAYERQUESTLOG_SV2CL_REMOVEQUEST(quest.resourceID);
            SendToClient(mQuestFinish);
            //SendToClient(mQuestRemove);
        }
        public void CompleteQT(Quest_Type quest, int targetIndex)
        {
            int completedValue = quest.targets[targetIndex].GetCompletedProgressValue();
            questData.UpdateQuest(quest.resourceID, targetIndex, completedValue);
            Message m = PacketCreator.S2C_GAME_PLAYERQUESTLOG_SV2CL_SETTARGETPROGRESS(
                quest.resourceID, targetIndex, completedValue);
            SendToClient(m);
        }
        public void SetQTProgress(int questID, int targetIndex, int progress)
        {
            //Server quest data
            questData.UpdateQuest(questID, targetIndex, progress);

            //Dispatch packet
            Message m = PacketCreator.S2C_GAME_PLAYERQUESTLOG_SV2CL_SETTARGETPROGRESS(
                questID, targetIndex, progress);
            SendToClient(m);
        }

        /// <summary>
        /// Attempts to advance a quest target's progress for both server and client 
        /// </summary>
        /// <param name="quest"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool TryAdvanceTarget(Quest_Type quest, QuestTarget target)
        {
            var playerTarProgress = questData.getProgress(quest.resourceID);
            int tarIndex = quest.getTargetIndex(target.resource.ID);
            int newValue = -1;

            //If the objective is already completed, return false;
            if (playerTarProgress.targetProgress[tarIndex] >= target.GetCompletedProgressValue())
            {
                return false;
            }

            #region Update and check QuestConditions
            //Get condition targets
            for (int n = 0; n < quest.targets.Count;n++)
            {
                var t = quest.targets[n];

                var qc = t as QuestCondition;
                if (qc)
                {
                    //If this is a final target of the QC, update QC, 
                    //and don't advance this target if not true
                    if (qc.HasFinalTarget(target.resource))
                    {

                        //If QC not fulfilled, return false
                        if (!qc.UpdateAndCheck(this, quest)) return false;
                    }
                    //Otherwise just update the QC status
                    else {
                        qc.UpdateAndCheck(this, quest);
                    }
                }
            }
            #endregion
            newValue = playerTarProgress.targetProgress[tarIndex] + 1;

            //Do the update
            if (newValue != -1) {

                SetQTProgress(quest.resourceID, tarIndex, newValue);

                //target onAdvance
                target.onAdvance(this, newValue);

                return true;
            }
            return false;
        }
        public bool TryAdvanceQTKill(Quest_Type quest, QuestTarget target, NPC_Type npcKilled)
        {
            var playerTarProgress = questData.getProgress(quest.resourceID);
            int tarIndex = quest.getTargetIndex(target.resource.ID);

            int newValue = -1;

            if (target is QT_Kill)
            {
                var qtKill = target as QT_Kill;
                for (int n = 0; n < qtKill.NpcTargetIDs.Count;n++)
                {
                    var targetNPC = qtKill.NpcTargetIDs[n];
                    if (targetNPC.ID == npcKilled.resourceID)
                    {
                        var oldValue = playerTarProgress.targetProgress[tarIndex];
                        //Valshaaran - try bit flag for each target, experimental
                        //Bitwise OR with old progress value
                        newValue = oldValue | (1 << n);

                        //If value unchanged, continue 
                        //- this will flag other unflagged kill targets of the same type
                        if (newValue == oldValue)
                        {
                            newValue = -1;
                            continue;
                        }
                        else break;
                    }
                }
            }            

            //Do the update
            if (newValue != -1)
                {

                //Server quest data
                questData.UpdateQuest(quest.resourceID, tarIndex, newValue);

                //Dispatch packet
                Message m = PacketCreator.S2C_GAME_PLAYERQUESTLOG_SV2CL_SETTARGETPROGRESS(quest.resourceID, tarIndex, newValue);
                SendToClient(m);

                //target onAdvance
                target.onAdvance(this, newValue);

                return true;
                }
            return false;
        }

        #endregion

        #region Persistent variables

        [SerializeField, ReadOnly]
        public PersistentVarsContainer persistentVars;

        #endregion
    }
}