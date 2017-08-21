using System;
using JAGBE.GB.DataTypes;

namespace JAGBE.GB.Computation.Execution.Alu
{
    internal static class Ops
    {
        internal delegate byte BitOp(GbMemory mem, GbUInt8 valIn, GbUInt8 dest);

        internal delegate void ArithOp8(GbMemory mem, GbUInt8 valIn);

        internal static bool BitOpFunc(Opcode op, GbMemory memory, int step, BitOp operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            switch (step)
            {
                case 0:
                    if (op.Src == 6)
                    {
                        return false;
                    }

                    memory.R.SetR8(op.Src, operation(memory, memory.R.GetR8(op.Src), op.Dest));
                    return true;

                case 1:
                    op.Data1 = memory.GetMappedMemoryHl();
                    return false;

                case 2:
                    memory.SetMappedMemoryHl(operation(memory, op.Data1, op.Dest));
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        internal static bool ArithOp8Func(Opcode op, GbMemory memory, int step, ArithOp8 operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (step == 0)
            {
                if (op.Src == 6 || op.Src == 8)
                {
                    return false;
                }

                operation(memory, memory.R.GetR8(op.Src));
                return true;
            }

            if (step == 1)
            {
                operation(memory, op.Src == 6 ? memory.GetMappedMemoryHl() : memory.LdI8());
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }
    }
}
