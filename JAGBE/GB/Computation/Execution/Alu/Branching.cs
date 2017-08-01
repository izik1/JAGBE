using System;

namespace JAGBE.GB.Computation.Execution.Alu
{
    internal static class Branching
    {
        public static bool Jr8(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                if (op.Src > 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(op));
                }

                return false;
            }

            if (step == 1)
            {
                if (op.Src == 0)
                {
                    return false;
                }

                if (mem.R.F.GetBit(op.Src == 1 ? RFlags.Z : RFlags.C) ^ (op.Dest == 0))
                {
                    mem.R.Pc++;
                    return true;
                }

                return false;
            }

            if (step == 2)
            {
                mem.R.Pc += (sbyte)mem.LdI8();
                mem.R.Pc++;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }
    }
}
