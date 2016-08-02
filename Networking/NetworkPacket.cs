using System;

namespace DAEmu.Networking
{
    public class NetworkPacket
    {
        public byte Command { get; set; }
        public byte Ordinal { get; set; }
        public byte[] Data { get; set; }

        public NetworkPacket(byte[] array, int start, int count)
        {
            Command = array[start + 0];
            Ordinal = array[start + 1];
            Data = new byte[count - 2];

            if (Data.Length != 0)
            {
                Buffer.BlockCopy(array, start + 2, 
                    Data, 0, Data.Length);
            }
        }

        public byte[] ToArray()
        {
            var buffer = new byte[Data.Length + 5];

            buffer[0] = 0xAA;
            buffer[1] = (byte)((Data.Length + 2) >> 8);
            buffer[2] = (byte)((Data.Length + 2) >> 0);
            buffer[3] = Command;
            buffer[4] = Ordinal;

            var i = 0;
            while (i < Data.Length)
            {
                buffer[i + 5] = Data[i];
                i++;
            }

            return buffer;
        }

        public override string ToString()
        {
            return $"" +
                   $"{Command:X2} " +
                   $"{Ordinal:X2} " +
                   $"{BitConverter.ToString(Data).Replace('-', ' ')}";
        }
    }
}