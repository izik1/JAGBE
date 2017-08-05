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
            InitCbInstructionTest(memory, 0, 0b100_0000);
            InstructionRegTest(cpu, memory, 8, 0b1000_0000, 0, 2);
            InstructionRegTest(cpu, memory, 8, 0, RFlags.CB | RFlags.ZB, 2);
            memory.Rom[1] = 0x06;
            InstructionHlTest(cpu, memory, 16, 0b1000_0000, 0, 2);
            InstructionHlTest(cpu, memory, 16, 0, RFlags.CB | RFlags.ZB, 2);
        }

        private static void InitCbInstructionTest(GbMemory m, byte inst, byte initVal)
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
            InitCbInstructionTest(memory, 0x10, 0b100_0000);
            InstructionRegTest(cpu, memory, 8, 0b1000_0000, 0, 2);
            InstructionRegTest(cpu, memory, 8, 0, RFlags.CB | RFlags.ZB, 2);
            InstructionRegTest(cpu, memory, 8, 1, 0, 2);
            memory.Rom[1] = 0x16;
            memory.R.F = 0;
            InstructionHlTest(cpu, memory, 16, 0b1000_0000, 0, 2);
            InstructionHlTest(cpu, memory, 16, 0, RFlags.CB | RFlags.ZB, 2);
            InstructionHlTest(cpu, memory, 16, 1, 0, 2);
        }

        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckBitInstructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x2);
            InitCbInstructionTest(memory, 0x40, 0b1111_1111);

            for (int i = 0; i < 8; i++)
            {
                InstructionRegTest(cpu, memory, 8, 0b1111_1111, RFlags.HB, 2);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x40;
            memory.R.B = 0;
            for (int i = 0; i < 8; i++)
            {
                InstructionRegTest(cpu, memory, 8, 0, RFlags.ZB | RFlags.HB, 2);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x46;

            for (int i = 0; i < 8; i++)
            {
                InstructionHlTest(cpu, memory, 12, 0b1111_1111, RFlags.HB, 2);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x46;
            memory.SetMappedMemory(memory.R.Hl, 0);
            for (int i = 0; i < 8; i++)
            {
                InstructionHlTest(cpu, memory, 12, 0, RFlags.ZB | RFlags.HB, 2);
                memory.Rom[1] += 8;
            }
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckInc8Instructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x1);
            InitNmInstructionTest(memory, 0x04, 255);
            InstructionRegTest(cpu, memory, 4, 0, RFlags.ZB | RFlags.HB, 1);
            InstructionRegTest(cpu, memory, 4, 1, 0, 1);
            memory.Rom[0] = 0x34;
            InstructionHlTest(cpu, memory, 12, 0, RFlags.ZB | RFlags.HB, 1);
            InstructionHlTest(cpu, memory, 12, 1, 0, 1);
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckDec8Instructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x1);
            InitNmInstructionTest(memory, 0x05, 1);
            InstructionRegTest(cpu, memory, 4, 0, RFlags.ZB | RFlags.NB, 1);
            InstructionRegTest(cpu, memory, 4, 255, RFlags.HB | RFlags.NB, 1);
            memory.Rom[0] = 0x35;
            InstructionHlTest(cpu, memory, 12, 0, RFlags.ZB | RFlags.NB, 1);
            InstructionHlTest(cpu, memory, 12, 255, RFlags.HB | RFlags.NB, 1);
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckAddInstructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x1);
            InitNmInstructionTest(memory, 0x80, 2);
            memory.R.A = 254;
            InstructionArithmeticTest(cpu, memory.R, 4, 0, RFlags.ZB | RFlags.HB | RFlags.CB, 1);
            InstructionArithmeticTest(cpu, memory.R, 4, 2, 0, 1);
            memory.R.A = 254;
            memory.Rom[0] = 0x86;
            InstructionArithmeticTest(cpu, memory.R, 8, 0, RFlags.ZB | RFlags.HB | RFlags.CB, 1);
            InstructionArithmeticTest(cpu, memory.R, 8, 2, 0, 1);
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckXorInstructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x1);
            InitNmInstructionTest(memory, 0xA8, 2);
            memory.R.A = 2;
            InstructionArithmeticTest(cpu, memory.R, 4, 0, RFlags.ZB, 1);
            InstructionArithmeticTest(cpu, memory.R, 4, 2, 0, 1);
            memory.Rom[0] = 0xAE;
            InstructionArithmeticTest(cpu, memory.R, 8, 0, RFlags.ZB, 1);
            InstructionArithmeticTest(cpu, memory.R, 8, 2, 0, 1);
        }

        private static void InitNmInstructionTest(GbMemory m, byte inst, byte initVal)
        {
            m.Rom[0] = inst;
            m.R.B = initVal;
            m.R.Hl = 0xC000;
            m.SetMappedMemory(m.R.Hl, initVal);
        }

        private static void InstructionRegTest(Cpu c, GbMemory mem, int ticks, byte expectedRegData, byte expectedFlags, ushort pcDecCount)
        {
            c.Tick(ticks);
            Assert.AreEqual(expectedRegData, mem.R.B, "Data");
            Assert.AreEqual(expectedFlags, mem.R.F, "Flags");
            mem.R.Pc -= pcDecCount;
        }

        private static void InstructionArithmeticTest(Cpu c, GbRegisters R, int ticks, byte expectedRegVal, byte expectedFlags, ushort pcDecC)
        {
            c.Tick(ticks);
            Assert.AreEqual(expectedRegVal, R.A, "Data");
            Assert.AreEqual(expectedFlags, R.F, "Flags");
            R.Pc -= pcDecC;
        }

        private static void InstructionHlTest(Cpu c, GbMemory mem, int ticks, byte expectedVal, byte expectedFlags, ushort pcDecCount)
        {
            c.Tick(ticks);
            Assert.AreEqual(expectedVal, mem.GetMappedMemoryHl(), " HL");
            Assert.AreEqual(expectedFlags, mem.R.F, " Flags");
            mem.R.Pc -= pcDecCount;
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
