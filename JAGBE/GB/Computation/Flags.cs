namespace JAGBE.GB.Computation
{
    /// <summary>
    /// Defines constants for the flags in the flags register.
    /// </summary>
    internal static class RFlags
    {
        /// <summary>
        /// Carry flag.
        /// </summary>
        internal const byte CF = 4;

        /// <summary>
        /// A Bit-field of the Carry flag;
        /// </summary>
        internal const byte CB = 1 << CF;

        /// <summary>
        /// Half carry flag (BCD)
        /// </summary>
        internal const byte HF = 5;

        /// <summary>
        /// A Bit-field of the Half Carry flag;
        /// </summary>
        internal const byte HB = 1 << HF;

        /// <summary>
        /// Add/Sub flag (BCD)
        /// </summary>
        internal const byte NF = 6;

        /// <summary>
        /// A Bit-field of the Add/Sub flag;
        /// </summary>
        internal const byte NB = 1 << NF;

        /// <summary>
        /// Zero flag.
        /// </summary>
        internal const byte ZF = 7;

        /// <summary>
        /// A Bit-field of the Zero flag;
        /// </summary>
        internal const byte ZB = 1 << ZF;
    }
}
