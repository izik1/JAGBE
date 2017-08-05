using Microsoft.VisualStudio.TestTools.UnitTesting;
using JAGBE.GB.Computation;
using System.Diagnostics;
using System.IO;
using JAGBE.GB.DataTypes;
using System;

namespace JAGBETests

{
    [TestClass]
    public class CpuTests
    {
        [TestMethod]
        public void EnsureRamValues()
        {
            Cpu cpu = new Cpu(File.ReadAllBytes("DMG_ROM.bin"), File.ReadAllBytes("Tetris (World).gb"));
            cpu.DisableLcdRenderer();

            Stopwatch sw = Stopwatch.StartNew();
            while (cpu.Pc != 0x100 && sw.ElapsedTicks < Stopwatch.Frequency * 2)
            {
                cpu.Tick(Cpu.DelayStep);
            }

            Assert.IsTrue(cpu.Pc == 0x100, "Program counter is wrong (0x" + cpu.Pc.ToString("X4") + ")");
            byte[] cpuRam = cpu.RamDump;
            byte[] controlRam = File.ReadAllBytes("RamDump.dump");
            if (controlRam != null)
            {
                for (int i = 0; i < controlRam.Length; i++)
                {
                    Assert.IsTrue(cpuRam[i] == controlRam[i], "Ram doesn't match {0x" + i.ToString("X4") + "} Expected: 0x" +
                        controlRam[i].ToString() + " Got 0x" + cpuRam[i].ToString());
                }
            }
            else
            {
                Assert.Fail("Control ram not found");
            }
        }

        [TestMethod]
        public void CheckRlcInstructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x2);
            memory.Rom[0] = 0xCB;
            memory.Rom[1] = 0x00;

            BitwiseRegTest(cpu, memory, 8, 0, 0b0100_0000, 0b1000_0000, 0);
            memory.R.Pc -= 2;

            BitwiseRegTest(cpu, memory, 8, 0, 0, 0, RFlags.ZB);
            memory.R.Pc -= 2;

            BitwiseRegTest(cpu, memory, 8, 0, 0b1100_0000, 0b1000_0000, RFlags.CB);
            memory.R.Pc -= 2;

            BitwiseRegTest(cpu, memory, 8, 0, 0b1000_0000, 0, RFlags.ZB | RFlags.CB);
            memory.R.Pc -= 2;

            memory.R.Hl = 0xD000;
            memory.Rom[1] = 0x06;

            BitwiseHlTest(cpu, memory, 16, 1 << 6, 1 << 7, 0);
            memory.R.Pc -= 2;

            BitwiseHlTest(cpu, memory, 16, 0, 0, RFlags.ZB);
            memory.R.Pc -= 2;

            BitwiseHlTest(cpu, memory, 16, 0b11 << 6, 1 << 7, RFlags.CB);
            memory.R.Pc -= 2;

            BitwiseHlTest(cpu, memory, 16, 1 << 7, 0, RFlags.ZB | RFlags.CB);
        }

        private static void BitwiseRegTest(Cpu c, GbMemory mem, int ticks, int reg, byte initReg, byte expectedReg, byte expectedFlags)
        {
            mem.R.F = 0;
            mem.R.SetR8(reg, initReg);
            c.Tick(ticks);
            Assert.AreEqual(expectedReg, mem.R.GetR8(reg), "Data");
            Assert.AreEqual(expectedFlags, mem.R.F, "Flags");
        }

        private static void BitwiseHlTest(Cpu c, GbMemory mem, int ticks, byte initVal, byte expectedVal, byte expectedFlags)
        {
            mem.R.F = 0;
            mem.SetMappedMemory(mem.R.Hl, initVal);
            Console.WriteLine(mem.GetMappedMemoryHl());
            c.Tick(ticks);

            Assert.AreEqual(expectedVal, mem.GetMappedMemoryHl(), " HL");
            Assert.AreEqual(expectedFlags, mem.R.F, " Flags");
        }

        /// <summary>
        /// Checks the CB opcodes.
        /// </summary>
        /// <remarks>In the future might do more, for now just checks if all CB opcodes are implemented.</remarks>
        [TestMethod]
        public void CheckCbOpCodes()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x200);
            cpu.DisableLcdRenderer();
            memory.Rom = new byte[0x200];
            for (int i = 0; i < 0x100; i++)
            {
                memory.Rom[(ushort)((i * 2) + 0)] = 0xCD;
                memory.Rom[(ushort)((i * 2) + 1)] = (byte)i;
            }

            memory.SetMappedMemory(0xFF50, 1); // Force bootmode to be disabled.

            while (cpu.Pc < 0x200)
            {
                cpu.Tick(4);
            }
        }

        private static (Cpu, GbMemory) ConfigureCpu(int romSize)
        {
            GbMemory mem = new GbMemory();
            Cpu c = new Cpu(mem);
            mem.Rom = new byte[romSize];
            mem.SetMappedMemory(0xFF50, 1); // Force bootmode to be disabled.
            c.DisableLcdRenderer();
            return (c, mem);
        }
    }
}
