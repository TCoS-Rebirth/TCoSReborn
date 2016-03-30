using System;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using Common;
using Network;
using NetConnection = Lidgren.Network.NetConnection;

namespace LoginServer
{
    public class LoginServer
    {
        readonly Dictionary<LoginHeader, Action<Message>> _dispatchTable = new Dictionary<LoginHeader, Action<Message>>();

        readonly Queue<Message> _incomingMessages = new Queue<Message>();

        readonly HashSet<int> _usedTransferKeys = new HashSet<int>();

        DateTime _lastPopulationUpdate;
        NetServer _proxyServer;

        NetConnector _server;

        public int GenerateTransferKey()
        {
            var newKey = new Random(DateTime.Now.Minute).Next();
            while (_usedTransferKeys.Contains(newKey))
            {
                newKey = new Random(DateTime.Now.Second).Next();
            }
            return newKey;
        }

        public bool StartServer(LoginServerConfiguration config)
        {
            _server = new NetConnector(config.ListenIP, config.ListenPort, _incomingMessages);
            _dispatchTable.Add(LoginHeader.C2L_USER_LOGIN, HandleAuthChallenge);
            _dispatchTable.Add(LoginHeader.C2L_QUERY_UNIVERSE_LIST, HandleQueryUniverseList);
            _dispatchTable.Add(LoginHeader.C2L_UNIVERSE_SELECTED, HandleUniverseSelection);
            _dispatchTable.Add(LoginHeader.DISCONNECT, HandleDisconnect);
            _server.OnDisconnected += OnDisconnect;
            var pConfig = new NetPeerConfiguration("TCoSReborn");
            pConfig.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            pConfig.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            pConfig.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            pConfig.ConnectionTimeout = 5f;
            pConfig.Port = config.ProxyServerListenPort;
            _proxyServer = new NetServer(pConfig);
            _proxyServer.Start();
            _lastPopulationUpdate = DateTime.Now;
            return _server.Start();
        }

        public int GetConnectionCount()
        {
            return _server.GetConnectionCount();
        }

        public void UpdateMessageQueue()
        {
            while (_incomingMessages.Count > 0)
            {
                var m = _incomingMessages.Dequeue();
                Action<Message> messageHandler;
                if (!Enum.IsDefined(typeof (LoginHeader), m.Header))
                {
                    Debug.Log("no messageType defined for: " + m.Header, ConsoleColor.Red);
                    continue;
                }
                if (_dispatchTable.TryGetValue((LoginHeader) m.Header, out messageHandler))
                {
                    messageHandler(m);
                }
                else
                {
                    Debug.Log("no messageHandler defined for: " + (LoginHeader) m.Header, ConsoleColor.Red);
                }
            }
            NetIncomingMessage msg;
            while ((msg = _proxyServer.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.UnconnectedData:
                        HandleUnconnectedLauncherRequest(msg);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        HandleProxyConnectionStatusChanged(msg, (NetConnectionStatus) msg.ReadByte());
                        break;
                    case NetIncomingMessageType.Data:
                        HandleProxyData(msg);
                        break;
                    case NetIncomingMessageType.DiscoveryRequest:
                        _proxyServer.SendDiscoveryResponse(_proxyServer.CreateMessage("LoginServer"), msg.SenderEndPoint);
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        Debug.Log(msg.ReadString(), ConsoleColor.Red);
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        Debug.Log(msg.ReadString(), ConsoleColor.Red);
                        break;
                }
            }
            if (DateTime.Now - _lastPopulationUpdate > TimeSpan.FromMilliseconds(Program.Config.PopulationRefreshInterval))
            {
                _lastPopulationUpdate = DateTime.Now;
                var updateRequest = _proxyServer.CreateMessage();
                updateRequest.Write((byte)CommunicationHeader.L2U_QUERY_POPULATION);
                _proxyServer.SendToAll(updateRequest, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void ShutDown()
        {
            if (_server != null)
            {
                _server.Shutdown();
            }
        }

        #region Handler

        #region Proxy

        public List<UniverseInfo> GetConnectedUniverses()
        {
            var unis = new List<UniverseInfo>();
            for (var i = 0; i < _proxyServer.ConnectionsCount; i++)
            {
                var uInfo = _proxyServer.Connections[i].Tag as UniverseInfo;
                if (uInfo != null)
                {
                    unis.Add(uInfo);
                }
            }
            return unis;
        }

        public List<NetConnection> GetUniverseConnections()
        {
            var cons = new List<NetConnection>();
            for (var i = 0; i < _proxyServer.ConnectionsCount; i++)
            {
                if (_proxyServer.Connections[i].Tag is UniverseInfo)
                {
                    cons.Add(_proxyServer.Connections[i]);
                }
            }
            return cons;
        }

        void HandleProxyConnectionStatusChanged(NetIncomingMessage msg, NetConnectionStatus status)
        {
            switch (status)
            {
                case NetConnectionStatus.Connected:
                    if (msg.SenderConnection.RemoteHailMessage.LengthBytes > 0)
                    {
                        var hmsg = msg.SenderConnection.RemoteHailMessage;
                        var header = (CommunicationHeader)hmsg.ReadByte();

                        if (header == CommunicationHeader.U2L_REGISTER_UNIVERSE)
                        {
                            var uName = hmsg.ReadString();
                            var uLanguage = hmsg.ReadString();
                            var uType = hmsg.ReadString();
                            var uRestriction = (AccountPrivilege) hmsg.ReadByte();
                            var uIp = hmsg.ReadString();
                            var uPort = hmsg.ReadInt32();
                            var knownUniverses = DBAccess.GetKnownUniverses();
                            var uId = knownUniverses.Count;
                            var newUInfo = new UniverseInfo(uId, uName, uLanguage, uType, 0, uRestriction, uIp, uPort);
                            msg.SenderConnection.Tag = newUInfo;
                            for (var i = 0; i < knownUniverses.Count; i++)
                            {
                                if (!knownUniverses[i].Name.Equals(uName, StringComparison.OrdinalIgnoreCase)) continue;
                                newUInfo.Id = knownUniverses[i].Id;
                                Debug.Log("Universe reconnected: " + newUInfo.Name, ConsoleColor.Green);
                                return;
                            }
                            knownUniverses.Add(newUInfo);
                            DBAccess.UpdateKnownUniverses(knownUniverses);
                            Debug.Log("Universe connected: " + newUInfo.Name, ConsoleColor.Green);
                        }
                        else
                        {
                            Debug.Log("Non Universe connection attempt. Disconnecting", ConsoleColor.Yellow);
                            msg.SenderConnection.Disconnect("Nope");
                        }
                    }
                    break;
                case NetConnectionStatus.Disconnected:
                    var uInfo = msg.SenderConnection.Tag as UniverseInfo;
                    if (uInfo != null)
                    {
                        Debug.Log("Universe disconnected: " + uInfo.Name, ConsoleColor.Yellow);
                    }
                    msg.SenderConnection.Tag = null;
                    break;
            }
        }

        void HandleProxyData(NetIncomingMessage msg)
        {
            if (msg.LengthBytes < 1)
            {
                var uInfo = msg.SenderConnection.Tag as UniverseInfo;
                if (uInfo != null)
                {
                    Debug.Log("invalid proxy message from Universe received: " + uInfo.Name, ConsoleColor.Red);
                    return;
                }
                Debug.Log("invalid proxy message received", ConsoleColor.Red);
                return;
            }
            var header = (CommunicationHeader) msg.ReadByte();
            switch (header)
            {
                case CommunicationHeader.INVALID:
                    Debug.Log("invalid proxy message received", ConsoleColor.Red);
                    break;
                case CommunicationHeader.U2L_UPDATE_POPULATION:
                    var updatedUniverse = msg.SenderConnection.Tag as UniverseInfo;
                    if (updatedUniverse == null)
                    {
                        Debug.Log("Population update message for non Universe connection received", ConsoleColor.Red);
                        break;
                    }
                    updatedUniverse.Population = msg.ReadInt32();
                    break;
                case CommunicationHeader.U2L_ACCOUNT_REQUESTLOGIN_ACK:
                    var success = msg.ReadBoolean();
                    var accName = msg.ReadString();
                    var accPass = msg.ReadString();
                    var tKey = msg.ReadInt32();
                    var connection = _server.GetConnection(con =>
                    {
                        var pInfo = con.player;
                        if (pInfo == null) return false;
                        var acc = pInfo.Account;
                        if (acc == null) return false;
                        return acc.Name == accName && acc.PasswordHash == accPass;
                    });
                    var sourceUniverse = msg.SenderConnection.Tag as UniverseInfo;
                    if (connection != null)
                    {
                        if (sourceUniverse != null)
                        {
                            var outMsg = L2C_UNIVERSE_SELECTED_ACK(tKey, success ? sourceUniverse : null);
                            connection.SendMessage(outMsg);
                        }   
                    }
                    break;
            }
        }

        void HandleUnconnectedLauncherRequest(NetIncomingMessage msg)
        {
            if (msg.LengthBytes < 1)
            {
                return;
            }
            var header = (CommunicationHeader) msg.ReadByte();
            switch (header)
            {
                case CommunicationHeader.CL2L_QUERY_INFO:
                    var m = _proxyServer.CreateMessage();
                    m.Write((byte) CommunicationHeader.L2CL_UPDATE_INFO);
                    m.Write(Program.Config.RegistrationAllowed);
                    m.Write(GetConnectedUniverses().Count > 0);
                    m.Write(Program.Config.Message);
                    _proxyServer.SendUnconnectedMessage(m, msg.SenderEndPoint);
                    break;
                case CommunicationHeader.CL2L_REGISTER_ACCOUNT:
                    var rm = _proxyServer.CreateMessage();
                    rm.Write((byte) CommunicationHeader.L2CL_REGISTER_ACCOUNT_ACK);
                    if (!Program.Config.RegistrationAllowed)
                    {
                        rm.Write(3);
                        _proxyServer.SendUnconnectedMessage(rm, msg.SenderEndPoint);
                        break;
                    }
                    var name = msg.ReadString();
                    var pass = msg.ReadString();
                    var mail = msg.ReadString();
                    var res = DBAccess.RegisterAccont(name, pass, mail, (int)AccountPrivilege.Player);
                    if (res == 0)
                    {
                        Debug.Log("Account created for: " + name);
                    }
                    rm.Write(res);
                    _proxyServer.SendUnconnectedMessage(rm, msg.SenderEndPoint);
                    break;
            }
        }

        #endregion

        void HandleAuthChallenge(Message m)
        {
            var clientVersion = (int) m.ReadUInt32();
            var name = m.ReadString();
            var password = m.ReadString();
            if (DBAccess.GetSupportedClientVersion() != clientVersion)
            {
                SendAuthResult(m.Connection, eLoginRequestResult.LRR_INVALID_REVISION);
                return;
            }
            var acc = DBAccess.GetAccount(name);
            if (acc == null)
            {
                SendAuthResult(m.Connection, eLoginRequestResult.LRR_INVALID_USERNAME);
                return;
            }
            if (acc.Banned)
            {
                SendAuthResult(m.Connection, eLoginRequestResult.LRR_BANNED_ACCOUNT);
                return;
            }
            if (acc.IsOnline)
            {
                for (var i = 0; i < _proxyServer.ConnectionsCount; i++)
                {
                    UniverseInfo uInfo = _proxyServer.Connections[i].Tag as UniverseInfo;
                    if (uInfo == null) continue;
                    if (uInfo.Id != acc.LastUniverse) continue;
                    var msg = _proxyServer.CreateMessage();
                    msg.Write((byte)CommunicationHeader.L2U_CLEANUP_LEFTOVER_ACCOUNT);
                    msg.Write(acc.UID);
                    _proxyServer.Connections[i].SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
                    
                    break;
                }
            }
            if (acc.PasswordHash != UserAccount.GetSha1Hash(password))
            {
                SendAuthResult(m.Connection, eLoginRequestResult.LRR_INVALID_PASSWORD);
            }
            else
            {
                acc.LastLogin = DateTime.Now;
                acc.SessionKey = GenerateTransferKey();
                if (DBAccess.SaveAccount(acc))
                {
                    m.Connection.player = new PlayerInfo(acc, m.Connection);
                    Debug.Log("Player authenticated: " + acc.Name, ConsoleColor.Yellow);
                    SendAuthResult(m.Connection, eLoginRequestResult.LRR_NONE);
                }
                else
                {
                    Debug.Log("Error updating account for login", ConsoleColor.Red);
                    SendAuthResult(m.Connection, eLoginRequestResult.LRR_LOGIN_CONNECT_FAILED);
                }
            }
        }

        static void SendAuthResult(Network.NetConnection netConnection, eLoginRequestResult result)
        {
            var m = L2C_USER_LOGIN_ACK(result);
            netConnection.SendMessage(m);
        }

        void HandleQueryUniverseList(Message m)
        {
            var outMessage = L2C_QUERY_UNIVERSE_LIST_ACK(m);
            m.Connection.SendMessage(outMessage);
        }

        void HandleUniverseSelection(Message m)
        {
            var selectedID = m.ReadInt32();
            var universes = GetUniverseConnections();
            var acc = m.Connection.player.Account;
            for (var i = 0; i < universes.Count; i++)
            {
                var uInfo = universes[i].Tag as UniverseInfo;
                if (uInfo.Id != selectedID) continue;
                var notificationMsg = _proxyServer.CreateMessage();
                notificationMsg.Write((byte)CommunicationHeader.L2U_ACCOUNT_REQUESTLOGIN);
                notificationMsg.Write(acc.Name);
                notificationMsg.Write(acc.PasswordHash);
                notificationMsg.Write(acc.SessionKey);
                var res = universes[i].SendMessage(notificationMsg, NetDeliveryMethod.ReliableOrdered, 0);
                return;
            }
            Debug.Log("Error transfering session to Universe", ConsoleColor.Red);
            var errorMsg = L2C_UNIVERSE_SELECTED_ACK(-1, null);
            m.Connection.SendMessage(errorMsg);
        }

        static void HandleDisconnect(Message m)
        {
            if (m.Connection.player != null & m.Connection.player.Account != null)
            {
                Debug.Log(m.Connection.player.Account.Name + " disconnected");
            }
        }

        static void OnDisconnect(Network.NetConnection con)
        {
            if (con.player != null && con.player.Account != null)
            {
                Debug.Log("Player disconnected: " + con.player.Account.Name, ConsoleColor.Yellow);
            }
        }

        #endregion

        #region Packet creation

        public static Message L2C_USER_LOGIN_ACK(eLoginRequestResult result)
        {
            var m = new Message(LoginHeader.L2C_USER_LOGIN_ACK);
            m.WriteInt32((int) MessageStatusCode.NO_ERROR);
            m.WriteInt32((int) result);
            return m;
        }

        public Message L2C_QUERY_UNIVERSE_LIST_ACK(Message inMessage)
        {
            var acc = inMessage.Connection.player.Account;
            var m = new Message(LoginHeader.L2C_QUERY_UNIVERSE_LIST_ACK);
            m.WriteInt32((int) MessageStatusCode.NO_ERROR);
            var infos = GetConnectedUniverses();
            for (var i = infos.Count;i-->0;)
            {
                if (acc.Level < infos[i].MinPrivilege) infos.RemoveAt(i);
            }
            m.WriteInt32(infos.Count);
            for (var i = 0; i < infos.Count; i++)
            {
                m.WriteInt32(infos[i].Id);
                m.WriteString(infos[i].Name);
                m.WriteString(infos[i].Language);
                m.WriteString(infos[i].Type);
                m.WriteString(infos[i].Population.ToString());
            }
            return m;
        }

        public static Message L2C_UNIVERSE_SELECTED_ACK(int key, UniverseInfo selected)
        {
            var m = new Message(LoginHeader.L2C_UNIVERSE_SELECTED_ACK);
            m.WriteInt32(selected != null ? (int) MessageStatusCode.NO_ERROR : (int) MessageStatusCode.UNKNOWN_ERROR);
            m.WriteInt32(0);
            m.WriteString("Complete_Universe");
            m.WriteInt32(selected != null ? key : -1);
            m.WriteByteArrayWithoutLength(selected != null ? IPAddress.Parse(selected.Ip).GetAddressBytes() : new byte[0]);
            m.WriteUint16(selected != null ? (ushort) selected.Port : (ushort) 0);
            return m;
        }

        #endregion
    }
}