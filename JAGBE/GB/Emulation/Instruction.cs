namespace JAGBE.GB.Emulation
{
    internal static class Instruction
    {
        /// <summary>
        /// The <see cref="Opcode"/> array containing the functions ran for CB prefixed instructions.
        /// </summary>
        private static readonly Opcode[] CbOps = GetCbOps();

        /// <summary>
        /// An opcode representing invalid instructions.
        /// </summary>
        private static readonly Opcode InvalidOpcode = new Opcode(0, 0, (o, m) =>
        {
            m.Status = CpuState.HUNG;
            return 1;
        });

        /// <summary>
        /// The <see cref="Opcode"/> containing instructions ran for non-CB prefixed instructions.
        /// </summary>
        private static readonly Opcode[] NmOps = GetNmOps();

        /// <summary>
        /// Runs an instruction and returns the number of ticks it took to complete.
        /// </summary>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks that the instruction took to run.</returns>
        public static int Run(GbMemory memory)
        {
            byte opcode = (byte)memory.LdI8();
            if (memory.HaltBugged)
            {
                memory.HaltBugged = false;
                memory.R.Pc--;
            }

            return NmOps[opcode].Invoke(memory);
        }

        /// <summary>
        /// Gets the CB prefixed instructions.
        /// </summary>
        /// <returns>The CB prefixed instructions.</returns>
        private static Opcode[] GetCbOps()
        {
            Opcode[] ops = new Opcode[0x100];
            for (int i = 0; i < 8; i++)
            {
                ops[i + 0x00] = new Opcode(0, (byte)i, Alu.Bitwise.Rlc);
                ops[i + 0x08] = new Opcode(0, (byte)i, Alu.Bitwise.Rrc);
                ops[i + 0x10] = new Opcode(0, (byte)i, Alu.Bitwise.Rl);
                ops[i + 0x18] = new Opcode(0, (byte)i, Alu.Bitwise.Rr);
                ops[i + 0x20] = new Opcode(0, (byte)i, Alu.Bitwise.Sla);
                ops[i + 0x28] = new Opcode(0, (byte)i, Alu.Bitwise.Sra);
                ops[i + 0x30] = new Opcode(0, (byte)i, Alu.Bitwise.Swap);
                ops[i + 0x38] = new Opcode(0, (byte)i, Alu.Bitwise.Srl);
            }

            for (int i = 0; i < 0x40; i++)
            {
                ops[i + 0x40] = new Opcode((byte)((i >> 3) & 7), (byte)(i & 7), Alu.Bitwise.Bit);
                ops[i + 0x80] = new Opcode((byte)((i >> 3) & 7), (byte)(i & 7), Alu.Bitwise.Res);
                ops[i + 0xC0] = new Opcode((byte)((i >> 3) & 7), (byte)(i & 7), Alu.Bitwise.Set);
            }

            return ops;
        }

        /// <summary>
        /// Gets the normal instructions.
        /// </summary>
        /// <returns>The normal instructions.</returns>
        private static Opcode[] GetNmOps()
        {
            Opcode[] ops = new Opcode[0x100];
            for (int i = 0; i < 0x40; i++)
            {
                if (i != 0x36)
                {
                    ops[i + 0x40] = new Opcode((byte)((i >> 3) & 7), (byte)(i & 7), Alu.Loading.Ld8);
                }
            }

            for (int i = 0; i < 8; i++)
            {
                ops[(i * 8) + 0x04] = new Opcode((byte)i, (byte)i, Alu.Arithmetic.Inc8);
                ops[(i * 8) + 0x05] = new Opcode((byte)i, (byte)i, Alu.Arithmetic.Dec8);
                ops[(i * 8) + 0x06] = new Opcode((byte)i, 0, Alu.Loading.LdD8);
                ops[(i * 1) + 0x80] = new Opcode(7, (byte)(i & 7), Alu.Arithmetic.Add);
                ops[(i * 1) + 0x88] = new Opcode(7, (byte)(i & 7), Alu.Arithmetic.Adc);
                ops[(i * 1) + 0x90] = new Opcode(7, (byte)(i & 7), Alu.Arithmetic.Sub);
                ops[(i * 1) + 0x98] = new Opcode(7, (byte)(i & 7), Alu.Arithmetic.Sbc);
                ops[(i * 1) + 0xA0] = new Opcode(7, (byte)(i & 7), Alu.Arithmetic.And);
                ops[(i * 1) + 0xA8] = new Opcode(7, (byte)(i & 7), Alu.Arithmetic.Xor);
                ops[(i * 1) + 0xB0] = new Opcode(7, (byte)(i & 7), Alu.Arithmetic.Or);
                ops[(i * 1) + 0xB8] = new Opcode(7, (byte)(i & 7), Alu.Arithmetic.Cp);
                ops[(i * 8) + 0xC7] = new Opcode((byte)i, 0, Alu.Branching.Rst);
            }

            for (int i = 0; i < 4; i++)
            {
                ops[(i * 0x10) + 0x01] = new Opcode((byte)i, 0, Alu.Loading.LdD16);
                ops[(i * 0x10) + 0x02] = new Opcode((byte)i, 8, Alu.Loading.LdR16);
                ops[(i * 0x10) + 0x03] = new Opcode((byte)i, 0, Alu.Arithmetic.Inc16);
                ops[(i * 0x10) + 0x09] = new Opcode(2, (byte)i, Alu.Arithmetic.AddHl);
                ops[(i * 0x10) + 0x0A] = new Opcode(8, (byte)i, Alu.Loading.LdR16);
                ops[(i * 0x10) + 0x0B] = new Opcode((byte)i, 0, Alu.Arithmetic.Dec16);
                ops[(i * 0x08) + 0x20] = new Opcode((byte)(i & 1), (byte)((i / 2) + 1), Alu.Branching.Jr8);
                ops[(i * 0x08) + 0xC0] = new Opcode((byte)(i & 1), (byte)((i / 2) + 1), Alu.Branching.RetC);
                ops[(i * 0x08) + 0xC2] = new Opcode((byte)(i & 1), (byte)((i / 2) + 1), Alu.Branching.Jp);
                ops[(i * 0x10) + 0xC1] = new Opcode((byte)i, 0, Alu.Loading.Pop);
                ops[(i * 0x08) + 0xC4] = new Opcode((byte)(i & 1), (byte)((i / 2) + 1), Alu.Branching.Call);
                ops[(i * 0x10) + 0xC5] = new Opcode((byte)i, 0, Alu.Loading.Push);
            }

            ops[0x00] = new Opcode(0, 0, (a, b) => 1); // NOP
            ops[0x07] = new Opcode(0, 0, (op, mem) => // RLCA
            {
                mem.R.F = (GbUInt8)(mem.R.A & 0x80) >> (7 - RFlags.CF);
                mem.R.A = (GbUInt8)((mem.R.A << 1) | ((mem.R.F & RFlags.CB) >> RFlags.CF));
                return 1;
            });
            ops[0x08] = new Opcode(0, 0, Alu.Loading.LdA16Sp);
            ops[0x0F] = new Opcode(0, 0, (op, mem) => // RRCA
            {
                mem.R.F = (GbUInt8)((mem.R.A & 1) << RFlags.CF);
                mem.R.A = (GbUInt8)((mem.R.A >> 1) | ((mem.R.F & RFlags.CB) << (7 - RFlags.CF)));
                return 1;
            });
            ops[0x10] = new Opcode(0, 0, (op, mem) => // STOP
            {
                mem.R.Pc++;
                mem.Status = CpuState.STOP;
                return 1;
            });
            ops[0x17] = new Opcode(0, 0, (op, mem) => // RLA
            {
                bool b = mem.R.A[7];
                mem.R.A <<= 1;
                mem.R.A |= (byte)(mem.R.F[RFlags.CF] ? 1 : 0);
                mem.R.F = b ? RFlags.CB : (byte)0;
                return 1;
            });
            ops[0x18] = new Opcode(0, 0, Alu.Branching.Jr8);
            ops[0x1F] = new Opcode(0, 0, (op, mem) => // RRA
            {
                bool oldCf = mem.R.F[RFlags.CF];
                mem.R.F = mem.R.A[0] ? RFlags.CB : (byte)0;
                mem.R.A = (byte)(mem.R.A >> 1 | (oldCf ? 0x80 : 0));
                return 1;
            });
            ops[0x27] = new Opcode(0, 0, Alu.Arithmetic.Daa);
            ops[0x2F] = new Opcode(7, 7, Alu.Arithmetic.Cpl);
            ops[0x37] = new Opcode(0, 0, (op, mem) => // SCF
            {
                mem.R.F = (GbUInt8)(mem.R.F & RFlags.ZB);
                mem.R.F |= RFlags.CB;
                return 1;
            });
            ops[0x3F] = new Opcode(0, 0, (op, mem) => // CCF
            {
                mem.R.F &= RFlags.ZCB;
                mem.R.F ^= RFlags.CB;
                return 1;
            });
            ops[0x76] = new Opcode(0, 0, (op, mem) => // HALT
            {
                if (mem.IME || (mem.IF & mem.IER & 0x1F) == 0)
                {
                    mem.Status = CpuState.HALT;
                }
                else
                {
                    mem.HaltBugged = true;
                }

                return 1;
            });
            ops[0xC3] = new Opcode(0, 0, Alu.Branching.Jp);
            ops[0xC6] = new Opcode(7, 8, Alu.Arithmetic.Add);
            ops[0xC9] = new Opcode(0, 0, Alu.Branching.Ret);
            ops[0xCB] = new Opcode(0, 0, (op, mem) => // CBPrefix
            {
                mem.Update();
                return CbOps[mem.LdI8()].Invoke(mem) + 1;
            });
            ops[0xCD] = new Opcode(0, 0, Alu.Branching.Call);
            ops[0xCE] = new Opcode(7, 8, Alu.Arithmetic.Adc);
            ops[0xD3] = InvalidOpcode;
            ops[0xD6] = new Opcode(7, 8, Alu.Arithmetic.Sub);
            ops[0xD9] = new Opcode(1, 0, Alu.Branching.Ret); //RetI
            ops[0xDB] = InvalidOpcode;
            ops[0xDE] = new Opcode(7, 8, Alu.Arithmetic.Sbc);
            ops[0xE0] = new Opcode(0, 7, Alu.Loading.LdH);
            ops[0xE2] = new Opcode(0, 0, (op, mem) => // LD (C),A
            {
                mem.Update();
                mem.SetMappedMemory((GbUInt16)(0xFF00 + mem.R.C), mem.R.A);
                return 2;
            });
            ops[0xD3] = InvalidOpcode;
            ops[0xE6] = new Opcode(7, 8, Alu.Arithmetic.And);
            ops[0xE8] = new Opcode(0, 0, Alu.Arithmetic.AddSpR8);
            ops[0xE9] = new Opcode(0, 0, (op, mem) => // JP (HL)
            {
                mem.R.Pc = mem.R.Hl;
                return 1;
            });
            ops[0xEA] = new Opcode(0, 7, Alu.Loading.LdA16);
            ops[0xEB] = InvalidOpcode;
            ops[0xEC] = InvalidOpcode;
            ops[0xED] = InvalidOpcode;
            ops[0xEE] = new Opcode(7, 8, Alu.Arithmetic.Xor);
            ops[0xF0] = new Opcode(7, 0, Alu.Loading.LdH);
            ops[0xF2] = new Opcode(0, 0, (op, mem) => // LD A,(C)
            {
                mem.Update();
                mem.R.A = mem.GetMappedMemory((GbUInt16)(mem.R.C + 0xFF00));
                return 1;
            });
            ops[0xF3] = new Opcode(0, 0, (op, mem) => // DI
            {
                mem.IME = false;
                mem.NextIMEValue = false;
                return 1;
            });
            ops[0xF4] = InvalidOpcode;
            ops[0xF6] = new Opcode(7, 8, Alu.Arithmetic.Or);
            ops[0xF8] = new Opcode(0, 0, Alu.Loading.LdHlSpR8);
            ops[0xF9] = new Opcode(0, 0, Alu.Loading.LdSpHl);
            ops[0xFA] = new Opcode(7, 0, Alu.Loading.LdA16);
            ops[0xFB] = new Opcode(0, 0, (op, mem) => // EI
            {
                mem.NextIMEValue = true;
                mem.IME = false;
                return 1;
            });
            ops[0xFC] = InvalidOpcode;
            ops[0xFD] = InvalidOpcode;
            ops[0xFE] = new Opcode(7, 8, Alu.Arithmetic.Cp);
            return ops;
        }
    }
}
