using System;
using JAGBE.Logging;

namespace JAGBE.GB.Emulation
{
    internal sealed class Instruction
    {
        private static readonly Opcode[] CbOps = GetCbOps();

        private static readonly Opcode InvalidOpcode = new Opcode(0, 0, (o, m, s) =>
        {
            m.Status = CpuState.HUNG;
            return true;
        });

        private static readonly Opcode[] NmOps = GetNmOps();
        private byte opcode;

        public Instruction(GbUInt8 opcode) => this.opcode = (byte)opcode;

        public bool Run(GbMemory memory, int step) => NmOps[this.opcode].Invoke(memory, step);

        private static bool CbPrefix(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                mem.instrData1 = mem.LdI8();
            }

            return CbOps[mem.instrData1].Invoke(mem, step - 1);
        }

        private static Opcode[] GetCbOps()
        {
            Opcode[] ops = new Opcode[0x100];

            for (int i = 0; i < 0x100; i++)
            {
                ops[i] = new Opcode((byte)i, 1, Unimplemented);
            }

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

        private static Opcode[] GetNmOps()
        {
            Opcode[] ops = new Opcode[0x100];

            // Keep as a fallback instead of NullRefrenceExceptions, which are much harder to debug.
            for (int i = 0; i < 0x100; i++)
            {
                ops[i] = new Opcode((byte)i, 0, Unimplemented);
            }

            for (int i = 0; i < 0x40; i++)
            {
                if (i != 0x36)
                {
                    ops[i + 0x40] = new Opcode((byte)((i >> 3) & 7), (byte)(i & 7), Alu.Loading.Ld8);
                }
                else
                {
                    // Put Stop instruction here.
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

            ops[0x00] = new Opcode(0, 0, (a, b, c) => true); // NOP
            ops[0x07] = new Opcode(0, 0, (op, mem, step) => // RLCA
            {
                mem.R.F = mem.R.A[7] ? RFlags.CB : (byte)0;
                mem.R.A <<= 1;
                if (mem.R.F[RFlags.CF])
                {
                    mem.R.A |= 0x01;
                }

                return true;
            });
            ops[0x08] = new Opcode(0, 0, Alu.Loading.LdA16Sp);
            ops[0x0F] = new Opcode(0, 0, (op, mem, step) => // RRCA
            {
                mem.R.F = mem.R.A[0] ? RFlags.CB : (byte)0;
                mem.R.A >>= 1;
                if (mem.R.F[RFlags.CF])
                {
                    mem.R.A |= 0x80;
                }

                return true;
            });
            ops[0x10] = new Opcode(0, 0, (op, mem, step) =>
             {
                 mem.R.Pc++;
                 mem.Status = CpuState.STOP;
                 return true;
             });
            ops[0x17] = new Opcode(0, 0, (op, mem, step) =>
            {
                bool b = mem.R.A[7];
                mem.R.A <<= 1;
                mem.R.A |= (byte)(mem.R.F[RFlags.CF] ? 1 : 0);
                mem.R.F = b ? RFlags.CB : (byte)0;
                return true;
            });
            ops[0x18] = new Opcode(0, 0, Alu.Branching.Jr8);
            ops[0x1F] = new Opcode(0, 0, (op, mem, step) => // RRA
            {
                bool oldCf = mem.R.F[RFlags.CF];
                mem.R.F = mem.R.A[0] ? RFlags.CB : (byte)0;
                mem.R.A = (byte)(mem.R.A >> 1 | (oldCf ? 0x80 : 0));
                return true;
            });
            ops[0x27] = new Opcode(0, 0, Alu.Arithmetic.Daa);
            ops[0x2F] = new Opcode(7, 7, Alu.Arithmetic.Cpl);
            ops[0x37] = new Opcode(0, 0, (op, mem, step) => // SCF
            {
                if (step == 0)
                {
                    mem.R.F &= RFlags.ZB;
                    mem.R.F |= RFlags.CB;
                    return true;
                }

                throw new ArgumentOutOfRangeException(nameof(step));
            });
            ops[0x3F] = new Opcode(0, 0, (op, mem, step) => // CCF
            {
                if (step == 0)
                {
                    mem.R.F &= RFlags.ZCB;
                    mem.R.F ^= RFlags.CB;
                    return true;
                }

                throw new ArgumentOutOfRangeException(nameof(step));
            });

            ops[0xC3] = new Opcode(0, 0, Alu.Branching.Jp);
            ops[0xC6] = new Opcode(7, 8, Alu.Arithmetic.Add);
            ops[0xC9] = new Opcode(0, 0, Alu.Branching.Ret);
            ops[0xCB] = new Opcode(0, 0, CbPrefix);
            ops[0xCD] = new Opcode(0, 0, Alu.Branching.Call);
            ops[0xCE] = new Opcode(7, 8, Alu.Arithmetic.Adc);
            ops[0xD3] = InvalidOpcode;
            ops[0xD6] = new Opcode(7, 8, Alu.Arithmetic.Sub);
            ops[0xD9] = new Opcode(1, 0, Alu.Branching.Ret); //RetI
            ops[0xDB] = InvalidOpcode;
            ops[0xDE] = new Opcode(7, 8, Alu.Arithmetic.Sbc);
            ops[0xE0] = new Opcode(0, 7, Alu.Loading.LdH);
            ops[0xE2] = new Opcode(0, 0, (op, mem, step) =>
            {
                if (step == 0)
                {
                    return false;
                }

                if (step == 1)
                {
                    mem.SetMappedMemory((GbUInt16)(0xFF00 + mem.R.C), mem.R.A);
                    return true;
                }

                throw new ArgumentOutOfRangeException(nameof(step));
            });
            ops[0xD3] = InvalidOpcode;
            ops[0xE6] = new Opcode(7, 8, Alu.Arithmetic.And);
            ops[0xE8] = new Opcode(0, 0, Alu.Arithmetic.AddSpR8);
            ops[0xE9] = new Opcode(0, 0, (op, mem, step) =>
            {
                if (step == 0)
                {
                    mem.R.Pc = mem.R.Hl;
                    return true;
                }

                throw new ArgumentOutOfRangeException(nameof(step));
            });
            ops[0xEA] = new Opcode(0, 7, Alu.Loading.LdA16);
            ops[0xEB] = InvalidOpcode;
            ops[0xEC] = InvalidOpcode;
            ops[0xED] = InvalidOpcode;
            ops[0xEE] = new Opcode(7, 8, Alu.Arithmetic.Xor);
            ops[0xF0] = new Opcode(7, 0, Alu.Loading.LdH);
            ops[0xF2] = new Opcode(0, 0, (op, mem, step) =>
            {
                if (step == 0)
                {
                    return false;
                }

                if (step == 1)
                {
                    mem.R.A = mem.GetMappedMemory((GbUInt16)(mem.R.C + 0xFF00));
                    return true;
                }

                throw new ArgumentOutOfRangeException(nameof(step));
            });
            ops[0xF3] = new Opcode(0, 0, (op, mem, s) => // DI
            {
                mem.IME = false;
                mem.NextIMEValue = false;
                return true;
            });
            ops[0xF4] = InvalidOpcode;
            ops[0xF6] = new Opcode(7, 8, Alu.Arithmetic.Or);
            ops[0xF8] = new Opcode(0, 0, Alu.Loading.LdHlSpR8);
            ops[0xF9] = new Opcode(0, 0, Alu.Loading.LdSpHl);
            ops[0xFA] = new Opcode(7, 0, Alu.Loading.LdA16);
            ops[0xFB] = new Opcode(0, 0, (op, mem, s) => // EI
            {
                mem.NextIMEValue = true;
                mem.IME = false;
                return true;
            });
            ops[0xFC] = InvalidOpcode;
            ops[0xFD] = InvalidOpcode;
            ops[0xFE] = new Opcode(7, 8, Alu.Arithmetic.Cp);
            return ops;
        }

        private static bool Unimplemented(Opcode o, GbMemory mem, int step)
        {
            Logger.LogError("Unimplemented opcode 0x" + (o.Src > 0 ? "CB" : "") + o.Dest.ToString("X2"));
            mem.Status = CpuState.ERROR;
            return true;
        }
    }
}
