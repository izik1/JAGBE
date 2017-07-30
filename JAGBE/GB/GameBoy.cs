using System.IO;
using JAGBE.GB.Computation;

namespace JAGBE.GB
{
    internal sealed class GameBoy
    {
        internal Cpu cpu;

        internal GameBoy(string romPath, string BootromPath) =>
            this.cpu = new Cpu(File.ReadAllBytes(BootromPath), File.ReadAllBytes(romPath));

        private GameBoy()
        {
        }

        internal void Update(int targetUpdateRate) => this.cpu.Tick(this.cpu.WriteToConsole ? 40 : Cpu.ClockSpeedHz / targetUpdateRate);
    }
}
