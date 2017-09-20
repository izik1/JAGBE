using Microsoft.VisualStudio.TestTools.UnitTesting;
using JAGBE.GB.Emulation;
using System.Diagnostics;
using System.IO;
using System;

namespace JAGBETests
{
    [TestClass]
    public class CpuTests
    {
        [TestMethod]
        public void BenchmarkTcpµs()
        {
            Stopwatch stopwatch = new Stopwatch();
            byte[] bootRom = File.ReadAllBytes("boot rom.bin");
            byte[] rom = File.ReadAllBytes("blargg/cpu_instrs/cpu_instrs.gb");
            Cpu cpu = new Cpu(bootRom, rom, null);
            stopwatch.Start();
            const int ticks = 4000_0000 * Cpu.MCycle;
            cpu.Tick(ticks);
            stopwatch.Stop();
            Console.WriteLine("elapsedµs:cpuTicks");
            Console.WriteLine((stopwatch.ElapsedTicks * 1000000 / Stopwatch.Frequency).ToString() +
                ":" + ticks.ToString());
        }
    }
}
