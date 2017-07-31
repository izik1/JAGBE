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
            this.cpu = new Cpu(File.ReadAllBytes("DMG_ROM.bin"), File.ReadAllBytes("Tetris (World).gb"));
            this.cpu.DisableLcdRenderer();
        }

        [TestMethod]
        public void EnsureRamValues()
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (this.cpu.Pc != 0x100 && sw.ElapsedTicks < Stopwatch.Frequency * 2)
            {
                this.cpu.Tick(Cpu.DelayStep);
            }

            Assert.IsTrue(this.cpu.Pc == 0x100, "Program counter is wrong (0x" + this.cpu.Pc.ToString("X4") + ")");
            byte[] cpuRam = this.cpu.RamDump;
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

        /// <summary>
        /// Checks the CB opcodes.
        /// </summary>
        /// <remarks>In the future might do more, for now just checks if all CB opcodes are implemented.</remarks>
        [TestMethod]
        public void CheckCbOpCodes()
        {
            byte[] cRom = new byte[0x200];
            for (int i = 0; i < 0x200; i += 2)
            {
                cRom[i] = 0xCB;
                cRom[i + 1] = (byte)(i / 2);
            }

            this.cpu = new Cpu(new byte[256], cRom);
            while (this.cpu.Pc < 0x200)
            {
                this.cpu.Tick(4);
            }
        }
    }
}
