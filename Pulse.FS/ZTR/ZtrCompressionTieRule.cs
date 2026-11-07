namespace Pulse.FS
{
    /// <summary>
    /// The rule used to resolve the ties between the pairs of bytes with the same
    /// repetition count during the ZTR compression.
    /// </summary>
    public enum ZtrCompressionTieRule
    {
        /// <summary>
        /// The first pair in the data order wins. It is the rule of the ZTRtool
        /// (<c>-c2</c> switch) and it is used by the files like <c>auto_yus_us</c>.
        /// </summary>
        TieFirst,

        /// <summary>
        /// The pair with the smallest offset of the last occurrence wins. It is the
        /// rule of the original game generator, so the rebuilt files are byte
        /// identical to the retail ones.
        /// </summary>
        TieMinLast
    }
}
