using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PackageExtractor
{
    public class SBFileReader : IDisposable
    {
        BinaryReader accessor;
        //private FileInfo info;

        public SBFileReader(string fullFilePath)
        {
            try
            {
                var stream = File.OpenRead(fullFilePath);
                accessor = new BinaryReader(stream);
            }
            catch
            {
                throw new NullReferenceException("Package file could not be opened");
            }
        }

        public long Position
        {
            get { return accessor.BaseStream.Position; }
        }

        public void Dispose()
        {
            accessor.Close();
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            accessor.BaseStream.Seek(offset, origin);
        }

        public void Read<T>(out T structure, int structSize) where T : struct
        {
            var bytes = accessor.ReadBytes(structSize);
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            structure = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof (T));
            handle.Free();
        }

        public byte ReadByte()
        {
            var result = accessor.ReadByte();
            return result;
        }

        public byte[] ReadBytes(int count)
        {
            var result = new byte[count];
            result = accessor.ReadBytes(count);
            return result;
        }

        public short ReadInt16()
        {
            var result = accessor.ReadInt16();
            return result;
        }

        public uint ReadUInt32()
        {
            var result = accessor.ReadUInt32();
            return result;
        }

        public int ReadInt32()
        {
            var result = accessor.ReadInt32();
            return result;
        }


        public float ReadFloat()
        {
            var result = accessor.ReadSingle();
            return result;
        }

        public long ReadInt64()
        {
            var result = accessor.ReadInt64();
            return result;
        }

        //Helper function to avoid API break: allows to call ReadIndex without parameters
        public int ReadIndex()
        {
            var dummy = 0; //this var is not used

            var index = ReadIndex(ref dummy);

            return index;
        }

        public int ReadIndex(ref int indexSize)
        {
            var isNegative = false;
            var firstByte = ReadByte();
            indexSize = 1;
            var index = firstByte & 0x3F;
            if ((firstByte & 0x80) != 0)
                isNegative = true;

            if ((firstByte & 0x40) != 0)
            {
                var secondByte = ReadByte();
                ++indexSize;
                index = index | ((secondByte & 0x7F) << 6);
                if ((secondByte & 0x80) != 0)
                {
                    var thirdByte = ReadByte();
                    ++indexSize;
                    index = index | ((thirdByte & 0x7F) << 13);
                    if ((thirdByte & 0x80) != 0)
                    {
                        var fourthByte = ReadByte();
                        ++indexSize;
                        index = index | ((fourthByte & 0x7f) << 20);
                        if ((fourthByte & 0x80) != 0)
                        {
                            var fifthByte = ReadByte();
                            ++indexSize;
                            index = index | ((fifthByte & 0x7F) << 27);
                        }
                    }
                }
            }
            if (isNegative)
                index = -index;

            return index;
        }
    }
}