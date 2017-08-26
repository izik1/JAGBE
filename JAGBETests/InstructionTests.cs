using Microsoft.VisualStudio.TestTools.UnitTesting;

using JAGBE.GB.Emulation;

namespace JAGBETests

{
    [TestClass]
    public class InstructionTests
    {
        /// <summary>
        /// Checks that the ADC instruction gives the correct output.
        /// </summary>
        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckAdc()
        {
            GbMemory memory = ConfigureMemory(1);
            InitNmTest(memory, 0x88, 2);
            memory.R.A = 254;
            ArithmeticTest(memory, 0, RFlags.ZHCB);
            ArithmeticTest(memory, 3, 0);
            memory.R.A = 254;
            memory.Rom[0] = 0x8E;
            ArithmeticTest(memory, 0, RFlags.ZHCB);
            ArithmeticTest(memory, 3, 0);
        }

        /// <summary>
        /// Checks that the ADD instruction gives the correct output.
        /// </summary>
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

        /// <summary>
        /// Checks that the AND instruction gives the correct output.
        /// </summary>
        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckAnd()
        {
            GbMemory memory = ConfigureMemory(1);
            InitNmTest(memory, 0xA0, 2);
            memory.R.A = 2;
            ArithmeticTest(memory, 2, RFlags.HB);
            memory.R.A = 1;
            ArithmeticTest(memory, 0, RFlags.ZHB);
            memory.Rom[0] = 0xA6;
            memory.R.A = 2;
            ArithmeticTest(memory, 2, RFlags.HB);
            memory.R.A = 1;
            ArithmeticTest(memory, 0, RFlags.ZHB);
        }

        /// <summary>
        /// Checks that the BIT instruction has the correct timimg and output.
        /// </summary>
        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckBit()
        {
            GbMemory mem = ConfigureMemory(1);
            Instruction instr = new Instruction(0xCB);

            for (int bitNum = 7; bitNum >= 0; bitNum--)
            {
                for (int reg = 7; reg >= 0; reg--)
                {
                    if (reg == 6)
                    {
                        continue;
                    }

                    for (int val = 255; val >= 0; val--)
                    {
                        byte expectedZFlag = (byte)((val & (1 << bitNum)) == 0 ? RFlags.ZB : 0);
                        mem.Rom[0] = (byte)(0x40 + (bitNum * 8) + reg);
                        GbUInt8 cachedVal = (GbUInt8)val;
                        mem.R.SetR8(reg, cachedVal);
                        for (int fVal = 15; fVal >= 0; fVal--)
                        {
                            mem.R.F = (GbUInt8)(fVal << 4);
                            Assert.IsTrue(instr.Run(mem, 1));
                            Assert.AreEqual(cachedVal, mem.R.GetR8(reg)); // The register shouldn't change.
                            Assert.AreEqual(RFlags.HB | ((fVal & 1) == 0 ? (GbUInt8)0 : RFlags.CB) | expectedZFlag, mem.R.F);
                            mem.R.Pc = 0;
                        }
                    }
                }

                mem.R.Hl = 0xC000;
                for (int val = 255; val >= 0; val--)
                {
                    byte expectedZFlag = (byte)((val & (1 << bitNum)) == 0 ? RFlags.ZB : 0);
                    mem.Rom[0] = (byte)(0x46 + (bitNum * 8));
                    GbUInt8 cachedVal = (GbUInt8)val;
                    mem.SetMappedMemoryHl(cachedVal);
                    for (int fVal = 15; fVal >= 0; fVal--)
                    {
                        mem.R.F = (GbUInt8)(fVal << 4);
                        GbUInt8 initFlags = mem.R.F;
                        Assert.IsFalse(instr.Run(mem, 1));
                        Assert.AreEqual(initFlags, mem.R.F);
                        Assert.IsTrue(instr.Run(mem, 2));
                        Assert.AreEqual((GbUInt8)val, mem.GetMappedMemoryHl()); // The value shouldn't change.
                        Assert.AreEqual(RFlags.HB | ((fVal & 1) == 0 ? (GbUInt8)0 : RFlags.CB) | expectedZFlag, mem.R.F);
                        mem.R.Pc = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Checks that the Complement Carry flag instruction gives the correct output.
        /// </summary>
        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckCcf()
        {
            GbMemory mem = ConfigureMemory(1);
            InitNmTest(mem, 0x3F, 0);
            RegTest(mem, 0, RFlags.CB);
            RegTest(mem, 0, 0);
            mem.R.F = RFlags.ZB;
            RegTest(mem, 0, RFlags.ZCB);
            RegTest(mem, 0, RFlags.ZB);
        }

        /// <summary>
        /// Checks that the CP instruction gives the correct output.
        /// </summary>
        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckCp()
        {
            GbMemory memory = ConfigureMemory(1);
            InitNmTest(memory, 0xB8, 2);
            memory.R.A = 2;
            ArithmeticTest(memory, 2, RFlags.ZNB);
            memory.R.A = 0;
            ArithmeticTest(memory, 0, RFlags.NHCB);
            memory.Rom[0] = 0xBE;
            memory.R.A = 2;
            ArithmeticTest(memory, 2, RFlags.ZNB);
            memory.R.A = 0;
            ArithmeticTest(memory, 0, RFlags.NHCB);
        }

        /// <summary>
        /// Checks that the CPL instruction gives the correct output.
        /// </summary>
        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckCpl()
        {
            GbMemory memory = ConfigureMemory(1);
            InitNmTest(memory, 0x2F, 0);
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    memory.R.A = (byte)i;
                    memory.R.F = (byte)(j << 4);
                    ArithmeticTest(memory, (byte)(255 - i), (byte)(memory.R.F | RFlags.NHB));
                    ArithmeticTest(memory, (byte)i, (byte)(memory.R.F | RFlags.NHB));
                }
            }
        }

        /// <summary>
        /// Checks that the Dec8 instruction gives the correct output.
        /// </summary>
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

        /// <summary>
        /// Checks that the Inc8 instruction gives the correct output.
        /// </summary>
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

        /// <summary>
        /// Checks that the JpA16 instructions gives the correct output.
        /// </summary>
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

                    Assert.AreEqual((GbUInt16)(((i & 1) == 0) ^ r == 1 ? 0xC : 3), memory.R.Pc);
                    memory.R.Pc = 0;
                    memory.Rom[0] += 8;
                }

                memory.R.F = RFlags.ZCB;
            }
        }

        /// <summary>
        /// Checks the timing and output values for ld r,r instructions.
        /// </summary>
        [TestMethod]
        [TestCategory("Ld")]
        public void CheckLdR8R8()
        {
            GbMemory mem = ConfigureMemory(0);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (i == 6 || j == 6)
                    {
                        continue;
                    }

                    GbUInt8 destR = (GbUInt8)i;
                    GbUInt8 srcR = (GbUInt8)j;
                    Instruction instr = new Instruction((GbUInt8)(0x40 + (destR * 8) + srcR));
                    for (int k = 0; k < 256; k++)
                    {
                        mem.R.SetR8(destR, (GbUInt8)(255 - k)); // Ensure that dest is always different from source.
                        mem.R.SetR8(srcR, (GbUInt8)k);
                        Assert.IsTrue(instr.Run(mem, 0)); // Check timing.
                        Assert.AreEqual((GbUInt8)k, mem.R.GetR8(destR)); // Check value.
                    }
                }
            }
        }

        /// <summary>
        /// Checks that Ld r,(hl) and Ld (hl),r give the correct output and have the correct timing.
        /// </summary>
        [TestMethod]
        [TestCategory("Ld")]
        public void CheckLdrHl()
        {
            GbMemory mem = new GbMemory();
            mem.R.Hl = 0xC000;
            for (int i = 0; i < 8; i++)
            {
                if (i == 4 || i == 5 || i == 6)
                {
                    if (i != 6)
                    {
                        Instruction inst = new Instruction((GbUInt8)(0x46 + (i * 8)));
                        Assert.IsFalse(inst.Run(mem, 0));
                        Assert.IsTrue(inst.Run(mem, 1));

                        inst = new Instruction((GbUInt8)(0x70 + i));
                        Assert.IsFalse(inst.Run(mem, 0));
                        Assert.IsTrue(inst.Run(mem, 1));
                    }

                    continue;
                }

                for (int j = 0; j < 256; j++)
                {
                    GbUInt8 overriddenVal = (GbUInt8)(255 - j);
                    mem.R.SetR8(i, overriddenVal);
                    mem.SetMappedMemoryHl((GbUInt8)j);
                    Instruction inst = new Instruction((GbUInt8)(0x46 + (i * 8)));
                    Assert.IsFalse(inst.Run(mem, 0));
                    Assert.AreEqual(overriddenVal, mem.R.GetR8(i));
                    Assert.IsTrue(inst.Run(mem, 1));
                    Assert.AreEqual((GbUInt8)j, mem.R.GetR8(i));

                    mem.R.SetR8(i, (GbUInt8)j);
                    mem.SetMappedMemoryHl((GbUInt8)(255 - j));
                    inst = new Instruction((GbUInt8)(0x70 + i));
                    Assert.IsFalse(inst.Run(mem, 0));
                    Assert.AreEqual(overriddenVal, mem.GetMappedMemoryHl());
                    Assert.IsTrue(inst.Run(mem, 1));
                    Assert.AreEqual((GbUInt8)j, mem.GetMappedMemoryHl());
                }
            }
        }

        /// <summary>
        /// Checks that the OR instruction gives the correct output.
        /// </summary>
        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckOr()
        {
            GbMemory memory = ConfigureMemory(1);
            InitNmTest(memory, 0xB0, 0);
            memory.R.A = 0;
            ArithmeticTest(memory, 0, RFlags.ZB);
            memory.R.A = 1;
            ArithmeticTest(memory, 1, 0);
            memory.Rom[0] = 0xB6;
            memory.R.A = 0;
            ArithmeticTest(memory, 0, RFlags.ZB);
            memory.R.A = 1;
            ArithmeticTest(memory, 1, 0);
        }

        /// <summary>
        /// Checks that the RL instruction gives the correct output.
        /// </summary>
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

        /// <summary>
        /// Checks that the RLC instruction gives the correct output.
        /// </summary>
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

        /// <summary>
        /// Checks that the RR instruction gives the correct output.
        /// </summary>
        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckRr()
        {
            GbMemory memory = ConfigureMemory(2);
            InitCbTest(memory, 0x18, 2);
            RegTest(memory, 1, 0);
            RegTest(memory, 0, RFlags.ZCB);
            RegTest(memory, 0x80, 0);
            memory.Rom[1] = 0x1E;
            HlTest(memory, 1, 0);
            HlTest(memory, 0, RFlags.ZCB);
            HlTest(memory, 0x80, 0);
        }

        /// <summary>
        /// Checks that the SET instruction gives the correct output.
        /// </summary>
        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckSet()
        {
            GbMemory memory = ConfigureMemory(2);
            InitCbTest(memory, 0xC0, 0);
            byte v = 1;
            for (int i = 0; i < 8; i++)
            {
                RegTest(memory, v, 0);
                v <<= 1;
                v++;
                memory.Rom[1] += 8;
            }

            v = 1;
            memory.Rom[1] = 0xC6;

            for (int i = 0; i < 8; i++)
            {
                HlTest(memory, v, 0);
                v <<= 1;
                v++;
                memory.Rom[1] += 8;
            }
        }

        /// <summary>
        /// Checks that the SLA instruction gives the correct output.
        /// </summary>
        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckSla()
        {
            GbMemory mem = ConfigureMemory(2);
            mem.Rom[0] = 0xCB;

            for (int i = 0; i < 8; i++)
            {
                mem.Rom[1] = (byte)(0x20 + i);
                mem.R.Hl = 0xC000;
                for (int j = 0; j < 256; j++)
                {
                    byte expectedFlags = j > 0x80 ? RFlags.CB : j == 0x80 ? RFlags.ZCB : j == 0 ? RFlags.ZB : (byte)0;
                    mem.R.F = 0;
                    if (i == 6)
                    {
                        mem.SetMappedMemoryHl((GbUInt8)j);
                        System.Console.WriteLine(j);
                        HlTest(mem, (byte)(j << 1), expectedFlags);
                    }
                    else
                    {
                        mem.R.SetR8(i, (GbUInt8)j);
                        RegTest(mem, (byte)(j << 1), expectedFlags, i);
                    }
                }
            }
        }

        /// <summary>
        /// Checks that the SUB instruction gives the correct output.
        /// </summary>
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

        /// <summary>
        /// Checks that the XOR instruction gives the correct output.
        /// </summary>
        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckXor()
        {
            GbMemory memory = ConfigureMemory(2);
            InitNmTest(memory, 0xA8, 2);
            memory.R.A = 2;
            memory.Rom[1] = 2;
            ArithmeticTest(memory, 0, RFlags.ZB);
            ArithmeticTest(memory, 2, 0);
            memory.Rom[0] = 0xAE;
            ArithmeticTest(memory, 0, RFlags.ZB);
            ArithmeticTest(memory, 2, 0);
            memory.Rom[0] = 0xEE;
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

        private static void BranchTest(GbMemory mem, GbUInt16 expectedAddress)
        {
            RunInst(mem);
            Assert.AreEqual(expectedAddress, mem.R.Pc);
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
            m.SetMappedMemoryHl(initVal);
        }

        private static void InitNmTest(GbMemory m, byte inst, byte initVal)
        {
            m.Rom[0] = inst;
            m.R.B = initVal;
            m.R.Hl = 0xC000;
            m.SetMappedMemoryHl(initVal);
        }

        private static void RegTest(GbMemory mem, byte expectedRegData, byte expectedFlags) => RegTest(mem, expectedRegData, expectedFlags, 0);

        private static void RegTest(GbMemory mem, GbUInt8 expectedRegData, byte expectedFlags, int reg)
        {
            RunInst(mem);
            Assert.AreEqual(expectedRegData, mem.R.GetR8(reg), "Data");
            Assert.AreEqual(expectedFlags, mem.R.F, "Flags");
            mem.R.Pc = 0;
        }

        private static void RunInst(GbMemory mem) => RunInst(mem, new Instruction(mem.LdI8()));

        private static void RunInst(GbMemory mem, Instruction inst)
        {
            int step = 0;
            while (!inst.Run(mem, step))
            {
                step++;
            }
        }
    }
}
