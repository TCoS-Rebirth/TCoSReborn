using System;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using Common;
using Database.Dynamic;
using UnityEngine;
using World;
using Random = System.Random;

namespace Network
{
    public class LoginServer: MonoBehaviour
    {
        readonly Dictionary<LoginHeader, Action<Message>> _dispatchTable = new Dictionary<LoginHeader, Action<Message>>();

        readonly Queue<Message> _incomingMessages = new Queue<Message>();

        readonly HashSet<int> _usedTransferKeys = new HashSet<int>();

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

        public bool StartServer(ServerConfiguration config)
        {
            _server = new NetConnector(config.LoginServerPort, _incomingMessages);
            _dispatchTable.Add(LoginHeader.C2L_USER_LOGIN, HandleAuthChallenge);
            _dispatchTable.Add(LoginHeader.C2L_QUERY_UNIVERSE_LIST, HandleQueryUniverseList);
            _dispatchTable.Add(LoginHeader.C2L_UNIVERSE_SELECTED, HandleUniverseSelection);
            _dispatchTable.Add(LoginHeader.DISCONNECT, HandleDisconnect);
            _server.OnDisconnected += OnDisconnect;
            return _server.Start();
        }

        public int GetConnectionCount()
        {
            return _server.GetConnectionCount();
        }

        void Update()
        {
            while (_incomingMessages.Count > 0)
            {
                Message m;
                lock (_incomingMessages)
                {
                    m = _incomingMessages.Dequeue();
                }
                Action<Message> messageHandler;
                if (!Enum.IsDefined(typeof (LoginHeader), m.Header))
                {
                    Debug.LogWarning("no messageType defined for: " + m.Header);
                    continue;
                }
                if (_dispatchTable.TryGetValue((LoginHeader) m.Header, out messageHandler))
                {
                    messageHandler(m);
                }
                else
                {
                    Debug.LogWarning("no messageHandler defined for: " + (LoginHeader) m.Header);
                }
                
            }
        }

        public void ShutDown()
        {
            Debug.Log("Shutting down LoginServer");
            if (_server != null)
            {
                _server.Shutdown();
            }
            Debug.Log("LoginServer shut down");
        }

        #region Handler

        void HandleAuthChallenge(Message m)
        {
            var clientVersion = (int) m.ReadUInt32();
            var name = m.ReadString();
            var password = m.ReadString();
            if (GameWorld.Instance.GameConfig.SupportedClientVersion != clientVersion)
            {
                SendAuthResult(m.Connection, eLoginRequestResult.LRR_INVALID_REVISION);
                return;
            }
            var acc = DB.AccountDB.GetAccount(name);
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
                SendAuthResult(m.Connection, eLoginRequestResult.LRR_LOGIN_ADD_FAILED);
            }
            if (acc.PasswordHash != UserAccount.GetSha1Hash(password))
            {
                SendAuthResult(m.Connection, eLoginRequestResult.LRR_INVALID_PASSWORD);
            }
            else
            {
                acc.LastLogin = DateTime.Now;
                acc.SessionKey = GenerateTransferKey();
                if (DB.AccountDB.UpdateAccount(acc))
                {
                    m.Connection.player = new PlayerInfo(acc, m.Connection);
                    Debug.Log("Player authenticated: " + acc.Name);
                    SendAuthResult(m.Connection, eLoginRequestResult.LRR_NONE);
                }
                else
                {
                    Debug.LogWarning("Error updating account for login");
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
            /*var selectedID = */m.ReadInt32();
            var msg = L2C_UNIVERSE_SELECTED_ACK(m.Connection.player.Account.SessionKey);
            m.Connection.SendMessage(msg);
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
                Debug.Log("Player disconnected: " + con.player.Account.Name);
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
            var config = GameWorld.Instance.ServerConfig;
            var acc = inMessage.Connection.player.Account;
            var m = new Message(LoginHeader.L2C_QUERY_UNIVERSE_LIST_ACK);
            m.WriteInt32((int) MessageStatusCode.NO_ERROR);
            m.WriteInt32(1);
            m.WriteInt32(0);
            m.WriteString(config.ServerName);
            m.WriteString(config.ServerLanguage);
            m.WriteString("PVE");
            m.WriteString(GameWorld.Instance.PlayerPopulation.ToString());
            return m;
        }

        public static Message L2C_UNIVERSE_SELECTED_ACK(int key)
        {
            var config = GameWorld.Instance.ServerConfig;
            var m = new Message(LoginHeader.L2C_UNIVERSE_SELECTED_ACK);
            m.WriteInt32((int) MessageStatusCode.NO_ERROR);
            m.WriteInt32(0);
            m.WriteString("Complete_Universe");
            m.WriteInt32(key);
            m.WriteByteArrayWithoutLength(IPAddress.Parse(config.PublicIP).GetAddressBytes());
            m.WriteUint16((ushort) config.GameServerPort);
            return m;
        }

        #endregion
    }
}