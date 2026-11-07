using System;

namespace Pulse.FS
{
    /// <summary>
    /// Builds the 264 bytes XOR table used by the filelist crypto from the 8 bytes seed.
    /// Port of <c>WhiteBinTools.Crypto.Generator.GenerateXORtable</c>.
    /// </summary>
    internal static class Generator
    {
        public const int XorTableSize = 264;

        public static byte[] GenerateXorTable(byte[] seedArray)
        {
            if (seedArray == null)
                throw new ArgumentNullException(nameof(seedArray));

            if (seedArray.Length < 8)
                throw new ArgumentException("The seed must be at least 8 bytes long.", nameof(seedArray));

            byte[] xorTable = new byte[XorTableSize];

            byte[] reversed = new byte[8];
            for (int i = 0; i < 8; i++)
                reversed[i] = seedArray[7 - i];

            uint seedHalfA = BitConverter.ToUInt32(reversed, 0);
            uint seedHalfB = BitConverter.ToUInt32(reversed, 4);

            seedHalfA = (seedHalfA << 0x08) | (seedHalfA >> 0x18);
            seedHalfB = (seedHalfB >> 0x10) | (seedHalfB << 0x10);

            byte[] xorBlock = new byte[8];
            Buffer.BlockCopy(BitConverter.GetBytes(seedHalfB), 0, xorBlock, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(seedHalfA), 0, xorBlock, 4, 4);
            xorBlock[0] = (byte)(xorBlock[0] + 0x45);

            for (int i = 1; i < 8; i++)
            {
                int tmp = xorBlock[i] + 0xD4 + xorBlock[i - 1];
                tmp ^= xorBlock[i - 1] << 2;
                tmp ^= 0x45;

                xorBlock[i] = (byte)tmp;
            }

            Buffer.BlockCopy(xorBlock, 0, xorTable, 0, 8);

            ulong previousXorBlock = BitConverter.ToUInt64(xorBlock, 0);
            int copyIndex = 8;

            for (int i = 1; i < 0x21; i++)
            {
                uint blockHalfA = (uint)(previousXorBlock & 0xFFFFFFFF);
                uint blockHalfB = (uint)(previousXorBlock >> 32);

                ulong a = 5 * previousXorBlock;
                a ^= (ulong)blockHalfB << 32;

                uint tmpBlockHalfA = (uint)(blockHalfA ^ a);
                uint tmpBlockHalfB = (uint)(a >> 32);

                a = blockHalfA | (a & 0xFFFFFFFF00000000);

                ulong b = tmpBlockHalfA;

                uint xorBlockHalfA = (uint)(a ^ b);
                tmpBlockHalfB ^= blockHalfB;
                uint xorBlockHalfB = tmpBlockHalfB;

                byte[] nextBlock = new byte[8];
                Buffer.BlockCopy(BitConverter.GetBytes(xorBlockHalfA), 0, nextBlock, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(xorBlockHalfB), 0, nextBlock, 4, 4);

                Buffer.BlockCopy(nextBlock, 0, xorTable, copyIndex, 8);

                previousXorBlock = BitConverter.ToUInt64(nextBlock, 0);
                copyIndex += 8;
            }

            return xorTable;
        }
    }
}
