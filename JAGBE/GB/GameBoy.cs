using System.IO;
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
        internal Cpu cpu;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameBoy"/> class.
        /// </summary>
        /// <param name="romPath">The rom path.</param>
        /// <param name="BootromPath">The bootrom path.</param>
        /// <param name="inputHandler">The input handler.</param>
        internal GameBoy(string romPath, string BootromPath, Input.IInputHandler inputHandler) =>
            this.cpu = new Cpu(File.ReadAllBytes(BootromPath), File.ReadAllBytes(romPath), inputHandler);

        /// <summary>
        /// Updates the cpu using the given <paramref name="targetUpdateRate"/> to determine the
        /// number of ticks to run.
        /// </summary>
        /// <param name="targetUpdateRate">The target update rate.</param>
        internal void Update(int targetUpdateRate) => this.cpu.Tick(this.cpu.WriteToConsole ? 160 : Cpu.ClockSpeedHz / targetUpdateRate);
    }
}
