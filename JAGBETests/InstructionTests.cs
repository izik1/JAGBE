using Microsoft.VisualStudio.TestTools.UnitTesting;
using JAGBE.GB.Emulation;
using System;

namespace JAGBETests
{
    [TestClass]
    public class InstructionTests
    {
        private delegate void InstructionTest(int dest, int src, byte val, GbMemory mem);

        private void TestInstruction(InstructionTest testFunc)
        {
            if (testFunc == null)
            {
                throw new ArgumentNullException(nameof(testFunc));
            }

            GbMemory mem = ConfigureMemory(0, 0);
            for (int dest = 0; dest < 8; dest++)
            {
                for (int src = 0; src < 8; src++)
                {
                    for (int val = 0; val < 256; val++)
                    {
                        testFunc(dest, src, (byte)val, mem);
                    }
                }
            }
        }

        /// <summary>
        /// Checks that the ADC instruction gives the correct output.
        /// </summary>
        [TestMethod]
        [TestCategory("Arithmetic")]
        public void CheckAdc()
        {
            GbMemory memory = InitTest(2, 0x88);
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
            GbMemory memory = InitTest(2, 0x80);
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
            GbMemory memory = InitTest(2, 0xA0);
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
        public void CheckBit() => TestInstruction((bitNum, reg, val, mem) =>
        {
            mem.R.Hl = 0xC000;
            Instruction instr = new Instruction(0xCB);
            GbUInt8 cachedVal = val;
            byte expectedZFlag = (byte)((val & (1 << bitNum)) == 0 ? RFlags.ZB : 0);
            mem.Rom[0] = (byte)(0x40 + (bitNum * 8) + reg);
            int i = 1;
            if (reg == 6)
            {
                mem.SetMappedMemoryHl(cachedVal);
                i++;
            }
            else
            {
                mem.R.SetR8(reg, cachedVal);
            }
            for (int fVal = 15; fVal >= 0; fVal--)
            {
                mem.R.F = (GbUInt8)(fVal << 4);
                GbUInt8 initFlags = mem.R.F;
                if (reg == 6)
                {
                    Assert.IsFalse(instr.Run(mem, 1));
                    Assert.AreEqual(initFlags, mem.R.F);
                }

                Assert.IsTrue(instr.Run(mem, i));
                Assert.AreEqual(cachedVal, reg == 6 ? mem.GetMappedMemoryHl() : mem.R.GetR8(reg)); // The value shouldn't change.
                Assert.AreEqual(RFlags.HB | ((fVal & 1) << RFlags.CF) | expectedZFlag, mem.R.F);
                mem.R.Pc = 0;
            }
        });

        /// <summary>
        /// Checks that the Complement Carry flag instruction gives the correct output.
        /// </summary>
        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckCcf()
        {
            GbMemory mem = ConfigureMemory(0x3F);
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
            GbMemory memory = InitTest(2, 0xB8);
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
            GbMemory memory = InitTest(0, 0x2F);
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
            GbMemory memory = InitTest(1, 0x05);
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
            GbMemory memory = InitTest(0xFF, 0x04);
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
            GbMemory memory = ConfigureMemory(0xC3, 0xC, 0);
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
        public void CheckLdR8R8() => TestInstruction((dest, src, val, mem) =>
        {
            if (dest == 6 || src == 6)
            {
                return;
            }

            Instruction instr = new Instruction((GbUInt8)(0x40 + (dest * 8) + src));
            mem.R.SetR8(dest, (GbUInt8)(255 - val)); // Ensure that dest is always different from source.
            mem.R.SetR8(src, val);
            Assert.IsTrue(instr.Run(mem, 0)); // Check timing.
            Assert.AreEqual(val, mem.R.GetR8(dest)); // Check value.
        });

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
            GbMemory memory = InitTest(0, 0xB0);
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
            GbMemory memory = InitTest(0x40, 0xCB, 0x10);
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
            GbMemory memory = InitTest(0x40, 0xCB, 0);
            RegTest(memory, 0x80, 0);
            RegTest(memory, 1, RFlags.CB);
            memory.Rom[1] = 0x06;
            HlTest(memory, 0x80, 0);
            HlTest(memory, 1, RFlags.CB);
        }

        /// <summary>
        /// Checks that the RR instruction gives the correct output.
        /// </summary>
        [TestMethod]
        [TestCategory("Bitwise")]
        public void CheckRr()
        {
            GbMemory memory = InitTest(2, 0xCB, 0x18);
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
            GbMemory memory = InitTest(0, 0xCB, 0xC0);
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
            GbMemory mem = ConfigureMemory(0xCB, 0);
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
            GbMemory memory = InitTest(2, 0x90);
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
            GbMemory memory = InitTest(2, 0xA8, 2);
            memory.R.A = 2;
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

        private static GbMemory ConfigureMemory(params byte[] rom)
        {
            if (rom == null)
            {
                throw new ArgumentNullException(nameof(rom));
            }

            GbMemory mem = new GbMemory
            {
                Rom = rom
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

        private static GbMemory InitTest(byte initVal, params byte[] rom)
        {
            GbMemory m = ConfigureMemory(rom);
            m.R.B = initVal;
            m.R.Hl = 0xC000;
            m.SetMappedMemoryHl(initVal);
            return m;
        }

        private static void RegTest(GbMemory mem, byte expectedRegData, byte expectedFlags) =>
            RegTest(mem, expectedRegData, expectedFlags, 0);

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
