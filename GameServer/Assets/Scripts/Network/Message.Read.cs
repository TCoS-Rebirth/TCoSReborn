using System;
using System.Text;
#if UNITY
using Common.UnrealTypes;
using UnityEngine;
#endif

namespace Network
{
    public partial class Message
    {
        //HACK try bitshifting

        void LogOverflow()
        {
            Debug.Log("buffer read overflow");
        }

        public byte ReadByte()
        {
            if (position + 1 > buffer.Length)
            {
                LogOverflow();
                return 0;
            }
            var b = buffer[position];
            position += 1;
            return b;
        }

        public byte[] ReadByteArray()
        {
            var arraySize = ReadUInt32();
            var arr = new byte[arraySize];
            if (position + arraySize > buffer.Length)
            {
                LogOverflow();
                return arr;
            }
            for (var i = 0; i < arraySize; i++)
            {
                arr[i] = ReadByte();
            }
            return arr;
        }

        public int[] ReadIntArray()
        {
            var arraySize = ReadUInt32();
            var arr = new int[arraySize];
            if (position + arraySize > buffer.Length)
            {
                LogOverflow();
                return arr;
            }
            for (var i = 0; i < arraySize; i++)
            {
                arr[i] = ReadInt32();
            }
            return arr;

        }

        public ushort ReadUInt16()
        {
            if (position + 2 > buffer.Length)
            {
                LogOverflow();
                return 0;
            }
            var u = BitConverter.ToUInt16(buffer, position);
            position += 2;
            return u;
        }

        public ushort PeekUInt16()
        {
            if (Position + 2 > buffer.Length)
            {
                LogOverflow();
                return 0;
            }
            var u = BitConverter.ToUInt16(buffer, position);
            return u;
        }

        public int ReadInt32()
        {
            if (position + 4 > buffer.Length)
            {
                LogOverflow();
                return 0;
            }
            var i = BitConverter.ToInt32(buffer, position);
            position += 4;
            return i;
        }

        public uint ReadUInt32()
        {
            if (position + 4 > buffer.Length)
            {
                LogOverflow();
                return 0;
            }
            var u = BitConverter.ToUInt32(buffer, position);
            position += 4;
            return u;
        }

        public ulong ReadUInt64()
        {
            if (position + 8 > buffer.Length)
            {
                LogOverflow();
                return 0;
            }
            var u = BitConverter.ToUInt64(buffer, position);
            position += 8;
            return u;
        }

        public float ReadFloat()
        {
            if (position + 4 > buffer.Length)
            {
                LogOverflow();
                return 0;
            }
            var s = BitConverter.ToSingle(buffer, position);
            position += 4;
            return s;
        }

        public string ReadString()
        {
            var length = (int) ReadUInt32();
            var str = new byte[length*2];
            for (var i = 0; i < str.Length; i++)
            {
                str[i] = ReadByte();
            }
            return Encoding.Unicode.GetString(str);
        }

#if UNITY
        public Vector3 ReadVector3()
        {
            var vec = new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
            return vec;
        }

        public Rotator ReadRotator()
        {
            return new Rotator(ReadInt32(), ReadInt32(), ReadInt32());
        }
#endif
    }
}