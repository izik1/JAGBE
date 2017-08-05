using Microsoft.VisualStudio.TestTools.UnitTesting;
using JAGBE.GB.Computation;
using JAGBE.GB.DataTypes;
using JAGBE.GB.Computation.Execution;

namespace JAGBETests

{
    [TestClass]
    public class CpuTests
    {
        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckAddInstructions()
        {
            GbMemory memory = ConfigureMemory(1);
            InitNmInstructionTest(memory, 0x80, 2);
            memory.R.A = 254;
            InstructionArithmeticTest(memory, 0, RFlags.ZB | RFlags.HB | RFlags.CB);
            InstructionArithmeticTest(memory, 2, 0);
            memory.R.A = 254;
            memory.Rom[0] = 0x86;
            InstructionArithmeticTest(memory, 0, RFlags.ZB | RFlags.HB | RFlags.CB);
            InstructionArithmeticTest(memory, 2, 0);
        }

        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckBitInstructions()
        {
            GbMemory memory = ConfigureMemory(0x2);
            InitCbInstructionTest(memory, 0x40, 0xFF);

            for (int i = 0; i < 8; i++)
            {
                InstructionRegTest(memory, 0xFF, RFlags.HB);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x40;
            memory.R.B = 0;
            for (int i = 0; i < 8; i++)
            {
                InstructionRegTest(memory, 0, RFlags.ZB | RFlags.HB);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x46;

            for (int i = 0; i < 8; i++)
            {
                InstructionHlTest(memory, 0xFF, RFlags.HB);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x46;
            memory.SetMappedMemory(memory.R.Hl, 0);
            for (int i = 0; i < 8; i++)
            {
                InstructionHlTest(memory, 0, RFlags.ZB | RFlags.HB);
                memory.Rom[1] += 8;
            }
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckDec8Instructions()
        {
            GbMemory memory = ConfigureMemory(0x2);
            InitNmInstructionTest(memory, 0x05, 1);
            InstructionRegTest(memory, 0, RFlags.ZNB);
            InstructionRegTest(memory, 255, RFlags.NHB);
            memory.Rom[0] = 0x35;
            InstructionHlTest(memory, 0, RFlags.ZNB);
            InstructionHlTest(memory, 255, RFlags.NHB);
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckInc8Instructions()
        {
            GbMemory memory = ConfigureMemory(0x2);
            InitNmInstructionTest(memory, 0x04, 255);
            InstructionRegTest(memory, 0, RFlags.ZHB);
            InstructionRegTest(memory, 1, 0);
            memory.Rom[0] = 0x34;
            InstructionHlTest(memory, 0, RFlags.ZHB);
            InstructionHlTest(memory, 1, 0);
        }

        [TestMethod]
        [TestCategory("Branching")]
        public void CheckJpA16Instructions()
        {
            GbMemory memory = ConfigureMemory(0x10);
            memory.Rom[0] = 0xC3;
            memory.Rom[1] = 0xC; // Low byte, all that is needed.

            InstructionBranchTest(memory, 0xC);
            for (int r = 0; r < 2; r++)
            {
                memory.Rom[0] = 0xC2;
                for (int i = 0; i < 4; i++)
                {
                    RunInst(memory);

                    Assert.AreEqual((ushort)(((i & 1) == 0) ^ r == 1 ? 0xC : 3), memory.R.Pc.Value);
                    memory.R.Pc = 0;
                    memory.Rom[0] += 8;
                }

                memory.R.F = RFlags.ZCB;
            }
        }

        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckRlcInstructions()
        {
            GbMemory memory = ConfigureMemory(0x2);
            InitCbInstructionTest(memory, 0, 0x40);
            InstructionRegTest(memory, 0x80, 0);
            InstructionRegTest(memory, 0, RFlags.ZCB);
            memory.Rom[1] = 0x06;
            InstructionHlTest(memory, 0x80, 0);
            InstructionHlTest(memory, 0, RFlags.ZCB);
        }

        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckRlInstructions()
        {
            GbMemory memory = ConfigureMemory(0x2);
            InitCbInstructionTest(memory, 0x10, 0x40);
            InstructionRegTest(memory, 0x80, 0);
            InstructionRegTest(memory, 0, RFlags.ZCB);
            InstructionRegTest(memory, 1, 0);
            memory.Rom[1] = 0x16;
            memory.R.F = 0;
            InstructionHlTest(memory, 0x80, 0);
            InstructionHlTest(memory, 0, RFlags.ZCB);
            InstructionHlTest(memory, 1, 0);
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckXorInstructions()
        {
            GbMemory memory = ConfigureMemory(1);
            InitNmInstructionTest(memory, 0xA8, 2);
            memory.R.A = 2;
            InstructionArithmeticTest(memory, 0, RFlags.ZB);
            InstructionArithmeticTest(memory, 2, 0);
            memory.Rom[0] = 0xAE;
            InstructionArithmeticTest(memory, 0, RFlags.ZB);
            InstructionArithmeticTest(memory, 2, 0);
        }

        private static GbMemory ConfigureMemory(int romSize)
        {
            GbMemory mem = new GbMemory
            {
                Rom = new byte[romSize]
            };
            mem.SetMappedMemory(0xFF50, 1); // Force bootmode to be disabled.
            return mem;
        }

        private static void InitCbInstructionTest(GbMemory m, byte inst, byte initVal)
        {
            m.Rom[0] = 0xCB;
            m.Rom[1] = inst;
            m.R.B = initVal;
            m.R.Hl = 0xC000;
            m.SetMappedMemory(m.R.Hl, initVal);
        }

        private static void InitNmInstructionTest(GbMemory m, byte inst, byte initVal)
        {
            m.Rom[0] = inst;
            m.R.B = initVal;
            m.R.Hl = 0xC000;
            m.SetMappedMemory(m.R.Hl, initVal);
        }

        private static void InstructionArithmeticTest(GbMemory mem, byte expectedRegVal, byte expectedFlags)
        {
            RunInst(mem);
            Assert.AreEqual(expectedRegVal, mem.R.A, "Data");
            Assert.AreEqual(expectedFlags, mem.R.F, "Flags");
            mem.R.Pc = 0;
        }

        private static void InstructionBranchTest(GbMemory mem, ushort expectedAddress)
        {
            RunInst(mem);
            Assert.AreEqual(expectedAddress, mem.R.Pc.Value);
            mem.R.Pc = 0;
        }

        private static void InstructionHlTest(GbMemory mem, byte expectedVal, byte expectedFlags)
        {
            RunInst(mem);
            Assert.AreEqual(expectedVal, mem.GetMappedMemoryHl(), "HL");
            Assert.AreEqual(expectedFlags, mem.R.F, "Flags");
            mem.R.Pc = 0;
        }

        private static void InstructionRegTest(GbMemory mem, byte expectedRegData, byte expectedFlags)
        {
            RunInst(mem);
            Assert.AreEqual(expectedRegData, mem.R.B, "Data");
            Assert.AreEqual(expectedFlags, mem.R.F, "Flags");
            mem.R.Pc = 0;
        }

        private static void RunInst(GbMemory mem)
        {
            Instruction inst = new Instruction(mem.LdI8());
            int step = 0;
            while (!inst.Run(mem, step))
            {
                step++;
            }
        }
    }
}
