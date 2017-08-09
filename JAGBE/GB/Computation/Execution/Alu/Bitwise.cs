using System;

namespace JAGBE.GB.Computation.Execution.Alu
{
    internal static class Bitwise
    {
        private delegate byte Op(GbMemory mem, byte valIn, byte dest);

        public static bool Bit(Opcode code, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (code.Src == 6)
                {
                    return false;
                }

                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, !memory.R.GetR8(code.Src).GetBit(code.Dest)).Res(RFlags.NF).Set(RFlags.HF);
                return true;
            }

            if (step == 1)
            {
                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, !memory.GetMappedMemoryHl().GetBit(code.Dest)).Res(RFlags.NF).Set(RFlags.HF);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Res(Opcode code, GbMemory memory, int step)
        {
            switch (step)
            {
                case 0:
                    if (code.Src == 6)
                    {
                        return false;
                    }

                    memory.R.SetR8(code.Src, memory.R.GetR8(code.Src).Res(code.Dest));
                    return true;

                case 1:
                    code.Data1 = memory.GetMappedMemoryHl();
                    return false;

                case 2:
                    memory.SetMappedMemoryHl(code.Data1.Res(code.Dest));
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool Rl(Opcode code, GbMemory memory, int step)
        {
            bool carryIn;

            switch (step)
            {
                case 0:
                    if (code.Src == 6)
                    {
                        return false;
                    }
                    carryIn = memory.R.F.GetBit(RFlags.CF);
                    byte b = memory.R.GetR8(code.Src);
                    memory.R.SetR8(code.Src, (byte)((b << 1) | (carryIn ? 1 : 0)));
                    memory.R.F = (b.GetBit(7) ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, memory.R.GetR8(code.Src) == 0);
                    return true;

                case 1:
                    code.Data1 = memory.GetMappedMemoryHl();
                    return false;

                case 2:
                    carryIn = memory.R.F.GetBit(RFlags.CF);
                    memory.SetMappedMemoryHl((byte)((code.Data1 << 1) | (carryIn ? 1 : 0)));
                    memory.R.F = (code.Data1.GetBit(7) ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, memory.GetMappedMemoryHl() == 0);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool Rlc(Opcode code, GbMemory memory, int step)
        {
            switch (step)
            {
                case 0:
                    if (code.Src == 6)
                    {
                        return false;
                    }

                    byte b = memory.R.GetR8(code.Src);
                    memory.R.F = b.GetBit(7) ? RFlags.CB : (byte)0;
                    b <<= 1;
                    memory.R.SetR8(code.Src, b);
                    memory.R.F = memory.R.F.AssignBit(RFlags.ZF, b == 0);
                    return true;

                case 1:
                    code.Data1 = memory.GetMappedMemoryHl();
                    return false;

                case 2:
                    memory.R.F = code.Data1.GetBit(7) ? RFlags.CB : (byte)0;
                    code.Data1 <<= 1;
                    memory.SetMappedMemoryHl(code.Data1);
                    memory.R.F = memory.R.F.AssignBit(RFlags.ZF, code.Data1 == 0);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool Rr(Opcode code, GbMemory memory, int step)
        {
            bool carryIn;
            switch (step)
            {
                case 0:
                    if (code.Src == 6)
                    {
                        return false;
                    }

                    carryIn = memory.R.F.GetBit(RFlags.CF);
                    byte b = memory.R.GetR8(code.Src);
                    memory.R.F = b.GetBit(0) ? RFlags.CB : (byte)0;
                    b = (byte)((b >> 1) | (carryIn ? 0x80 : 0));
                    memory.R.SetR8(code.Src, b);
                    memory.R.F = memory.R.F.AssignBit(RFlags.ZF, b == 0);
                    return true;

                case 1:
                    code.Data1 = memory.GetMappedMemoryHl();
                    return false;

                case 2:
                    carryIn = memory.R.F.GetBit(RFlags.CF);
                    memory.R.F = code.Data1.GetBit(0) ? RFlags.CB : (byte)0;
                    code.Data1 = (byte)((code.Data1 >> 1) | (carryIn ? 0x80 : 0));
                    memory.SetMappedMemoryHl(code.Data1);
                    memory.R.F = memory.R.F.AssignBit(RFlags.ZF, code.Data1 == 0);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Rrc(Opcode code, GbMemory memory, int step)
        {
            switch (step)
            {
                case 0:
                    if (code.Src == 6)
                    {
                        return false;
                    }

                    byte b = memory.R.GetR8(code.Src);

                    memory.R.F = b.GetBit(0) ? RFlags.CF : (byte)0;

                    memory.R.SetR8(code.Src, (byte)((b >> 1) | (b.GetBit(0) ? 1 : 0)));
                    memory.R.F = memory.R.F.AssignBit(RFlags.ZF, b == 0);
                    return true;

                case 1:
                    code.Data1 = memory.GetMappedMemoryHl();
                    return false;

                case 2:
                    memory.R.F = code.Data1.GetBit(0) ? RFlags.CF : (byte)0;
                    memory.SetMappedMemoryHl((byte)((code.Data1 >> 1) | (code.Data1.GetBit(0) ? 1 : 0)));
                    memory.R.F = memory.R.F.AssignBit(RFlags.ZF, code.Data1 == 0);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        /// <summary>
        /// Sets the src bit of the dest register to 1
        /// </summary>
        /// <remarks>Affected Flags: None.</remarks>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns>
        /// <see langword="true"/> if the operation is complete, <see langword="false"/> otherwise
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">step</exception>
        public static bool Set(Opcode code, GbMemory memory, int step)
        {
            switch (step)
            {
                case 0:
                    if (code.Src == 6)
                    {
                        return false;
                    }

                    memory.R.SetR8(code.Src, memory.R.GetR8(code.Src).Set(code.Dest));
                    return true;

                case 1:
                    code.Data1 = memory.GetMappedMemoryHl();
                    return false;

                case 2:
                    memory.SetMappedMemoryHl(code.Data1.Set(code.Dest));
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        /// <summary>
        /// Shift left, bit 7 gets shifted into Carry.
        /// </summary>
        /// <remarks>
        /// Flags affected: Z is set if the result of the operation is zero. Reset N,H. C = bit 7 of
        /// The original number
        /// </remarks>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns>
        /// The state of the operation <see langword="true"/> if complete, <see langword="false"/> otherwise
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">step</exception>
        public static bool Sla(Opcode code, GbMemory memory, int step)
        {
            switch (step)
            {
                case 0:
                    if (code.Src == 6)
                    {
                        return false;
                    }

                    byte b = memory.R.GetR8(code.Src);

                    memory.R.F = b.GetBit(7) ? RFlags.CB : (byte)0;
                    b <<= 1;
                    memory.R.SetR8(code.Src, b);
                    memory.R.F = memory.R.F.AssignBit(RFlags.ZF, b == 0);
                    return true;

                case 1:
                    code.Data1 = memory.GetMappedMemoryHl();
                    return false;

                case 2:
                    memory.R.F = code.Data1.GetBit(7) ? RFlags.CB : (byte)0;
                    code.Data1 <<= 1;
                    memory.SetMappedMemoryHl(code.Data1);
                    memory.R.F = memory.R.F.AssignBit(RFlags.ZF, code.Data1 == 0);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool Sra(Opcode code, GbMemory memory, int step)
        {
            switch (step)
            {
                case 0:
                    if (code.Src == 6)
                    {
                        return false;
                    }

                    byte b = memory.R.GetR8(code.Src);

                    memory.R.F = b.GetBit(0) ? RFlags.CB : (byte)0;
                    memory.R.SetR8(code.Src, (byte)((b >> 1) | (b & (1 << 7))));
                    memory.R.F = memory.R.F.AssignBit(RFlags.ZF, b == 0);
                    return true;

                case 1:
                    code.Data1 = memory.GetMappedMemoryHl();
                    return false;

                case 2:
                    memory.R.F = code.Data1.GetBit(0) ? RFlags.CB : (byte)0;
                    memory.SetMappedMemoryHl((byte)((code.Data1 << 1) | (code.Data1 & (1 << 7))));
                    memory.R.F = memory.R.F.AssignBit(RFlags.ZF, code.Data1 == 0);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool Srl(Opcode code, GbMemory memory, int step)
        {
            switch (step)
            {
                case 0:
                    if (code.Src == 6)
                    {
                        return false;
                    }

                    byte b = memory.R.GetR8(code.Src);

                    memory.R.F = b.GetBit(0) ? RFlags.CB : (byte)0;
                    memory.R.SetR8(code.Src, (byte)(b >> 1));
                    memory.R.F = memory.R.F.AssignBit(RFlags.ZF, b == 0);
                    return true;

                case 1:
                    code.Data1 = memory.GetMappedMemoryHl();
                    return false;

                case 2:
                    memory.R.F = code.Data1.GetBit(0) ? RFlags.CB : (byte)0;
                    memory.SetMappedMemoryHl((byte)(code.Data1 >> 1));
                    memory.R.F = memory.R.F.AssignBit(RFlags.ZF, code.Data1 == 0);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool Swap(Opcode code, GbMemory memory, int step)
        {
            switch (step)
            {
                case 0:
                    if (code.Src == 6)
                    {
                        return false;
                    }

                    byte b = memory.R.GetR8(code.Src);
                    memory.R.SetR8(code.Src, (byte)((b << 4) | (b >> 4)));
                    memory.R.F = b == 0 ? RFlags.ZB : (byte)0;
                    return true;

                case 1:
                    code.Data1 = memory.GetMappedMemoryHl();
                    return false;

                case 2:
                    memory.SetMappedMemoryHl((byte)((code.Data1 << 4) | (code.Data1 >> 4)));
                    memory.R.F = code.Data1 == 0 ? RFlags.ZB : (byte)0;
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        private static bool Operate(Opcode op, GbMemory memory, int step, Op operation)
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
    }
}
