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

                if (op.Dest == 6)
                {
                    memory.SetMappedMemory(memory.R.Hl, memory.R.GetR8(op.Src));
                    return true;
                }

                // Should never throw.
                throw new InvalidOperationException();
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool LdD8(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                op.Data1 = mem.LdI8();
                if (op.Dest == 6)
                {
                    return false;
                }

                mem.R.SetR8(op.Dest, op.Data1);
                return true;
            }

            if (step == 2)
            {
                mem.R.SetR8(op.Dest, op.Data1);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool LdH(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                op.Data1 = mem.LdI8();
                return false;
            }

            if (step == 2)
            {
                if (op.Dest == 7)
                {
                    mem.R.A = mem.GetMappedMemory((ushort)(0xFF00 + op.Data1));
                }
                else
                {
                    mem.SetMappedMemory((ushort)(0xFF00 + op.Data1), mem.R.A);
                }
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool LdR(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                if (op.Dest > 0)
                {
                    mem.SetMappedMemory(mem.R.GetR16(op.Dest, false), mem.R.A);
                }

                if (op.Src > 0)
                {
                    mem.R.A = mem.GetMappedMemory(mem.R.GetR16(op.Src, false));
                }

                if (op.Src == 3 || op.Dest == 3)
                {
                    mem.R.Hl++;
                }
                else if (op.Src == 4 || op.Dest == 4)
                {
                    mem.R.Hl--;
                }
                else
                {
                    // Do nothing.
                }
                return true;
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
