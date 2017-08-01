using System;
using JAGBE.GB.DataTypes;

namespace JAGBE.GB.Computation.Execution.Alu
{
    internal static class Loading
    {
        public static bool Ld8(Opcode op, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (op.Src == 6 || op.Dest == 6)
                {
                    return false;
                }

                if (op.Src == 6 && op.Dest == 6)
                {
                    throw new InvalidOperationException("Opcode with dest = 6 and src = 6 is invalid for this instruction.");
                }

                memory.R.SetR8(op.Dest, memory.R.GetR8(op.Src));
                return true;
            }

            if (step == 1)
            {
                if (op.Src == 6)
                {
                    memory.R.SetR8(op.Dest, memory.GetMappedMemoryHl());
                    return true;
                }

                memory.SetMappedMemory(memory.R.Hl, memory.R.GetR8(op.Src));
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool LdD16(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                mem.R.SetR16(op.Dest, new GbUInt16(mem.R.GetR16(op.Dest, false).HighByte, mem.LdI8()), false);
                return false;
            }

            if (step == 2)
            {
                mem.R.SetR16(op.Dest, new GbUInt16(mem.LdI8(), mem.R.GetR16(op.Dest, false).LowByte), false);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }
    }
}
