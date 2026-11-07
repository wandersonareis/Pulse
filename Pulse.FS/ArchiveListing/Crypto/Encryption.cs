using System;

namespace Pulse.FS
{
    /// <summary>
    /// Encrypts the 8 bytes blocks of a filelist body.
    /// Port of <c>WhiteBinTools.Crypto.Encryption.EncryptBlocks</c>.
    /// </summary>
    internal static class Encryption
    {
        public static void EncryptBlocks(byte[] xorTable, uint blockCount, byte[] buffer, int bufferOffset)
        {
            uint blockCounter = 0;

            for (uint i = 0; i < blockCount; i++)
            {
                int readPos = bufferOffset + (int)(i * 8);
                if (readPos + 8 > buffer.Length)
                    break;

                uint currentBlockId = blockCounter >> 3;

                CryptoBase.BlockCounterSetup(blockCounter, out uint tableOffset, out uint blockCounterEval, out uint blockCounterFval);
                CryptoBase.XorBlockSetup(xorTable, tableOffset, out uint xorBlockLowerVal, out uint xorBlockHigherVal);
                CryptoBase.SpecialKeySetup(blockCounterEval, blockCounterFval, out ulong specialKey1, out ulong specialKey2);

                byte byte0 = buffer[readPos + 0];
                byte byte1 = buffer[readPos + 1];
                byte byte2 = buffer[readPos + 2];
                byte byte3 = buffer[readPos + 3];
                byte byte4 = buffer[readPos + 4];
                byte byte5 = buffer[readPos + 5];
                byte byte6 = buffer[readPos + 6];
                byte byte7 = buffer[readPos + 7];

                ulong bytesToEncryptLowerVal = 0xFFFFFFFF00000000UL | ((ulong)byte7 << 24) | ((ulong)byte6 << 16) | ((ulong)byte5 << 8) | byte4;
                ulong bytesToEncryptHigherVal = 0xFFFFFFFF00000000UL | ((ulong)byte3 << 24) | ((ulong)byte2 << 16) | ((ulong)byte1 << 8) | byte0;

                bytesToEncryptLowerVal ^= xorBlockLowerVal;
                bytesToEncryptHigherVal ^= xorBlockHigherVal;

                bytesToEncryptLowerVal ^= specialKey1;
                bytesToEncryptHigherVal ^= specialKey2;

                bytesToEncryptLowerVal += xorBlockLowerVal;
                bytesToEncryptHigherVal += xorBlockHigherVal;

                uint carryFlag = (bytesToEncryptLowerVal & 0xFFFFFFFFUL) < xorBlockLowerVal ? 1u : 0u;
                bytesToEncryptHigherVal += carryFlag;

                byte[] computedBytes = new byte[8];
                Buffer.BlockCopy(BitConverter.GetBytes((uint)bytesToEncryptLowerVal), 0, computedBytes, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes((uint)bytesToEncryptHigherVal), 0, computedBytes, 4, 4);

                uint previousByte = (currentBlockId ^ 69) & 0xFF;
                for (int k = 0; k < 8; k++)
                {
                    previousByte ^= CryptoFunctions.LoopAByteReverse(computedBytes[k], xorTable, tableOffset);
                    buffer[readPos + k] = (byte)previousByte;
                }

                blockCounter += 8;
            }
        }
    }
}
