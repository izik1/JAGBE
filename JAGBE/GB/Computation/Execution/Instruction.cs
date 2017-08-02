using System;

namespace JAGBE.GB.Computation.Execution
{
    internal sealed class Instruction
    {
        private static readonly Opcode[] CbOps = GetCbOps();
        private static readonly Opcode[] NmOps = GetNmOps();
        private byte opcode;

        public Instruction(byte opcode) => this.opcode = opcode;

        public bool Run(GbMemory memory, int step) => NmOps[this.opcode].Invoke(memory, step);

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
                ops[(i * 8) + 0x04] = new Opcode((byte)i, 0, Alu.Arithmetic.Inc);
                ops[(i * 8) + 0x06] = new Opcode((byte)i, 0, Alu.Loading.LdD8);
                ops[(i * 1) + 0xA8] = new Opcode(0, (byte)(i & 7), Alu.Arithmetic.Xor);
            }

            for (int i = 0; i < 4; i++)
            {
                ops[(i * 0x10) + 0x01] = new Opcode((byte)i, 0, Alu.Loading.LdD16);

                // How to get the right arguments on both instances of this instruction?
                ops[(i * 0x10) + 0x02] = new Opcode((byte)i, 0, Alu.Loading.LdR);
                ops[(i * 0x10) + 0x0A] = new Opcode(0, (byte)i, Alu.Loading.LdR);

                ops[(i * 0x08) + 0x20] = new Opcode((byte)(i & 1), (byte)((i / 2) + 1), Alu.Branching.Jr8);
                ops[(i * 0x10) + 0xC1] = new Opcode((byte)i, 0, Alu.Loading.Pop);
                ops[(i * 0x08) + 0xC4] = new Opcode((byte)(i & 1), (byte)((i / 2) + 1), Alu.Branching.Call);
                ops[(i * 0x10) + 0xC5] = new Opcode((byte)i, 0, Alu.Loading.Push);
            }

            ops[0x00] = new Opcode(0, 0, (a, b, c) => true); // NOP
            ops[0x17] = new Opcode(0, 0, (op, mem, step) =>
            {
                bool b = mem.R.A.GetBit(7);
                mem.R.A <<= 1;
                mem.R.A |= (byte)(mem.R.F.GetBit(RFlags.C) ? 1 : 0);
                mem.R.F = ((byte)0).AssignBit(RFlags.C, b);

                return true;
            });
            ops[0x18] = new Opcode(0, 0, Alu.Branching.Jr8);

            ops[0xCB] = new Opcode(0, 0, CbPrefix);

            ops[0xCD] = new Opcode(0, 0, Alu.Branching.Call);

            ops[0xE0] = new Opcode(0, 7, Alu.Loading.LdH);

            // LD (C), A (FIXME)
            ops[0xE2] = new Opcode(0, 0, (op, mem, step) =>
            {
                if (step == 0)
                {
                    return false;
                }

                if (step == 1)
                {
                    mem.SetMappedMemory((ushort)(0xFF00 + mem.R.C), mem.R.A);
                    return true;
                }

                throw new ArgumentOutOfRangeException(nameof(step));
            });
            ops[0xF0] = new Opcode(7, 0, Alu.Loading.LdH);

            return ops;
        }

        private static bool CbPrefix(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                op.Data1 = mem.LdI8();
            }

            return CbOps[op.Data1].Invoke(mem, step - 1);
        }

        internal static void WriteAllUnimplementedNmOpcodes()
        {
            GbMemory m = new GbMemory();
            for (int i = 0; i < 0x100; i++)
            {
                NmOps[i].Invoke(m, 0);
            }
        }

        private static bool Unimplemented(Opcode o, GbMemory mem, int step)
        {
            Console.WriteLine("Unimplemented opcode 0x" + (o.Src > 0 ? "CB" : "") + o.Dest.ToString("X2"));
            mem.Status = CpuState.ERROR;
            return true;
        }
    }
}
