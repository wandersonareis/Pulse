using System;

namespace Pulse.FS
{
    /// <summary>
    /// Primitive operations of the filelist crypto.
    /// Port of <c>WhiteBinTools.Crypto.CryptoFunctions</c>.
    /// </summary>
    internal static class CryptoFunctions
    {
        public static uint LoopAByte(uint decryptedByte, byte[] xorTable, uint tableOffset)
        {
            for (uint byteIterator = 0; byteIterator < 8; byteIterator++)
            {
                int integerVal = IntegersArray.Integers[(int)(decryptedByte & 0xFF)];
                int xorTableByte = xorTable[tableOffset + byteIterator];

                decryptedByte = (uint)((integerVal - xorTableByte) & 0xFF);
            }

            return decryptedByte;
        }

        public static byte LoopAByteReverse(byte byteToEncrypt, byte[] xorTable, uint tableOffset)
        {
            for (int byteIterator = 7; byteIterator > -1; byteIterator--)
            {
                int xorTableByte = xorTable[tableOffset + (uint)byteIterator];
                int integerValUsed = (xorTableByte + byteToEncrypt) & 0xFF;

                byteToEncrypt = IntegersArray.IntegersInverse[integerValUsed];
            }

            return byteToEncrypt;
        }

        /// <summary>
        /// Sums every fourth byte of the specified region.
        /// </summary>
        public static uint ComputeCheckSum(byte[] data, uint blocks, uint readPos)
        {
            uint chkSumVal = 0;

            for (uint i = 0; i < blocks; i++)
            {
                uint position = readPos + i * 4;
                if (position >= (uint)data.Length)
                    break;

                chkSumVal += data[position];
            }

            return chkSumVal;
        }
    }
}
