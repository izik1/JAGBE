using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JAGBE.GB.Computation;
using System.Diagnostics;
using System.IO;

namespace JAGBETests

{
    [TestClass]
    public class CpuTests
    {
        private Cpu cpu;

        [TestInitialize]
        public void Init()
        {
            this.cpu = new Cpu(File.ReadAllBytes("DMG_ROM.bin"))
            {
                Rom = File.ReadAllBytes("Tetris (World).gb"),
            };
            this.cpu.DisableLcdRenderer();
        }

        [TestMethod]
        public void EnsureRamValues()
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (this.cpu.R.Pc != 0x100 && sw.ElapsedTicks < Stopwatch.Frequency * 2)
            {
                this.cpu.Tick(Cpu.DelayStep);
            }

            Assert.IsTrue(this.cpu.R.Pc == 0x100, "Program counter is wrong (0x" + this.cpu.R.Pc.ToString("X4") + ")");
            byte[] cpuRam = this.cpu.DumpRam();
            byte[] controlRam = File.ReadAllBytes("RamDump.dump");
            if (controlRam != null)
            {
                for (int i = 0; i < controlRam.Length; i++)
                {
                    Assert.IsFalse(cpuRam[i] != controlRam[i], "Ram doesn't match {0x" + i.ToString("X4") + "} Expected: 0x" +
                        controlRam[i].ToString() + " Got 0x" + cpuRam[i].ToString());
                }
            }
            else
            {
                Assert.Fail("Control ram not found");
            }
        }
    }
}
