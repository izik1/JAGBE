namespace JAGBE.GB.Computation
{
    /// <summary>
    /// Defines constants for the flags in the flags register.
    /// </summary>
    internal static class RFlags
    {
        /// <summary>
        /// A Bit-field of the Carry flag;
        /// </summary>
        internal const byte CB = 0b0001_0000;

        /// <summary>
        /// Carry flag.
        /// </summary>
        internal const byte CF = 4;

        /// <summary>
        /// A Bit-field of the Half Carry flag;
        /// </summary>
        internal const byte HB = 0b0010_0000;

        internal const byte HCB = 0b0011_0000;

        /// <summary>
        /// Half carry flag (BCD)
        /// </summary>
        internal const byte HF = 5;

        /// <summary>
        /// A Bit-field of the Add/Sub flag;
        /// </summary>
        internal const byte NB = 0b0100_0000;

        internal const byte NCB = 0b0101_0000;

        /// <summary>
        /// Add/Sub flag (BCD)
        /// </summary>
        internal const byte NF = 6;

        internal const byte NHB = 0b0110_0000;

        internal const byte NHCB = 0b0111_0000;

        /// <summary>
        /// A Bit-field of the Zero flag;
        /// </summary>
        internal const byte ZB = 0b1000_0000;

        internal const byte ZCB = 0b1001_0000;

        /// <summary>
        /// Zero flag.
        /// </summary>
        internal const byte ZF = 7;

        internal const byte ZHB = 0b1010_0000;
        internal const byte ZHCB = 0b1011_0000;

        internal const byte ZNB = 0b1100_0000;
        internal const byte ZNCB = 0b1101_0000;

        internal const byte ZNHB = 0b1110_0000;

        internal const byte ZNHCB = 0b1111_0000;
    }
}
