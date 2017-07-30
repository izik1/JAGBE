using System;

namespace JAGBE.GB.Computation.Execution
{
    internal static class Alu
    {
        public static bool Bit(Opcode code, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (code.Src == 6)
                {
                    return false;
                }

                memory.R.F.AssignBit(RFlags.Z, memory.R.GetR8(code.Src).GetBit(code.Dest));
                memory.R.F.AssignBit(RFlags.N, false);
                memory.R.F.AssignBit(RFlags.H, true);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemory(memory.R.Hl);
            }

            if (step == 2)
            {
                memory.R.F.AssignBit(RFlags.Z, code.Data1.GetBit(code.Dest));
                memory.R.F.AssignBit(RFlags.N, false);
                memory.R.F.AssignBit(RFlags.H, true);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }
    }
}
