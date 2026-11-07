namespace Pulse.FS
{
    /// <summary>
    /// Permutation table used by the filelist crypto stream.
    /// <para>
    /// The forward table maps a byte to <c>(value + 120) & 0xFF</c>, the inverse table
    /// performs the opposite mapping. Both tables are the exact equivalent of the
    /// <c>IntegersArray.Integers</c> table of the original WhiteBinTools implementation.
    /// </para>
    /// </summary>
    internal static class IntegersArray
    {
        public const int Size = 256;

        public static readonly byte[] Integers = CreateForward();
        public static readonly byte[] IntegersInverse = CreateInverse();

        private static byte[] CreateForward()
        {
            byte[] result = new byte[Size];
            for (int i = 0; i < Size; i++)
                result[i] = (byte)((i + 120) & 0xFF);

            return result;
        }

        private static byte[] CreateInverse()
        {
            byte[] result = new byte[Size];
            for (int i = 0; i < Size; i++)
                result[(i + 120) & 0xFF] = (byte)i;

            return result;
        }
    }
}
