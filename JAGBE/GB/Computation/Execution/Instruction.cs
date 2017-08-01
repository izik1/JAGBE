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

            for (int i = 0; i < 4; i++)
            {
                ops[(i * 0x10) + 0x01] = new Opcode((byte)i, 0, Alu.Loading.LdD16);
            }

            ops[0] = new Opcode(0, 0, (a, b, c) => true); // NOP
            ops[0xCB] = new Opcode(0, 0, CbPrefix);
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

        private static bool Unimplemented(Opcode o, GbMemory mem, int step)
        {
            Console.WriteLine("Unimplemented opcode 0x" + (o.Src > 0 ? "CB" : "") + o.Dest.ToString("X2"));
            mem.Status = CpuState.ERROR;
            return true;
        }
    }
}
