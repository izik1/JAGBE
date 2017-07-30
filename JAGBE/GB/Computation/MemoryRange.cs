namespace JAGBE.GB.Computation
{
    /// <summary>
    /// Defines constants for ranges of memory.
    /// </summary>
    internal static class MemoryRange
    {
        /// <summary>
        /// The Size of ERAM banks.
        /// </summary>
        /// <value>0x2000 (8192)</value>
        internal const int ERAMBANKSIZE = 0x2000;

        /// <summary>
        /// The size of HRAM.
        /// </summary>
        /// <value>0x7F (127)</value>
        internal const int HRAMSIZE = 0x7F;

        /// <summary>
        /// The size of Object Attribute Memory.
        /// </summary>
        /// <value>0xA0 (160)</value>
        internal const int OAMSIZE = 0xA0;

        /// <summary>
        /// The size of ROM banks.
        /// </summary>
        /// <value>0x4000 (16,384)</value>
        internal const int ROMBANKSIZE = 0x4000;

        /// <summary>
        /// The size of VRAM banks.
        /// </summary>
        /// <value>0x2000 (8192)</value>
        internal const int VRAMBANKSIZE = 0x2000;

        /// <summary>
        /// The size of WRAM banks.
        /// </summary>
        /// <value>0x1000 (4096)</value>
        internal const int WRAMBANKSIZE = 0x1000;
    }
}
