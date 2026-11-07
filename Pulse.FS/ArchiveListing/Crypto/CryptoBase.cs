using System;

namespace Pulse.FS
{
    /// <summary>
    /// Block counter dependent values of the filelist crypto.
    /// Port of <c>WhiteBinTools.Crypto.CryptoBase</c>.
    /// </summary>
    internal static class CryptoBase
    {
        public static void BlockCounterSetup(uint blockCounter, out uint xorTableOffset, out uint blockCounterEval, out uint blockCounterFval)
        {
            ushort blockCounterLowerMost = (ushort)(blockCounter & 0xFFFF);
            blockCounterLowerMost >>= 3;
            blockCounterLowerMost <<= 3;
            xorTableOffset = (uint)blockCounterLowerMost & 0xF8;

            long blockCounterABshiftVal = (long)blockCounter << 10;
            long blockCounterCDshiftVal = (long)blockCounter << 20;
            long blockCounterEFshiftVal = (long)blockCounter << 30;

            uint blockCounterAval = (uint)(blockCounterABshiftVal & 0xFFFFFFFF);
            uint blockCounterBval = (uint)(blockCounterABshiftVal >> 32);

            uint blockCounterCval = (uint)(blockCounterCDshiftVal & 0xFFFFFFFF);
            uint blockCounterDval = (uint)(blockCounterCDshiftVal >> 32);

            blockCounterCval |= blockCounterAval;
            blockCounterDval |= blockCounterBval;

            blockCounterEval = (uint)(blockCounterEFshiftVal & 0xFFFFFFFF);
            blockCounterFval = (uint)(blockCounterEFshiftVal >> 32);

            blockCounterEval |= blockCounter;
            blockCounterFval |= 0;

            blockCounterEval |= blockCounterCval;
            blockCounterFval |= blockCounterDval;
        }

        public static void XorBlockSetup(byte[] xorTable, uint tableOffset, out uint xorBlockLowerVal, out uint xorBlockHigherVal)
        {
            xorBlockLowerVal = BitConverter.ToUInt32(xorTable, (int)tableOffset);
            xorBlockHigherVal = BitConverter.ToUInt32(xorTable, (int)tableOffset + 4);
        }

        public static void SpecialKeySetup(uint blockCounterEval, uint blockCounterFval, out ulong specialKey1, out ulong specialKey2)
        {
            uint carryFlag = blockCounterEval > ~0xA1652347u ? 1u : 0u;

            specialKey1 = (ulong)blockCounterEval + 0xA1652347u;
            specialKey2 = (ulong)blockCounterFval + carryFlag;
        }
    }
}
