using System.IO;
using JAGBE.GB.Computation;

namespace JAGBE.GB
{
    internal sealed class GameBoy
    {
        internal Cpu cpu;

        internal GameBoy(string romPath, string BootromPath, Input.IInputHandler inputHandler) =>
            this.cpu = new Cpu(File.ReadAllBytes(BootromPath), File.ReadAllBytes(romPath), inputHandler);

        internal void Update(int targetUpdateRate) => this.cpu.Tick(this.cpu.WriteToConsole ? 160 : Cpu.ClockSpeedHz / targetUpdateRate);
    }
}
