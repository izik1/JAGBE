using Microsoft.VisualStudio.TestTools.UnitTesting;
using JAGBE.GB.Computation;
using JAGBE.GB.DataTypes;
using JAGBE.GB.Computation.Execution;

namespace JAGBETests

{
    [TestClass]
    public class InstructionTests
    {
        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckAdd()
        {
            GbMemory memory = ConfigureMemory(1);
            InitNmTest(memory, 0x80, 2);
            memory.R.A = 254;
            ArithmeticTest(memory, 0, RFlags.ZHCB);
            ArithmeticTest(memory, 2, 0);
            memory.R.A = 254;
            memory.Rom[0] = 0x86;
            ArithmeticTest(memory, 0, RFlags.ZHCB);
            ArithmeticTest(memory, 2, 0);
        }

        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckBit()
        {
            GbMemory memory = ConfigureMemory(2);
            InitCbTest(memory, 0x40, 0xFF);

            for (int i = 0; i < 8; i++)
            {
                RegTest(memory, 0xFF, RFlags.HB);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x40;
            memory.R.B = 0;
            for (int i = 0; i < 8; i++)
            {
                RegTest(memory, 0, RFlags.ZHB);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x46;

            for (int i = 0; i < 8; i++)
            {
                HlTest(memory, 0xFF, RFlags.HB);
                memory.Rom[1] += 8;
            }

            memory.Rom[1] = 0x46;
            memory.SetMappedMemory(memory.R.Hl, 0);
            for (int i = 0; i < 8; i++)
            {
                HlTest(memory, 0, RFlags.ZHB);
                memory.Rom[1] += 8;
            }
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckDec8()
        {
            GbMemory memory = ConfigureMemory(1);
            InitNmTest(memory, 0x05, 1);
            RegTest(memory, 0, RFlags.ZNB);
            RegTest(memory, 255, RFlags.NHB);
            memory.Rom[0] = 0x35;
            HlTest(memory, 0, RFlags.ZNB);
            HlTest(memory, 255, RFlags.NHB);
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckInc8()
        {
            GbMemory memory = ConfigureMemory(1);
            InitNmTest(memory, 0x04, 255);
            RegTest(memory, 0, RFlags.ZHB);
            RegTest(memory, 1, 0);
            memory.Rom[0] = 0x34;
            HlTest(memory, 0, RFlags.ZHB);
            HlTest(memory, 1, 0);
        }

        [TestMethod]
        [TestCategory("Branching")]
        public void CheckJpA16()
        {
            GbMemory memory = ConfigureMemory(0x10);
            memory.Rom[0] = 0xC3;
            memory.Rom[1] = 0xC; // Low byte, all that is needed.

            BranchTest(memory, 0xC);
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
        public void CheckRl()
        {
            GbMemory memory = ConfigureMemory(2);
            InitCbTest(memory, 0x10, 0x40);
            RegTest(memory, 0x80, 0);
            RegTest(memory, 0, RFlags.ZCB);
            RegTest(memory, 1, 0);
            memory.Rom[1] = 0x16;
            memory.R.F = 0;
            HlTest(memory, 0x80, 0);
            HlTest(memory, 0, RFlags.ZCB);
            HlTest(memory, 1, 0);
        }

        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckRlc()
        {
            GbMemory memory = ConfigureMemory(2);
            InitCbTest(memory, 0, 0x40);
            RegTest(memory, 0x80, 0);
            RegTest(memory, 0, RFlags.ZCB);
            memory.Rom[1] = 0x06;
            HlTest(memory, 0x80, 0);
            HlTest(memory, 0, RFlags.ZCB);
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckSub()
        {
            GbMemory memory = ConfigureMemory(1);
            InitNmTest(memory, 0x90, 2);
            memory.R.A = 2;
            ArithmeticTest(memory, 0, RFlags.ZNB);
            ArithmeticTest(memory, 254, RFlags.NHCB);
            memory.R.A = 2;
            memory.Rom[0] = 0x96;
            ArithmeticTest(memory, 0, RFlags.ZNB);
            ArithmeticTest(memory, 254, RFlags.NHCB);
        }

        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckXor()
        {
            GbMemory memory = ConfigureMemory(1);
            InitNmTest(memory, 0xA8, 2);
            memory.R.A = 2;
            ArithmeticTest(memory, 0, RFlags.ZB);
            ArithmeticTest(memory, 2, 0);
            memory.Rom[0] = 0xAE;
            ArithmeticTest(memory, 0, RFlags.ZB);
            ArithmeticTest(memory, 2, 0);
        }

        private static void ArithmeticTest(GbMemory mem, byte expectedRegVal, byte expectedFlags)
        {
            RunInst(mem);
            Assert.AreEqual(expectedRegVal, mem.R.A, "Data");
            Assert.AreEqual(expectedFlags, mem.R.F, "Flags");
            mem.R.Pc = 0;
        }

        private static void BranchTest(GbMemory mem, ushort expectedAddress)
        {
            RunInst(mem);
            Assert.AreEqual(expectedAddress, mem.R.Pc.Value);
            mem.R.Pc = 0;
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

        private static void HlTest(GbMemory mem, byte expectedVal, byte expectedFlags)
        {
            RunInst(mem);
            Assert.AreEqual(expectedVal, mem.GetMappedMemoryHl(), "HL");
            Assert.AreEqual(expectedFlags, mem.R.F, "Flags");
            mem.R.Pc = 0;
        }

        private static void InitCbTest(GbMemory m, byte inst, byte initVal)
        {
            m.Rom[0] = 0xCB;
            m.Rom[1] = inst;
            m.R.B = initVal;
            m.R.Hl = 0xC000;
            m.SetMappedMemory(m.R.Hl, initVal);
        }

        private static void InitNmTest(GbMemory m, byte inst, byte initVal)
        {
            m.Rom[0] = inst;
            m.R.B = initVal;
            m.R.Hl = 0xC000;
            m.SetMappedMemory(m.R.Hl, initVal);
        }

        private static void RegTest(GbMemory mem, byte expectedRegData, byte expectedFlags)
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
