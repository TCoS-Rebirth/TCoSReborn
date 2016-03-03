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
        void EnsureBuffersize(int more)
        {
            if (position + more > buffer.Length)
            {
                Array.Resize(ref buffer, buffer.Length + more);
            }
        }

        public void WriteByte(byte b)
        {
            EnsureBuffersize(1);
            buffer[position] = b;
            position += 1;
        }

        public void WriteByteArray(byte[] b)
        {
            WriteUint32((ushort) b.Length);
            EnsureBuffersize(b.Length);
            System.Buffer.BlockCopy(b, 0, buffer, position, b.Length);
            position += b.Length;
        }

        public void WriteByteArrayWithoutLength(byte[] b)
        {
            EnsureBuffersize(b.Length);
            System.Buffer.BlockCopy(b, 0, buffer, position, b.Length);
            position += b.Length;
        }

        public void WriteUint16(ushort s)
        {
            var b = BitConverter.GetBytes(s);
            EnsureBuffersize(b.Length);
            System.Buffer.BlockCopy(b, 0, buffer, position, b.Length);
            position += b.Length;
        }

        public void WriteUint32(uint i)
        {
            var b = BitConverter.GetBytes(i);
            EnsureBuffersize(b.Length);
            System.Buffer.BlockCopy(b, 0, buffer, position, b.Length);
            position += b.Length;
        }

        public void WriteInt32(int i)
        {
            var b = BitConverter.GetBytes(i);
            EnsureBuffersize(b.Length);
            System.Buffer.BlockCopy(b, 0, buffer, position, b.Length);
            position += b.Length;
        }

        public void WriteFloat(float f)
        {
            var b = BitConverter.GetBytes(f);
            EnsureBuffersize(b.Length);
            System.Buffer.BlockCopy(b, 0, buffer, position, b.Length);
            position += b.Length;
        }

        public void WriteString(string s)
        {
            var b = Encoding.Unicode.GetBytes(s);
            WriteUint32((uint) s.Length);
            EnsureBuffersize(b.Length);
            System.Buffer.BlockCopy(b, 0, buffer, position, b.Length);
            position += b.Length;
        }

#if UNITY
        public void WriteVector3(Vector3 vec)
        {
            WriteFloat(vec.x);
            WriteFloat(vec.y);
            WriteFloat(vec.z);
        }

        public void WriteRotator(Rotator rot)
        {
            WriteInt32(rot.Pitch);
            WriteInt32(rot.Yaw);
            WriteInt32(rot.Roll);
        }
#endif
    }
}