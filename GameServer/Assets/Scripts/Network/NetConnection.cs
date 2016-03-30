using System.Collections.Generic;
using System.Net.Sockets;
#if UNITY
using Gameplay.Entities;
using World;
#endif

namespace Network
{
    public class NetConnection
    {
        public Queue<Message> MessageQueue = new Queue<Message>();

        public NetConnection(Socket socket)
        {
            ClientSocket = socket;
        }

        public Socket ClientSocket { get; private set; }

        public PlayerInfo player { get; set; }
#if UNITY
        /// <summary>
        ///     Shortcut for player.ActiveCharacter
        /// </summary>
        public PlayerCharacter GetAssociatedCharacter()
        {
            return player.ActiveCharacter;
        }
#endif

        public void Disconnect()
        {
            if (ClientSocket.Connected)
            {
                ClientSocket.Disconnect(false);
            }
        }

        /// <summary>
        ///     Use this to send network messages to the client
        /// </summary>
        public void SendMessage(Message msg)
        {
            if (ClientSocket == null || !ClientSocket.Connected)
            {
                return;
            }
            msg.Connection = this;
            lock (MessageQueue)
            {
                MessageQueue.Enqueue(msg);
            }
        }

        #region Internal

        internal void SetupReceiveState(ReceiveState newState)
        {
            State = newState;
            switch (State)
            {
                case ReceiveState.Header:
                    incomingReadBuffer = new byte[4];
                    break;
                case ReceiveState.Body:
                    incomingMessageHeader = (ushort) (incomingReadBuffer[0] | incomingReadBuffer[1] << 8);
                    incomingMessageSize = (ushort) (incomingReadBuffer[2] | incomingReadBuffer[3] << 8);
                    incomingReadBuffer = new byte[incomingMessageSize];
                    break;
            }
        }

        internal Message ReceiveMessage()
        {
            var m = Message.CreateFromIncoming(this, incomingMessageHeader, incomingReadBuffer);
            incomingReadBuffer = new byte[4];
            State = ReceiveState.Header;
            return m;
        }

        internal ushort incomingMessageHeader;
        internal ushort incomingMessageSize;
        internal byte[] incomingReadBuffer;

        internal enum ReceiveState
        {
            Header,
            Body
        }

        internal ReceiveState State = ReceiveState.Header;

        #endregion
    }
}