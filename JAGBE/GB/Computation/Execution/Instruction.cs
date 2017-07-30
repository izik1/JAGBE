using System;

namespace JAGBE.GB.Computation
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

            return ops;
        }

        private static Opcode[] GetNmOps()
        {
            Opcode[] ops = new Opcode[0x100];
            for (int i = 0; i < 0x100; i++)
            {
                ops[i] = new Opcode((byte)i, 0, Unimplemented);
            }

            // Put opcodes here.

            return ops;
        }

        private static bool Unimplemented(Opcode o, GbMemory mem, int step)
        {
            Console.WriteLine("Unimplemented opcode 0x" + (o.Src > 0 ? "CB" : "") + o.Dest.ToString("X2"));
            mem.Status = CpuState.ERROR;
            return true;
        }
    }
}
