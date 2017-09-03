namespace JAGBE.GB.Emulation
{
    /// <summary>
    /// The states a given <see cref="Cpu"/> can be in.
    /// </summary>
    internal enum CpuState
    {
        /// <summary>
        /// The <see cref="Cpu"/> is running normally.
        /// </summary>
        OKAY = 0,

        /// <summary>
        /// The <see cref="Cpu"/> is being halted.
        /// </summary>
        HALT = 1,

        /// <summary>
        /// The <see cref="Cpu"/> is being stopped.
        /// </summary>
        STOP = 2,

        /// <summary>
        /// The <see cref="Cpu"/> has hung.
        /// </summary>
        HUNG = 3,
    }
}
