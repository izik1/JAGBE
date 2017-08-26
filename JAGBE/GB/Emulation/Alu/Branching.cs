using System;
using JAGBE.GB.DataTypes;

namespace JAGBE.GB.Emulation.Alu
{
    internal static class Branching
    {
        public static bool Call(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                    return false;

                case 1:
                    op.Data1 = mem.LdI8(); // Low byte
                    return false;

                case 2:
                    op.Data2 = mem.LdI8(); // High byte.
                    return false;

                case 3:
                    return op.Src != 0 && GetConditionalJumpState(op.Dest, op.Src, mem.R.F);

                case 4:
                    mem.Push(mem.R.Pc.HighByte);
                    return false;

                case 5:
                    mem.Push(mem.R.Pc.LowByte);
                    mem.R.Pc = new GbUInt16(op.Data2, op.Data1);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool Jp(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                    return false;

                case 1:
                    op.Data1 = mem.LdI8(); // Low byte
                    return false;

                case 2:
                    op.Data2 = mem.LdI8(); // High byte.
                    return op.Src != 0 && GetConditionalJumpState(op.Dest, op.Src, mem.R.F);

                case 3:
                    mem.R.Pc = new GbUInt16(op.Data2, op.Data1);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool Jr8(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                    return false;

                case 1:
                    if (op.Src != 0 && GetConditionalJumpState(op.Dest, op.Src, mem.R.F))
                    {
                        mem.R.Pc++;
                        return true;
                    }

                    return false;

                case 2:
                    mem.R.Pc += (sbyte)(byte)mem.LdI8();
                    mem.R.Pc++;
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool Ret(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                    return false;

                case 1:
                    op.Data1 = mem.Pop(); // Low Byte.
                    return false;

                case 2:
                    op.Data2 = mem.Pop(); // High Byte.
                    return false;

                case 3:
                    mem.R.Pc = new GbUInt16(op.Data2, op.Data1);
                    mem.IME |= op.Dest != 0; // Unlike EI IME gets enabled right away.
                    mem.NextIMEValue = mem.IME;
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
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
            switch (step)
            {
                case 0:
                    return false;

                case 1:
                    return GetConditionalJumpState(op.Dest, op.Src, mem.R.F);

                case 2:
                    op.Data1 = mem.Pop(); // Low Byte.
                    return false;

                case 3:
                    op.Data2 = mem.Pop(); // High Byte.
                    return false;

                case 4:
                    mem.R.Pc = new GbUInt16(op.Data2, op.Data1);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool Rst(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                case 1:
                    return false;

                case 2:
                    mem.Push(mem.R.Pc.HighByte);
                    return false;

                case 3:
                    mem.Push(mem.R.Pc.LowByte);
                    mem.R.Pc = new GbUInt16(0, (byte)(op.Dest * 8));
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        /// <summary>
        /// <see langword="true"/> is shouldn't jump.
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="src">The source.</param>
        /// <param name="f">The flags.</param>
        /// <returns></returns>
        private static bool GetConditionalJumpState(GbUInt8 dest, GbUInt8 src, GbUInt8 f) =>
            f[src == 1 ? RFlags.ZF : RFlags.CF] ^ (dest != 0);
    }
}
