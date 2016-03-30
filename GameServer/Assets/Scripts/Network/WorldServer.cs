using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Common.UnrealTypes;
using Database.Dynamic;
using Gameplay.Entities;
using Lidgren.Network;
using UnityEngine;
using Utility;
using World;

namespace Network
{
    /// <summary>
    ///     This class is responsible for receiving and dispatching networkmessages to all other game systems
    /// </summary>
    public class WorldServer : MonoBehaviour
    {
        readonly Dictionary<GameHeader, Action<Message>> _dispatchTable = new Dictionary<GameHeader, Action<Message>>();
        readonly Queue<Message> _incomingMessages = new Queue<Message>();

        readonly Queue<PlayerInfo> _pendingPlayerRemoves = new Queue<PlayerInfo>();

        readonly List<PlayerInfo> _players = new List<PlayerInfo>();
        float _lastDiscoveryRequest;

        NetClient _loginConnector;
        string _loginIP;
        int _loginPort;

        Action<PlayerInfo> _onPlayerLogout;

        NetConnector _server;

        Coroutine updatingRoutine;

        /// <summary>
        ///     returns a list of all connected players
        /// </summary>
        public List<PlayerInfo> Players
        {
            get { return _players; }
        }

        bool AddPlayer(PlayerInfo p)
        {
            if (!_players.Contains(p))
            {
                _players.Add(p);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Starts the server and enables it to receive network messages and respond to connection requests
        /// </summary>
        /// <param name="onPlayerLogout">delegate to handle a player logout on the MainThread</param>
        /// <returns></returns>
        public bool StartServer(ServerConfiguration config, Action<PlayerInfo> onPlayerLogout)
        {
            _server = new NetConnector(config.ListenIP, config.ServerPort, _incomingMessages);
            var npConfig = new NetPeerConfiguration("TCoSReborn");
            npConfig.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            npConfig.EnableMessageType(NetIncomingMessageType.StatusChanged);
            npConfig.ConnectionTimeout = 5f;
            _loginConnector = new NetClient(npConfig);
            _loginConnector.Start();
            _loginIP = config.LoginServerIp;
            _loginPort = config.LoginServerPort;
            RegisterHandlers();
            var success = _server.Start();
            if (success)
            {
                _onPlayerLogout = onPlayerLogout;
                _server.OnConnected += server_OnConnected;
                _server.OnDisconnected += server_OnDisconnected;
            }
            updatingRoutine = StartCoroutine(UpdateQueues());
            return success;
        }

        /// <summary>
        ///     closes the server and disconnects all players
        /// </summary>
        public void ShutDown()
        {
            Debug.Log("Shutting down WorldServer");
            if (_server != null)
            {
                _server.Shutdown();
            }
            if (_loginConnector != null)
            {
                _loginConnector.Shutdown("");
            }
            if (updatingRoutine != null)
            {
                StopCoroutine(updatingRoutine);
            }
            Debug.Log("WorldServer shut down");
        }

        void server_OnConnected(NetConnection connection)
        {
            if (connection.player != null && connection.player.Account != null)
            {
                Debug.Log("Game - _cachedConnection established: " + connection.player.Account.Name);
            }
            else
            {
                if (connection.ClientSocket != null)
                {
                    //Debug.Log("Game - _cachedConnection established: " + connection.ClientSocket.RemoteEndPoint.ToString());
                }
                else
                {
                    Debug.Log("[Warning] player connected, no socket ref set");
                }
            }
        }

        /// things added here are likely to break the server (unity -> not thread safe, use
        /// <see cref="GameWorld.HandlePlayerLogout" />
        /// instead)
        void server_OnDisconnected(NetConnection connection)
        {
            if (connection.player != null)
            {
                _pendingPlayerRemoves.Enqueue(connection.player);
            }
        }

        void RegisterHandlers()
        {
            _dispatchTable.Add(GameHeader.C2S_TRAVEL_CONNECT, HandleTravelConnect);
            _dispatchTable.Add(GameHeader.C2S_WORLD_PRE_LOGIN_ACK, HandlePreLoginAck);
            _dispatchTable.Add(GameHeader.C2S_CS_CREATE_CHARACTER, HandleCreateCharacter);
            _dispatchTable.Add(GameHeader.C2S_CS_SELECT_CHARACTER, HandleSelectCharacter);
            _dispatchTable.Add(GameHeader.C2S_CS_DELETE_CHARACTER, HandleDeleteCharacter);
            _dispatchTable.Add(GameHeader.C2S_GAME_CHAT_SEND_TEXTMESSAGE, HandleChatMessage);
            _dispatchTable.Add(GameHeader.C2S_WORLD_LOGIN_ACK, HandleWorldLoginAck);
            _dispatchTable.Add(GameHeader.C2S_WORLD_LOGOUT, HandleLogoutRequest);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERPAWN_CL2SV_UPDATEMOVEMENT, HandleMovementUpdate);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERPAWN_CL2SV_UPDATEMOVEMENTWITHPHYSICS, HandleMovementWithPhysicsUpdate);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERPAWN_CL2SV_UPDATEROTATION, HandleUpdateRotation);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERCOMBATSTATE_CL2SV_DRAWSHEATHEWEAPON, HandleToggleWeapon);
            _dispatchTable.Add(GameHeader.C2S_GAME_EMOTES_CL2SV_EMOTE, HandleDoEmote);
            _dispatchTable.Add(GameHeader.C2S_INTERACTIVELEVELELEMENT_CL2SV_ONRADIALMENUOPTION, HandleInteractiveElementInteraction);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERCOMBATSTATE_CL2SV_SWITCHWEAPONTYPE, HandleSwitchWeapon);
            _dispatchTable.Add(GameHeader.C2S_TEAM_INVITE, HandleTeamInvite);
            _dispatchTable.Add(GameHeader.C2S_TEAM_INVITE_ACK, HandleTeamInviteAck);
            _dispatchTable.Add(GameHeader.C2S_TEAM_KICK, HandleTeamKick);
            _dispatchTable.Add(GameHeader.C2S_TEAM_LEAVE, HandleTeamLeave);
            _dispatchTable.Add(GameHeader.C2S_TEAM_DISBAND, HandleTeamDisband);
            _dispatchTable.Add(GameHeader.C2S_TEAM_LEADER, HandleTeamLeader);
            _dispatchTable.Add(GameHeader.C2S_TEAM_LOOTMODE, HandleTeamLootMode);
            _dispatchTable.Add(GameHeader.C2S_GET_TEAM_INFO, HandleGetTeamInfo);
            _dispatchTable.Add(GameHeader.C2S_GAME_SKILLS_CL2SV_EXECUTEINDEX, HandleUseSkill);
            _dispatchTable.Add(GameHeader.C2S_GAME_SKILLS_CL2SV_EXECUTEINDEXL, HandleUseSkillLocation);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERCONVERSATION_CL2SV_INTERACT, HandleConversationInteract);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERCONVERSATION_CL2SV_REACT, HandleConversationReact);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERCONVERSATION_CL2SV_RESPOND, HandleConversationRespond);
            _dispatchTable.Add(GameHeader.C2S_GAME_PAWN_CL2SV_REST, HandleSitDown);
            _dispatchTable.Add(GameHeader.C2S_GAME_SKILLS_CL2SV_LEARNSKILL, HandleLearnSkill);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERSKILLS_CL2SV_SAVESKILLDECKSKILLS, HandleSaveSkillDeckSkills);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERINPUT_CL2SV_RESURRECT, HandleResurrect);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERITEMMANAGER_CL2SV_MOVEITEM, HandleMoveItem);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERQUESTLOG_CL2SV_SWIRLYOPTIONPAWN, HandleSwirlyOptionPawn);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERQUESTLOG_CL2SV_SWIRLYOPTION, HandleSwirlyOption);
            _dispatchTable.Add(GameHeader.C2S_GAME_PLAYERQUESTLOG_CL2SV_ABANDONQUEST, HandleAbandonQuest);
        }

        IEnumerator UpdateQueues()
        {
            while (true)
            {
                while (_incomingMessages.Count > 0)
                {
                    lock (_incomingMessages)
                    {
                        var m = _incomingMessages.Dequeue();
                        Action<Message> messageHandler;
                        if (!Enum.IsDefined(typeof (GameHeader), m.Header))
                        {
                            Debug.Log("no messageType defined for: " + m.Header);
                            continue;
                        }
                        if (_dispatchTable.TryGetValue((GameHeader) m.Header, out messageHandler))
                        {
                            messageHandler(m);
                        }
                        else
                        {
                            Debug.Log("no messageHandler defined for: " + (GameHeader) m.Header);
                        }
                    }
                }
                if (_pendingPlayerRemoves.Count > 0)
                {
                    var p = _pendingPlayerRemoves.Dequeue();
                    if (_onPlayerLogout != null)
                    {
                        _onPlayerLogout(p);
                    }
                    _players.Remove(p);
                }
                if (_loginConnector != null)
                {
                    UpdateLoginConnector();
                }
                yield return null;
            }
        }

        #region LoginServer

        void UpdateLoginConnector()
        {
            if (_loginConnector.ConnectionStatus == NetConnectionStatus.Disconnected || _loginConnector.ConnectionStatus == NetConnectionStatus.None)
            {
                if (Time.time - _lastDiscoveryRequest > 3f)
                {
                    _loginConnector.DiscoverKnownPeer(_loginIP, _loginPort);
                    Debug.Log("Trying to discover the Login Server to register to");
                    _lastDiscoveryRequest = Time.time;
                }
            }
            NetIncomingMessage msg;
            while ((msg = _loginConnector.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        HandleLoginServerStatus((NetConnectionStatus) msg.ReadByte());
                        break;
                    case NetIncomingMessageType.Data:
                        HandleLoginServerData(msg);
                        break;
                    case NetIncomingMessageType.DiscoveryResponse:
                        if (msg.LengthBytes > 0 && msg.ReadString().Equals("LoginServer", StringComparison.OrdinalIgnoreCase))
                        {
                            Debug.Log("Connecting to LoginServer");
                            var infoMsg = _loginConnector.CreateMessage();
                            infoMsg.Write((byte) CommunicationHeader.U2L_REGISTER_UNIVERSE);
                            var info = GameWorld.Instance.ServerConfig;
                            infoMsg.Write(info.ServerName);
                            infoMsg.Write(info.ServerLanguage);
                            infoMsg.Write(info.ServerType);
                            infoMsg.Write((byte) info.AccessRestriction);
                            infoMsg.Write(info.PublicIP);
                            infoMsg.Write(info.ServerPort);
                            _loginConnector.Connect(msg.SenderEndPoint, infoMsg);
                        }
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        Debug.LogWarning(msg.ReadString());
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        Debug.LogWarning(msg.ReadString());
                        break;
                }
            }
        }

        void HandleLoginServerStatus(NetConnectionStatus status)
        {
            switch (status)
            {
                case NetConnectionStatus.Connected:
                    Debug.Log("LoginServer connection established");
                    break;
                case NetConnectionStatus.Disconnected:
                    Debug.Log("LoginServer connection lost");
                    break;
            }
        }

        void HandleLoginServerData(NetIncomingMessage msg)
        {
            if (msg.LengthBytes < 1)
            {
                Debug.Log("invalid loginserver message received");
                return;
            }
            var header = (CommunicationHeader) msg.ReadByte();
            switch (header)
            {
                case CommunicationHeader.L2U_QUERY_POPULATION:
                    var oMsg = _loginConnector.CreateMessage();
                    oMsg.Write((byte) CommunicationHeader.U2L_UPDATE_POPULATION);
                    oMsg.Write(_players.Count);
                    _loginConnector.ServerConnection.SendMessage(oMsg, NetDeliveryMethod.ReliableOrdered, 0);
                    break;
                case CommunicationHeader.L2U_ACCOUNT_REQUESTLOGIN:
                    Debug.Log("TODO check for player limit, or similar reasons why this server would block incoming connections");
                    var response = _loginConnector.CreateMessage();
                    response.Write((byte) CommunicationHeader.U2L_ACCOUNT_REQUESTLOGIN_ACK);
                    response.Write(true); //allow/disallow
                    response.Write(msg.ReadString()); //accName
                    response.Write(msg.ReadString()); //passHash
                    response.Write(msg.ReadInt32()); //sessionKey
                    _loginConnector.ServerConnection.SendMessage(response, NetDeliveryMethod.ReliableOrdered, 0);
                    break;
            }
        }

        #endregion

        #region Handler

        #region Connection

        void HandleTravelConnect(Message m)
        {
            var key = m.ReadInt32();
            var acc = MysqlDb.AccountDB.GetAccount(key);
            if (acc != null)
            {
                var pInfo = new PlayerInfo(acc, m.Connection);
                if (!AddPlayer(pInfo))
                {
                    Debug.LogError("Player already in connected list! (this should not happen). Disconnecting");
                    m.Connection.Disconnect();
                }
                Debug.Log(string.Format("Player connected: '{0}'", pInfo.Account.Name));
                pInfo.LoadClientMap(MapIDs.CHARACTER_SELECTION);
                pInfo.CharacterCreationState = ECharacterCreationState.CCS_SELECT_CHARACTER;
                acc.IsOnline = true;
                acc.LastUniverse = GameWorld.Instance.UniverseID;
                MysqlDb.AccountDB.UpdateAccount(acc);
            }
            else
            {
                Debug.Log("Connection attempt with invalid SessionKey encountered");
                m.Connection.Disconnect();
            }
        }

        void HandlePreLoginAck(Message m)
        {
            var status = m.ReadUInt32();
            if (status == (uint) MessageStatusCode.NO_ERROR)
            {
                var p = m.Connection.player;
                p.LoadedMapID = p.MapIdTransitionTo;
                switch (p.LoadedMapID)
                {
                    case MapIDs.CHARACTER_SELECTION:
                        var characters = CharacterCreationSelection.GetAccountCharacters(p.Account);
                        m.Connection.SendMessage(PacketCreator.S2C_CS_LOGIN(p, characters));
                        if (characters.Count > 0)
                        {
                            p.CharacterCreationState = ECharacterCreationState.CCS_SELECT_CHARACTER;
                        }
                        else
                        {
                            p.CharacterCreationState = ECharacterCreationState.CCS_CREATE_CHARACTER;
                        }
                        break;
                    default:
                        if (p.ActiveCharacter == null || !GameWorld.Instance.InsertPlayerCharacter(p.ActiveCharacter, p.LoadedMapID))
                        {
                            p.Connection.Disconnect();
                            break;
                        }
                        //TODO handle other Maps with special requirements
                        p.CharacterCreationState = ECharacterCreationState.CCS_ENTER_WORLD;
                        m.Connection.SendMessage(PacketCreator.S2C_WORLD_LOGIN(p));
                        break;
                }
            }
            else
            {
                Debug.Log("Game - Client error for PreLoginAck: " + status);
                m.Connection.Disconnect();
            }
        }

        void HandleCreateCharacter(Message m)
        {
            if (!CharacterCreationSelection.CanCreateNewCharacter(m.Connection.player.Account))
            {
                Debug.Log("Game - Too many characters already, aborting charactercreation: " + m.Connection.player.Account.Name);
                m.Connection.Disconnect();
                return;
            }
            var ch = CharacterCreationSelection.CreateNewCharacter(m);
            if (ch != null)
            {
                var response = PacketCreator.S2C_CREATE_CHARACTER_ACK(ch);
                m.Connection.SendMessage(response);
                m.Connection.player.CharacterCreationState = ECharacterCreationState.CCS_SELECT_CHARACTER;
            }
            else
            {
                Debug.Log("Game - Error creating new Character");
                m.Connection.Disconnect();
            }
        }

        void HandleSelectCharacter(Message m)
        {
            var characterID = m.ReadInt32();
            var p = m.Connection.player;
            var dbc = CharacterCreationSelection.GetAccountCharacter(p.Account, characterID);
            if (dbc != null)
            {
                var pc = PlayerCharacter.Create(p, dbc);
                p.ActiveCharacter = pc;
                p.CharacterCreationState = ECharacterCreationState.CCS_ENTER_WORLD;
                if (p.ActiveCharacter.LastZoneID == MapIDs.LOGIN || p.ActiveCharacter.LastZoneID == MapIDs.CHARACTER_SELECTION)
                    //case of crash at login for example, TODO handle better (dont set start 'maps' in the first place for example)
                {
                    var defaultZone = GameWorld.Instance.GetDefaultZone();
                    if (!defaultZone)
                    {
                        Debug.LogError("Default Zone not set!");
                        return;
                    }
                    p.ActiveCharacter.LastZoneID = defaultZone.ID;
                    p.ActiveCharacter.Position = defaultZone.FindNearestRespawn(Vector3.zero).transform.position;
                }
                p.LoadClientMap(p.ActiveCharacter.LastZoneID);
            }
            else
            {
                p.CharacterCreationState = ECharacterCreationState.CCS_PREPARE_UNIVERSE_ENTRY;
                p.Connection.Disconnect();
            }
        }

        void HandleDeleteCharacter(Message m)
        {
            var charID = m.ReadInt32();
            var success = CharacterCreationSelection.DeleteAccountCharacter(m.Connection.player.Account, charID);
            var response = PacketCreator.S2C_CS_DELETE_CHARACTER_ACK(charID, success);
            m.Connection.SendMessage(response);
        }

        void HandleWorldLoginAck(Message m)
        {
            var queuedCharacter = m.Connection.player.ActiveCharacter;
            if (queuedCharacter == null)
            {
                Debug.Log("WorldLogin error: no active Character");
                m.Connection.Disconnect();
                return;
            }
            m.Connection.player.IsIngame = true;
        }

        void HandleLogoutRequest(Message m)
        {
            var player = m.Connection.player;
            int accountCharID;
            if (player != null && player.ActiveCharacter != null)
            {
                accountCharID = player.ActiveCharacter.dbRef.DBID;
            }
            else
            {
                accountCharID = -1;
            }
            m.Connection.SendMessage(PacketCreator.S2C_WORLD_LOGOUT_ACK());
            if (player != null)
            {
                m.Connection.SendMessage(PacketCreator.S2C_USER_ON_LOGOUT(player.Account.UID, accountCharID));
            }
        }

        #endregion

        #region Chat

        void HandleChatMessage(Message m)
        {
            m.ReadInt32(); //int senderID = 
            var channelID = (EGameChatRanges) m.ReadByte();
            var target = m.ReadString();
            var message = m.ReadString();
            ChatHandler.HandleChatMessage(m.GetAssociatedCharacter(), channelID, target, message);
        }

        #endregion

        #region Transformation

        void HandleMovementUpdate(Message m)
        {
            var pc = m.GetAssociatedCharacter();
            m.ReadInt32(); //int unknown = 
            var position = UnitConversion.ToUnity(m.ReadVector3());
            var velocity = UnitConversion.ToUnity(m.ReadVector3());
            var frameNumber = m.ReadByte();
            pc.ReplicateMovement(position, velocity, frameNumber);
        }

        void HandleMovementWithPhysicsUpdate(Message m)
        {
            var pc = m.GetAssociatedCharacter();
            m.ReadInt32(); //int unknown = 
            var position = UnitConversion.ToUnity(m.ReadVector3());
            var velocity = UnitConversion.ToUnity(m.ReadVector3());
            var physics = (EPhysics) m.ReadByte();
            var frameNumber = m.ReadByte();
            pc.ReplicateMovementWithPhysics(position, velocity, physics, frameNumber);
        }

        void HandleUpdateRotation(Message m)
        {
            var pc = m.GetAssociatedCharacter();
            m.ReadInt32(); //characterID
            var rot = m.ReadInt32(); //yaw
            pc.ReplicateRotation(UnitConversion.ToUnity(new Rotator(0, rot, 0)));
        }

        #endregion

        #region Combat

        void HandleToggleWeapon(Message m)
        {
            var pc = m.GetAssociatedCharacter();
            if (pc.CombatMode != ECombatMode.CBM_Idle)
            {
                pc.SheatheWeapon();
            }
            else
            {
                pc.DrawWeapon();
            }
        }

        void HandleSwitchWeapon(Message m)
        {
            var pc = m.GetAssociatedCharacter();
            m.ReadInt32(); //int charID = 
            int weaponType = m.ReadByte();
            pc.SwitchWeapon((EWeaponCategory) weaponType);
        }

        void HandleUseSkill(Message m)
        {
            var pc = m.GetAssociatedCharacter();
            if (pc != null)
            {
                m.ReadInt32(); //int casterID = 
                var skillbarIndex = m.ReadInt32();
                var targetPos = UnitConversion.ToUnity(m.ReadVector3());
                m.ReadRotator(); //Rotator viewRotation = 
                var targetID = m.ReadInt32();
                var clientTime = m.ReadFloat();
                pc.ClientUseSkill(skillbarIndex, targetID, Vector3.zero, targetPos, clientTime);
            }
        }

        void HandleUseSkillLocation(Message m)
        {
            var pc = m.GetAssociatedCharacter();
            if (pc != null)
            {
                m.ReadInt32(); //int casterID = 
                var skillbarIndex = m.ReadInt32();
                var targetPos = UnitConversion.ToUnity(m.ReadVector3());
                var camPos = m.ReadVector3(); //CameraPos
                m.ReadRotator(); //viewRotation 
                var targetID = m.ReadInt32();
                var clientTime = m.ReadFloat();
                pc.ClientUseSkill(skillbarIndex, targetID, camPos, targetPos, clientTime);
            }
        }

        #endregion

        #region Emotes

        void HandleDoEmote(Message m)
        {
            var pc = m.GetAssociatedCharacter();
            m.ReadInt32(); //int characterID = 
            var emote = (EContentEmote) m.ReadByte();
            pc.DoEmote(emote);
        }

        #endregion

        #region Interaction

        void HandleInteractiveElementInteraction(Message m)
        {
            Debug.Log(Helper.ByteArrayToHex(m.Buffer));
        }

        #endregion

        #region QuestLog

        void HandleSwirlyOption(Message m)
        {
            //PlayerCharacter pc = m.GetAssociatedCharacter();
            Debug.Log("HandleSwirlyOption : " + Helper.ByteArrayToHex(m.Buffer));
        }

        void HandleSwirlyOptionPawn(Message m)
        {
            var pc = m.GetAssociatedCharacter();
            Debug.Log("HandleSwirlyOptionPawn : " + Helper.ByteArrayToHex(m.Buffer));

            m.ReadInt32(); //probably self RID
            var targetID = m.ReadInt32(); //probably target pawn RID
            var menuOption1 = (ERadialMenuOptions) m.ReadByte(); //probably radial menu option
            var menuOption2 = (ERadialMenuOptions) m.ReadByte(); //probably radial menu option

            Debug.Log("WorldServer.HandleSwirlyOptionPawn : Player = " + pc.Name + ",targetID = " + targetID + ",menuOption1 = " + menuOption1 + ",menuOption2 = " +
                      menuOption2);

            //TODO: Handle
            if (pc != null)
            {
                var npc = pc.ActiveZone.GetNpc(targetID);
                if (npc != null)
                {
                    npc.OnSwirlyOption(pc, menuOption1, menuOption2);
                }
            }
        }

        void HandleAbandonQuest(Message m)
        {
            var pc = m.GetAssociatedCharacter();
            m.ReadInt32(); //probably player RID
            var questID = m.ReadInt32();
            pc.RemoveQuest(questID);
        }

        #endregion

        #region Conversations

        void HandleConversationInteract(Message m)
        {
            Debug.Log("HandleConversationInteract : " + Helper.ByteArrayToHex(m.Buffer));
            //PlayerCharacter pc = m.GetAssociatedCharacter();
            m.ReadInt32(); //int selfID
            m.ReadInt32(); //int targetID

            /* TODO: Handle non-swirly interacts?
            if (pc != null)
            {
                NpcCharacter npc = pc.ActiveZone.GetNpc(targetID);
                if (npc != null)
                {                    
                    npc.Greet(pc, ERadialMenuOptions.RMO_CONVERSATION); 
                }
            }
            */
        }

        void HandleConversationRespond(Message m)
        {
            // Debug.Log("HandleConversationRespond : " + Helper.ByteArrayToHex(m.GetBytes()));

            var pc = m.GetAssociatedCharacter();
            ////TODO: not sure if player rID or target rID, probably needs NpcCharacter rID though
            var playerID = m.ReadInt32(); //Player rID
            var responseID = m.ReadInt32();
            Debug.Log("HandleConversationRespond : responseID = " + responseID + ", playerID = " + playerID);

            if (pc != null)
            {
                if (pc.currentConv != null)
                {
                    //NPC converses
                    pc.currentConv.partner.Converse(pc, responseID);
                }
            }
        }

        void HandleConversationReact(Message m)
        {
            var pc = m.GetAssociatedCharacter();
            m.ReadInt32(); //int selfID = 
            var targetID = m.ReadInt32();
            if (pc != null)
            {
                var npc = pc.ActiveZone.GetNpc(targetID);
                if (npc != null)
                {
                    npc.ReactTo(pc);
                }
            }
        }

        #endregion

        #region Teaming

        public TeamHandler teamHandler;

        void HandleTeamInvite(Message m)
        {
            var inviter = m.GetAssociatedCharacter();
            var existingTeamID = m.ReadInt32(); // probably existing teamID
            var targetName = m.ReadString();
            teamHandler.HandleInvite(inviter, existingTeamID, targetName);
        }

        void HandleTeamInviteAck(Message m)
        {
            var answerer = m.GetAssociatedCharacter();
            var requestedTeamID = m.ReadInt32();
            var answer = (eTeamRequestResult) m.ReadInt32();
            var requesterName = m.ReadString();
            teamHandler.handleInvitationAnswer(answerer, requestedTeamID, answer, requesterName);
        }

        void HandleTeamKick(Message m)
        {
            //Debug.Log(Helper.ByteArrayToHex(m.GetBytes()));
            var pc = m.GetAssociatedCharacter();
            m.ReadInt32(); //Unknown, teamID?
            var kickedID = m.ReadInt32();
            if (pc.Team != null)
            {
                pc.Team.Kick(pc, kickedID);
            }
        }

        void HandleTeamLeave(Message m)
        {
            var pc = m.GetAssociatedCharacter();

            m.ReadInt32(); //teamID? 
            if (pc.Team != null)
            {
                pc.Team.Leave(pc);
            }
        }

        void HandleTeamDisband(Message m)
        {
            var pc = m.GetAssociatedCharacter();
            if (pc.Team != null)
            {
                pc.Team.Disband(pc);
            }
        }

        void HandleTeamLeader(Message m)
        {
            var pc = m.GetAssociatedCharacter();
            m.ReadInt32(); //teamID?
            var newLeadID = m.ReadInt32();
            var newLead = pc.Team.GetMember(newLeadID);

            if (pc.Team != null)
            {
                pc.Team.SetLeader(pc, newLead);
            }
        }

        void HandleTeamLootMode(Message m)
        {
            //Debug.Log (Helper.ByteArrayToHex (m.GetBytes ()));

            var pc = m.GetAssociatedCharacter();
            m.ReadInt32(); //teamID?
            var lootMode = (ELootMode) m.ReadInt32();
            if (pc.Team != null)
            {
                pc.Team.SetLootMode(pc, lootMode);
            }
        }

        void HandleGetTeamInfo(Message m)
        {
            var pc = m.GetAssociatedCharacter();

            if (pc.Team != null)
            {
                //Debug.Log ("Sending team info to" + pc.Name);
                pc.Team.GetTeamInfoAck(pc, eTeamRequestResult.TRR_NONE);
            }
        }

        #endregion

        #region PlayerActions

        void HandleSitDown(Message m)
        {
            m.ReadInt32(); //relID
            var sitState = m.ReadInt32();
            var p = m.GetAssociatedCharacter();
            if (p)
            {
                p.Rest(sitState);
            }
        }

        void HandleSaveSkillDeckSkills(Message m)
        {
            m.ReadInt32(); //int relID
            var numSkills = m.ReadInt32();
            var deck = new int[numSkills];
            for (var i = 0; i < numSkills; i++)
            {
                deck[i] = m.ReadInt32();
            }
            var p = m.GetAssociatedCharacter();
            if (p)
            {
                p.SetSkillDeck(deck);
            }
        }

        void HandleLearnSkill(Message m)
        {
            m.ReadInt32(); //int relID
            var skillID = m.ReadInt32();
            var p = m.GetAssociatedCharacter();
            if (p)
            {
                p.LearnSkill(skillID);
            }
        }

        void HandleResurrect(Message m)
        {
            var p = m.GetAssociatedCharacter();
            if (p)
            {
                p.Resurrect();
            }
        }

        #endregion

        #region Items

        void HandleMoveItem(Message m)
        {
            m.ReadInt32(); //playerID
            var sourceLocType = (EItemLocationType) m.ReadByte();
            var sourceLocSlot = m.ReadInt32();
            var sourceLocID = m.ReadInt32();
            var targetLocType = (EItemLocationType) m.ReadByte();
            var targetLocSlot = m.ReadInt32();
            var targetLocID = m.ReadInt32();
            var p = m.GetAssociatedCharacter();
            if (p != null)
            {
                p.ItemManager.MoveItem(sourceLocType, sourceLocSlot, sourceLocID, targetLocType, targetLocSlot, targetLocID);
            }
        }

        #endregion
    }

    #endregion
}