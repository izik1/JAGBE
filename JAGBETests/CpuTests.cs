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
        [TestCategory("Bitwise")]
        public void CheckRlcInstructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x2);
            InitBitwiseInstructions(memory, 0, 0b100_0000);
            BitwiseRegTest(cpu, memory, 8, 0, 0b1000_0000, 0);
            BitwiseRegTest(cpu, memory, 8, 0, 0, RFlags.CB | RFlags.ZB);
            memory.Rom[1] = 0x06;
            BitwiseHlTest(cpu, memory, 16, 0b1000_0000, 0);
            BitwiseHlTest(cpu, memory, 16, 0, RFlags.CB | RFlags.ZB);
        }

        [TestCategory("Bitwise")]
        private static void InitBitwiseInstructions(GbMemory m, byte inst, byte initVal)
        {
            m.Rom[0] = 0xCB;
            m.Rom[1] = inst;
            m.R.B = initVal;
            m.R.Hl = 0xC000;
            m.SetMappedMemory(m.R.Hl, initVal);
        }

        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckRlInstructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x2);
            InitBitwiseInstructions(memory, 0x10, 0b100_0000);
            BitwiseRegTest(cpu, memory, 8, 0, 0b1000_0000, 0);
            BitwiseRegTest(cpu, memory, 8, 0, 0, RFlags.CB | RFlags.ZB);
            BitwiseRegTest(cpu, memory, 8, 0, 1, 0);
            memory.Rom[1] = 0x16;
            memory.R.F = 0;
            BitwiseHlTest(cpu, memory, 16, 0b1000_0000, 0);
            BitwiseHlTest(cpu, memory, 16, 0, RFlags.CB | RFlags.ZB);
            BitwiseHlTest(cpu, memory, 16, 1, 0);
        }

        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckBitInstructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x2);
            InitBitwiseInstructions(memory, 0x40, 0b1111_1111);

            for (int i = 0; i < 8; i++)
            {
                BitwiseRegTest(cpu, memory, 8, 0, 0b1111_1111, RFlags.HB);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x40;
            memory.R.B = 0;
            for (int i = 0; i < 8; i++)
            {
                BitwiseRegTest(cpu, memory, 8, 0, 0, RFlags.ZB | RFlags.HB);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x46;

            for (int i = 0; i < 8; i++)
            {
                BitwiseHlTest(cpu, memory, 12, 0b1111_1111, RFlags.HB);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x46;
            memory.SetMappedMemory(memory.R.Hl, 0);
            for (int i = 0; i < 8; i++)
            {
                BitwiseHlTest(cpu, memory, 12, 0, RFlags.ZB | RFlags.HB);
                memory.Rom[1] += 8;
            }
        }

        private static void BitwiseRegTest(Cpu c, GbMemory mem, int ticks, int reg, byte expectedReg, byte expectedFlags)
        {
            c.Tick(ticks);
            Assert.AreEqual(expectedReg, mem.R.GetR8(reg), "Data");
            Assert.AreEqual(expectedFlags, mem.R.F, "Flags");
            mem.R.Pc -= 2;
        }

        private static void BitwiseHlTest(Cpu c, GbMemory mem, int ticks, byte expectedVal, byte expectedFlags)
        {
            c.Tick(ticks);

            Assert.AreEqual(expectedVal, mem.GetMappedMemoryHl(), " HL");
            Assert.AreEqual(expectedFlags, mem.R.F, " Flags");
            mem.R.Pc -= 2;
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
