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

            return result; 
        }

        public static void WriteString(FileStream stream, string data, int length)
        {
            var encoding = Encoding.GetEncoding("shift_jis");
            var buffer = encoding.GetBytes(data);

            if(buffer.Length != length)
            {
                throw new InvalidDataException($"{buffer.Length} != {length}");
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
    }
}
