using Microsoft.VisualStudio.TestTools.UnitTesting;
using JAGBE.GB.Computation;
using JAGBE.GB.DataTypes;

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
            InitCbInstructionTest(memory, 0, 0x40);
            InstructionRegTest(cpu, memory, 8, 0x80, 0);
            InstructionRegTest(cpu, memory, 8, 0, RFlags.CB | RFlags.ZB);
            memory.Rom[1] = 0x06;
            InstructionHlTest(cpu, memory, 16, 0x80, 0);
            InstructionHlTest(cpu, memory, 16, 0, RFlags.CB | RFlags.ZB);
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
            InitCbInstructionTest(memory, 0x10, 0x40);
            InstructionRegTest(cpu, memory, 8, 0x80, 0);
            InstructionRegTest(cpu, memory, 8, 0, RFlags.CB | RFlags.ZB);
            InstructionRegTest(cpu, memory, 8, 1, 0);
            memory.Rom[1] = 0x16;
            memory.R.F = 0;
            InstructionHlTest(cpu, memory, 16, 0x80, 0);
            InstructionHlTest(cpu, memory, 16, 0, RFlags.CB | RFlags.ZB);
            InstructionHlTest(cpu, memory, 16, 1, 0);
        }

        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckBitInstructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x2);
            InitCbInstructionTest(memory, 0x40, 0xFF);

            for (int i = 0; i < 8; i++)
            {
                InstructionRegTest(cpu, memory, 8, 0xFF, RFlags.HB);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x40;
            memory.R.B = 0;
            for (int i = 0; i < 8; i++)
            {
                InstructionRegTest(cpu, memory, 8, 0, RFlags.ZB | RFlags.HB);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x46;

            for (int i = 0; i < 8; i++)
            {
                InstructionHlTest(cpu, memory, 12, 0xFF, RFlags.HB);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x46;
            memory.SetMappedMemory(memory.R.Hl, 0);
            for (int i = 0; i < 8; i++)
            {
                InstructionHlTest(cpu, memory, 12, 0, RFlags.ZB | RFlags.HB);
                memory.Rom[1] += 8;
            }
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckInc8Instructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x1);
            InitNmInstructionTest(memory, 0x04, 255);
            InstructionRegTest(cpu, memory, 4, 0, RFlags.ZB | RFlags.HB);
            InstructionRegTest(cpu, memory, 4, 1, 0);
            memory.Rom[0] = 0x34;
            InstructionHlTest(cpu, memory, 12, 0, RFlags.ZB | RFlags.HB);
            InstructionHlTest(cpu, memory, 12, 1, 0);
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckDec8Instructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x1);
            InitNmInstructionTest(memory, 0x05, 1);
            InstructionRegTest(cpu, memory, 4, 0, RFlags.ZB | RFlags.NB);
            InstructionRegTest(cpu, memory, 4, 255, RFlags.HB | RFlags.NB);
            memory.Rom[0] = 0x35;
            InstructionHlTest(cpu, memory, 12, 0, RFlags.ZB | RFlags.NB);
            InstructionHlTest(cpu, memory, 12, 255, RFlags.HB | RFlags.NB);
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckAddInstructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x1);
            InitNmInstructionTest(memory, 0x80, 2);
            memory.R.A = 254;
            InstructionArithmeticTest(cpu, memory.R, 4, 0, RFlags.ZB | RFlags.HB | RFlags.CB);
            InstructionArithmeticTest(cpu, memory.R, 4, 2, 0);
            memory.R.A = 254;
            memory.Rom[0] = 0x86;
            InstructionArithmeticTest(cpu, memory.R, 8, 0, RFlags.ZB | RFlags.HB | RFlags.CB);
            InstructionArithmeticTest(cpu, memory.R, 8, 2, 0);
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckXorInstructions()
        {
            (Cpu cpu, GbMemory memory) = ConfigureCpu(0x1);
            InitNmInstructionTest(memory, 0xA8, 2);
            memory.R.A = 2;
            InstructionArithmeticTest(cpu, memory.R, 4, 0, RFlags.ZB);
            InstructionArithmeticTest(cpu, memory.R, 4, 2, 0);
            memory.Rom[0] = 0xAE;
            InstructionArithmeticTest(cpu, memory.R, 8, 0, RFlags.ZB);
            InstructionArithmeticTest(cpu, memory.R, 8, 2, 0);
        }

        private static void InitNmInstructionTest(GbMemory m, byte inst, byte initVal)
        {
            m.Rom[0] = inst;
            m.R.B = initVal;
            m.R.Hl = 0xC000;
            m.SetMappedMemory(m.R.Hl, initVal);
        }

        private static void InstructionRegTest(Cpu c, GbMemory mem, int ticks, byte expectedRegData, byte expectedFlags)
        {
            c.Tick(ticks);
            Assert.AreEqual(expectedRegData, mem.R.B, "Data");
            Assert.AreEqual(expectedFlags, mem.R.F, "Flags");
            mem.R.Pc = 0;
        }

        private static void InstructionArithmeticTest(Cpu c, GbRegisters R, int ticks, byte expectedRegVal, byte expectedFlags)
        {
            c.Tick(ticks);
            Assert.AreEqual(expectedRegVal, R.A, "Data");
            Assert.AreEqual(expectedFlags, R.F, "Flags");
            R.Pc = 0;
        }

        private static void InstructionHlTest(Cpu c, GbMemory mem, int ticks, byte expectedVal, byte expectedFlags)
        {
            c.Tick(ticks);
            Assert.AreEqual(expectedVal, mem.GetMappedMemoryHl(), "HL");
            Assert.AreEqual(expectedFlags, mem.R.F, "Flags");
            mem.R.Pc = 0;
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
