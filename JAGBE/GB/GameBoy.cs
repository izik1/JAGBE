using JAGBE.GB.Emulation;

namespace JAGBE.GB
{
    /// <summary>
    /// A UI level Wrapper for accessing running a <see cref="Cpu"/>
    /// </summary>
    internal sealed class GameBoy
    {
        /// <summary>
        /// The cpu
        /// </summary>
        internal readonly Cpu cpu;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameBoy"/> class.
        /// </summary>
        /// <param name="rom">The rom.</param>
        /// <param name="bootRom">The boot rom.</param>
        /// <param name="inputHandler">The input handler.</param>
        internal GameBoy(byte[] rom, byte[] bootRom, Input.IInputHandler inputHandler) => cpu = new Cpu(bootRom, rom, inputHandler);

        /// <summary>
        /// Updates the cpu using the given <paramref name="targetUpdateRate"/> to determine the
        /// number of ticks to run.
        /// </summary>
        /// <param name="targetUpdateRate">The target update rate.</param>
        internal void Update(int targetUpdateRate) => cpu.Tick(cpu.BreakMode ? 160 : Cpu.ClockSpeedHz / targetUpdateRate);
    }
}
