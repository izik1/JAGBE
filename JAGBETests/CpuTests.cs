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
        public void CheckRlcInstructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x2);
            InitBitwiseInstructions(cpu, memory, 0, 0b100_0000);
            BitwiseRegTest(cpu, memory, 8, 0, 0b1000_0000, 0);
            BitwiseRegTest(cpu, memory, 8, 0, 0, RFlags.CB | RFlags.ZB);
            memory.Rom[1] = 0x06;
            BitwiseHlTest(cpu, memory, 16, 0, 0b1000_0000, 0);
            BitwiseHlTest(cpu, memory, 16, 0, 0, RFlags.CB | RFlags.ZB);
        }

        private static void InitBitwiseInstructions(Cpu c, GbMemory m, byte inst, byte initVal)
        {
            m.Rom[0] = 0xCB;
            m.Rom[1] = inst;
            m.R.B = initVal;
            m.R.Hl = 0xC000;
            m.SetMappedMemory(m.R.Hl, initVal);
        }

        [TestMethod]
        public void CheckRlInstructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x2);
            InitBitwiseInstructions(cpu, memory, 0x10, 0b100_0000);
            BitwiseRegTest(cpu, memory, 8, 0, 0b1000_0000, 0);
            BitwiseRegTest(cpu, memory, 8, 0, 0, RFlags.CB | RFlags.ZB);
            BitwiseRegTest(cpu, memory, 8, 0, 1, 0);
            memory.Rom[1] = 0x16;
            memory.R.F = 0;
            BitwiseHlTest(cpu, memory, 16, 0, 0b1000_0000, 0);
            BitwiseHlTest(cpu, memory, 16, 0, 0, RFlags.CB | RFlags.ZB);
            BitwiseHlTest(cpu, memory, 16, 0, 1, 0);
        }

        private static void BitwiseRegTest(Cpu c, GbMemory mem, int ticks, int reg, byte expectedReg, byte expectedFlags)
        {
            c.Tick(ticks);
            Assert.AreEqual(expectedReg, mem.R.GetR8(reg), "Data");
            Assert.AreEqual(expectedFlags, mem.R.F, "Flags");
            mem.R.Pc -= 2;
        }

        private static void BitwiseHlTest(Cpu c, GbMemory mem, int ticks, byte initVal, byte expectedVal, byte expectedFlags)
        {
            c.Tick(ticks);

            Assert.AreEqual(expectedVal, mem.GetMappedMemoryHl(), " HL");
            Assert.AreEqual(expectedFlags, mem.R.F, " Flags");
            mem.R.Pc -= 2;
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
