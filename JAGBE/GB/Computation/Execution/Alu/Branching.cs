using System;
using JAGBE.GB.DataTypes;

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

        public static bool Call(Opcode op, GbMemory mem, int step)
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
                // Low byte
                op.Data1 = mem.LdI8();
                return false;
            }

            if (step == 2)
            {
                // High byte.
                op.Data2 = mem.LdI8();
                return false;
            }

            if (step == 3)
            {
                return op.Src != 0 && (mem.R.F.GetBit(op.Src == 1 ? RFlags.Z : RFlags.C) ^ (op.Dest == 0));
            }

            if (step == 4)
            {
                mem.Push(mem.R.Pc.HighByte);
                return false;
            }

            if (step == 5)
            {
                mem.Push(mem.R.Pc.LowByte);
                mem.R.Pc = new GbUInt16(op.Data2, op.Data1);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        /// <summary>
        /// Conditional Return.
        /// </summary>
        /// <param name="op">The opcode.</param>
        /// <param name="mem">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">step</exception>
        public static bool RetC(Opcode op, GbMemory mem, int step)
        {
            if (step == 0 || step == 1)
            {
                return false;
            }

            if (step == 2)
            {
                if (!mem.R.F.GetBit(op.Src == 1 ? RFlags.Z : RFlags.C) ^ (op.Dest == 0))
                {
                    return true;
                }

                // Low Byte.
                op.Data1 = mem.Pop();
                return false;
            }

            if (step == 3)
            {
                // High Byte.
                op.Data2 = mem.Pop();
                return true;
            }

            if (step == 4)
            {
                mem.R.Pc = new GbUInt16(op.Data2, op.Data1);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Ret(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                // Low Byte.
                op.Data1 = mem.Pop();
                return false;
            }

            if (step == 2)
            {
                // High Byte.
                op.Data2 = mem.Pop();
                return true;
            }

            if (step == 3)
            {
                mem.R.Pc = new GbUInt16(op.Data2, op.Data1);

                // Unlike EI IME gets right away.
                mem.IME |= op.Dest != 0;
                mem.NextIMEValue = mem.IME;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }
    }
}
