using System;

namespace Pulse.FS
{
    /// <summary>
    /// Decrypts the 8 bytes blocks of a filelist body.
    /// Port of <c>WhiteBinTools.Crypto.Decryption.DecryptBlocks</c>.
    /// </summary>
    internal static class Decryption
    {
        public static void DecryptBlocks(byte[] xorTable, uint blockCount, byte[] buffer, int bufferOffset)
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

                byte currentByte0 = buffer[readPos + 0];
                byte currentByte1 = buffer[readPos + 1];
                byte currentByte2 = buffer[readPos + 2];
                byte currentByte3 = buffer[readPos + 3];
                byte currentByte4 = buffer[readPos + 4];
                byte currentByte5 = buffer[readPos + 5];
                byte currentByte6 = buffer[readPos + 6];
                byte currentByte7 = buffer[readPos + 7];

                uint decryptedByte1 = CryptoFunctions.LoopAByte(((currentBlockId ^ 69) & 0xFF) ^ currentByte0, xorTable, tableOffset);
                uint decryptedByte2 = CryptoFunctions.LoopAByte((uint)(currentByte0 ^ currentByte1), xorTable, tableOffset);
                uint decryptedByte3 = CryptoFunctions.LoopAByte((uint)(currentByte1 ^ currentByte2), xorTable, tableOffset);
                uint decryptedByte4 = CryptoFunctions.LoopAByte((uint)(currentByte2 ^ currentByte3), xorTable, tableOffset);
                uint decryptedByte5 = CryptoFunctions.LoopAByte((uint)(currentByte3 ^ currentByte4), xorTable, tableOffset);
                uint decryptedByte6 = CryptoFunctions.LoopAByte((uint)(currentByte4 ^ currentByte5), xorTable, tableOffset);
                uint decryptedByte7 = CryptoFunctions.LoopAByte((uint)(currentByte5 ^ currentByte6), xorTable, tableOffset);
                uint decryptedByte8 = CryptoFunctions.LoopAByte((uint)(currentByte6 ^ currentByte7), xorTable, tableOffset);

                uint decryptedBytesLowerVal = decryptedByte1 | (decryptedByte2 << 8) | (decryptedByte3 << 16) | (decryptedByte4 << 24);
                uint decryptedBytesHigherVal = decryptedByte5 | (decryptedByte6 << 8) | (decryptedByte7 << 16) | (decryptedByte8 << 24);

                uint borrowFlag = decryptedBytesLowerVal < xorBlockLowerVal ? 1u : 0u;

                decryptedBytesLowerVal -= xorBlockLowerVal;
                decryptedBytesHigherVal -= xorBlockHigherVal;
                decryptedBytesHigherVal -= borrowFlag;

                decryptedBytesLowerVal ^= (uint)specialKey1;
                decryptedBytesHigherVal ^= (uint)specialKey2;

                decryptedBytesLowerVal ^= xorBlockLowerVal;
                decryptedBytesHigherVal ^= xorBlockHigherVal;

                Buffer.BlockCopy(BitConverter.GetBytes(decryptedBytesHigherVal), 0, buffer, readPos, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(decryptedBytesLowerVal), 0, buffer, readPos + 4, 4);

                blockCounter += 8;
            }
        }
    }
}
