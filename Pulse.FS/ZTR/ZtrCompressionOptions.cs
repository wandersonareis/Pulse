namespace Pulse.FS
{
    /// <summary>
    /// The mode of the ZTR compression.
    /// </summary>
    public enum ZtrCompressionMode
    {
        /// <summary>
        /// The real compression of the game: the repeated pairs of bytes are
        /// replaced with the pointers to the dictionary pages.
        /// </summary>
        Compressed,

        /// <summary>
        /// The chunks contain no dictionary at all and keep the raw data.
        /// </summary>
        Uncompressed
    }

    /// <summary>
    /// The options of the ZTR compression.
    /// </summary>
    public sealed class ZtrCompressionOptions
    {
        /// <summary>
        /// The options used by default: the real compression with the tie rule of
        /// the original game generator.
        /// </summary>
        public static ZtrCompressionOptions Default => new();

        public ZtrCompressionMode Mode { get; set; } = ZtrCompressionMode.Compressed;

        public ZtrCompressionTieRule TieRule { get; set; } = ZtrCompressionTieRule.TieMinLast;

        public ZtrCompressionOptions Clone()
        {
            return new ZtrCompressionOptions
            {
                Mode = Mode,
                TieRule = TieRule
            };
        }
    }
}
