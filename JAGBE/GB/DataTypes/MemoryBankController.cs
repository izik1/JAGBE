namespace JAGBE.GB.DataTypes
{
    internal enum MemoryBankController
    {
        /// <summary>
        /// All the ROM can be mapped to 0000h-7FFFh directly. A RAM chip up to 8KB may be connected
        /// to A000h-BFFFh, but a tiny circuit would be required to enable and disable it.
        /// </summary>
        None,
    }
}
