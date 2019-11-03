using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    public static class BinaryHelper
    {
        public static string ReadString(FileStream stream, int length)
        {



            var buffer = new byte[length];
            stream.Read(buffer, 0, length);

            var encoding = Encoding.GetEncoding("shift_jis");
            var result = encoding.GetString(buffer);
            result = result.Trim('\0'); // remove nulls 

            return result; 
        }

        public static void WriteString(FileStream stream, string data, int length)
        {
            var encoding = Encoding.GetEncoding("shift_jis");
            var buffer = encoding.GetBytes(data);

            // grow or truncate.. 
            if(buffer.Length != length)
            {
                var temp = buffer;
                buffer = new byte[length];

                for(var i = 0; i < Math.Min(length, temp.Length); ++i)
                {
                    buffer[i] = temp[i];
                }
            }

            stream.Write(buffer, 0, buffer.Length);
        }

        public static ushort ReadUInt16(FileStream stream)
        {
            ushort result = 0;

            var b0 = stream.ReadByte();
            var b1 = stream.ReadByte();
            
            result += (ushort) (b1 << 08);
            result += (ushort) (b0 << 00);

            return result; 
        }

        public static void WriteUInt16(FileStream stream, ushort data)
        {
            stream.WriteByte( (byte) (data >> 00) );
            stream.WriteByte( (byte) (data >> 08) );
        }

        public static ushort ReadUInt16(byte[] buffer)
        {
            ushort result = 0;

            var b0 = buffer[0];
            var b1 = buffer[1];

            result += (ushort)(b1 << 08);
            result += (ushort)(b0 << 00);

            return result;
        }

        public static void WriteUInt16(byte[] buffer, ushort data)
        {
            buffer[0] = (byte)(data >> 00);
            buffer[1] = (byte)(data >> 08);
        }

        public static uint ReadUInt32(FileStream stream)
        {
            uint result = 0;

            var b0 = stream.ReadByte();
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            var b3 = stream.ReadByte();

            result += ( (uint) b3 << 24 );
            result += ( (uint) b2 << 16 );
            result += ( (uint) b1 << 08 );
            result += ( (uint) b0 << 00 );

            return result;
        }

        public static void WriteUInt32(FileStream stream, uint data)
        {
            stream.WriteByte((byte)(data >> 00));
            stream.WriteByte((byte)(data >> 08));
            stream.WriteByte((byte)(data >> 16));
            stream.WriteByte((byte)(data >> 24));
        }

        public static uint ReadUInt32(byte[] buffer)
        {
            uint result = 0;

            var b0 = buffer[0];
            var b1 = buffer[1];
            var b2 = buffer[2];
            var b3 = buffer[3];

            result += ((uint)b3 << 24);
            result += ((uint)b2 << 16);
            result += ((uint)b1 << 08);
            result += ((uint)b0 << 00);

            return result;
        }

        public static void WriteUInt32(byte[] buffer, uint data)
        {
            buffer[0] = (byte)(data >> 00);
            buffer[1] = (byte)(data >> 08);
            buffer[2] = (byte)(data >> 16);
            buffer[3] = (byte)(data >> 24);
        }

        public static void WritePadding(FileStream stream, long length)
        {
            var buffer = new byte[length];
            stream.Write(buffer, 0, buffer.Length);
        }

        public static void WriteAFSPadding(FileStream stream, long streamPosition, long blockSize = 2048L)
        {
            if(streamPosition % blockSize != 0)
            {
                var padding = blockSize - streamPosition % blockSize;
                WritePadding(stream, padding); 
            }
        }
    }
}
