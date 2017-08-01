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

        public static bool Inc(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                if (op.Dest == 6)
                {
                    return false;
                }

                byte b = mem.R.GetR8(op.Dest);
                mem.R.F = mem.R.F.AssignBit(RFlags.Z, b == 255).Res(RFlags.N).AssignBit(RFlags.H, b.GetHFlag((byte)(b + 1)));
                mem.R.SetR8(op.Dest, (byte)(b + 1));
                return true;
            }

            if (step == 1)
            {
                op.Data1 = mem.GetMappedMemoryHl();

                return false;
            }

            if (step == 2)
            {
                mem.R.F = mem.R.F.AssignBit(RFlags.Z, op.Data1 == 255)
                    .Res(RFlags.N).AssignBit(RFlags.H, op.Data1.GetHFlag((byte)(op.Data1 + 1)));
                mem.R.SetR8(op.Dest, (byte)(op.Data1 + 1));

                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }
    }
}
