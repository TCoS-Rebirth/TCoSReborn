using System.Collections.Generic;
#if UNITY
using Gameplay.Entities;
#endif

namespace Network
{
    public partial class Message
    {
        byte[] buffer = new byte[0];

        int position;

        public Message(GameHeader header)
        {
            Header = (ushort) header;
            buffer = new byte[0];
            position = 0;
        }

        public Message(LoginHeader header)
        {
            Header = (ushort) header;
            buffer = new byte[0];
            position = 0;
        }

        Message()
        {
        }

        public NetConnection Connection { get; set; }

        public ushort Header { get; private set; }

        public uint Size
        {
            get { return (uint) buffer.Length; }
        }

        public byte[] Buffer
        {
            get { return buffer; }
            set
            {
                buffer = value;
                position = 0;
            }
        }

        public int Position
        {
            get { return position; }
            set { position = value; }
        }

#if UNITY
        /// <summary>
        ///     Shortcut for NetConnection.player.ActiveCharacter
        /// </summary>
        public PlayerCharacter GetAssociatedCharacter()
        {
            return Connection.player.ActiveCharacter;
        }
#endif

        internal static Message CreateFromIncoming(NetConnection connection, int header, byte[] buf)
        {
            var m = new Message
            {
                Connection = connection,
                Header = (ushort) header,
                buffer = buf,
                position = 0
            };
            return m;
        }

        internal byte[] FinalizeForSending()
        {
            var b = new List<byte>();
            b.Add((byte) Header);
            b.Add((byte) (Header >> 8));
            b.Add((byte) Size);
            b.Add((byte) (Size >> 8));
            b.AddRange(Buffer);
            return b.ToArray();
        }
    }

    public delegate void HandleMessageCallback(Message m);
}