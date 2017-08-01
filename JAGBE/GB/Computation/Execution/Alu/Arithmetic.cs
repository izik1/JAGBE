using System;

namespace JAGBE.GB.Computation.Execution.Alu
{
    internal static class Arithmetic
    {
        public static bool Xor(Opcode op, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (op.Src == 6)
                {
                    return false;
                }

                memory.R.A ^= memory.R.GetR8(op.Src);
                memory.R.F = memory.R.A == 0 ? RFlags.ZB : (byte)0;
                return true;
            }

            if (step == 1)
            {
                memory.R.A ^= memory.GetMappedMemoryHl();
                memory.R.F = memory.R.A == 0 ? RFlags.ZB : (byte)0;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }
    }
}
