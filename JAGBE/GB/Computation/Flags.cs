namespace JAGBE.GB.Computation
{
#pragma warning restore S1067 // Expressions should not be too complex

    /// <summary>
    /// Defines constants for the flags in the flags register.
    /// </summary>
    internal static class RFlags
    {
        /// <summary>
        /// Carry flag.
        /// </summary>
        internal const byte C = 4;

        /// <summary>
        /// A Bit-field of the Carry flag;
        /// </summary>
        internal const byte CB = 1 << C;

        /// <summary>
        /// Half carry flag (BCD)
        /// </summary>
        internal const byte H = 5;

        /// <summary>
        /// A Bit-field of the Half Carry flag;
        /// </summary>
        internal const byte HB = 1 << H;

        /// <summary>
        /// Add/Sub flag (BCD)
        /// </summary>
        internal const byte N = 6;

        /// <summary>
        /// A Bit-field of the Add/Sub flag;
        /// </summary>
        internal const byte NB = 1 << N;

        /// <summary>
        /// Zero flag.
        /// </summary>
        internal const byte Z = 7;

        /// <summary>
        /// A Bit-field of the Zero flag;
        /// </summary>
        internal const byte ZB = 1 << Z;
    }
}
