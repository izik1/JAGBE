using static JAGBE.GB.Emulation.Alu.Ops;

namespace JAGBE.GB.Emulation.Alu
{
    internal static class Arithmetic
    {
        /// <summary>
        /// Adds src and the carry flag to a.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected flags: Z 0 H C</remarks>
        public static int Adc(Opcode op, GbMemory memory) => ArithOp8Func(op, memory, (mem, val) =>
        {
            // https://github.com/eightlittlebits/elbgb/blob/dffc28001a7a01f93ef9e8abecd7161bcf03cc95/elbgb_core/CPU/LR35902.cs#L1038
            // Reimplemented on 8/26/2017 thanks to the previous link.
            int cIn = mem.R.F.GetBit(RFlags.CF) ? 1 : 0;
            int res = val + cIn + mem.R.A;
            mem.R.F = ((res & 0xFF) == 0 ? RFlags.ZB : byte.MinValue).AssignBit(RFlags.HF,
                (mem.R.A & 0x0F) + (val & 0x0F) + cIn > 0x0F).AssignBit(RFlags.CF, res > 0xFF);
            mem.R.A = (byte)res;
        });

        /// <summary>
        /// Adds src to a.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected flags: Z 0 H C</remarks>
        public static int Add(Opcode op, GbMemory memory) => ArithOp8Func(op, memory, (mem, val) =>
        {
            byte s = (byte)(mem.R.A + val);
            mem.R.F = (byte)((s == 0 ? RFlags.ZB : 0) |
            ((((mem.R.A & 0x0F) + (val & 0x0F)) & 0x10) == 0x10 ? RFlags.HB : 0) | (s < mem.R.A ? RFlags.CB : 0));
            mem.R.A = s;
        });

        public static int AddHl(Opcode op, GbMemory mem)
        {
            mem.Update();
            GbUInt16 val = mem.R.GetR16Sp(op.Src);
            mem.R.F = mem.R.F.Res(RFlags.NF).AssignBit(RFlags.HF, (((mem.R.Hl & 0xFFF) + (val & 0xFFF)) & 0x1000) == 0x1000)
                .AssignBit(RFlags.CF, val + mem.R.Hl < mem.R.Hl);
            mem.R.Hl += val;
            return 2;
        }

        /// <summary>
        /// Adds a 8 bit signed value to the Stack Pointer.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected Flags: - - H C</remarks>
        public static int AddSpR8(Opcode op, GbMemory memory)
        {
            sbyte v = (sbyte)memory.ReadCycleI8();
            memory.Update();
            memory.Update();
            memory.R.F = (((memory.R.Sp & 0x0F) + (v & 0x0F)) > 0x0F ? RFlags.HB : byte.MinValue)
                .AssignBit(RFlags.CF, ((memory.R.Sp & 0xFF) + (v & 0xFF)) > 0xFF);
            memory.R.Sp += v;
            return 4;
        }

        /// <summary>
        /// Ands A and src and stores the result into A.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected flags: Z 0 1 0</remarks>
        public static int And(Opcode op, GbMemory memory) => ArithOp8Func(op, memory, (mem, val) =>
        {
            mem.R.A = (byte)(mem.R.A & val);
            mem.R.F = mem.R.A == 0 ? RFlags.ZHB : RFlags.HB;
        });

        /// <summary>
        /// Subracts src from a and discards the result.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected flags: Z 1 H -</remarks>
        public static int Cp(Opcode op, GbMemory memory) => ArithOp8Func(op, memory, (mem, val) =>
        {
            int r = mem.R.A - val;
            mem.R.F = (byte)((r == 0 ? RFlags.ZNB : RFlags.NB) |
            ((mem.R.A & 0xF) < (r & 0xF) ? RFlags.HB : 0) | (r < 0 ? RFlags.CB : 0));
        });

        /// <summary>
        /// Complements the A register of the given <paramref name="memory"/>
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected Flags: Z, C = Unaffected. N,H = 1</remarks>
        public static int Cpl(Opcode op, GbMemory memory) => ArithOp8Func(op, memory, (mem, val) =>
        {
            mem.R.A = (byte)~val;
            mem.R.F |= RFlags.NHB;
        });

        /// <summary>
        /// Preforms BCD conversion.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="mem">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected Flags: Z - 0 C</remarks>
        public static int Daa(Opcode op, GbMemory mem)
        {
            int res = mem.R.A;
            if (mem.R.F.GetBit(RFlags.NF))
            {
                if (mem.R.F.GetBit(RFlags.HF))
                {
                    res = (res - 6) & 0xFF;
                }

                if (mem.R.F.GetBit(RFlags.CF))
                {
                    res -= 0x60;
                }
            }
            else
            {
                if (mem.R.F.GetBit(RFlags.HF) || (res & 0xF) > 9)
                {
                    res += 0x06;
                }

                if (mem.R.F.GetBit(RFlags.CF) || res > 0x9F)
                {
                    res += 0x60;
                }
            }

            mem.R.F = (byte)(mem.R.F & RFlags.NCB);
            if ((res & 0x100) == 0x100)
            {
                mem.R.F |= RFlags.CB;
            }

            res &= 0xFF;

            if (res == 0)
            {
                mem.R.F |= RFlags.ZB;
            }

            mem.R.A = (byte)res;
            return 1;
        }

        /// <summary>
        /// Decrements src.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="mem">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected flags: - - - -</remarks>
        public static int Dec16(Opcode op, GbMemory mem)
        {
            mem.Update();
            mem.R.SetR16(op.Dest, mem.R.GetR16Sp(op.Dest) - 1);
            return 2;
        }

        /// <summary>
        /// Decrements src.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected flags: Z 1 H -</remarks>
        public static int Dec8(Opcode op, GbMemory memory) => BitOpFunc(op, memory, (mem, val) =>
        {
            mem.R.F = (byte)(mem.R.F.AssignBit(RFlags.ZF, val == 1).AssignBit(RFlags.HF, (val & 0xF) == 0) | RFlags.NB);
            return (byte)(val - 1);
        });

        /// <summary>
        /// Increments src.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="mem">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected flags: - - - -</remarks>
        public static int Inc16(Opcode op, GbMemory mem)
        {
            mem.Update();
            mem.R.SetR16(op.Dest, mem.R.GetR16Sp(op.Dest) + (GbUInt16)1);
            return 2;
        }

        /// <summary>
        /// Increments src.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected flags: Z 0 H -</remarks>
        public static int Inc8(Opcode op, GbMemory memory) => BitOpFunc(op, memory, (mem, val) =>
        {
            mem.R.F = mem.R.F.AssignBit(RFlags.ZF, val == 255).Res(RFlags.NF).AssignBit(RFlags.HF, (((val & 0x0F) + 1) & 0x10) == 0x10);
            return (byte)(val + 1);
        });

        /// <summary>
        /// Ors A and src.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected flags: Z 0 0 0</remarks>
        public static int Or(Opcode op, GbMemory memory) => ArithOp8Func(op, memory, (mem, val) =>
        {
            mem.R.A |= val;
            mem.R.F = mem.R.A == 0 ? RFlags.ZB : byte.MinValue;
        });

#pragma warning disable S1067 // Expressions should not be too complex

        /// <summary>
        /// Subtracts src and the carry flag from A
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected flags: Z 1 H C</remarks>
        public static int Sbc(Opcode op, GbMemory memory) => ArithOp8Func(op, memory, (mem, val) =>
        {
            // https://github.com/eightlittlebits/elbgb/blob/dffc28001a7a01f93ef9e8abecd7161bcf03cc95/elbgb_core/CPU/LR35902.cs#L1084
            // Reimplemented on 8/26/2017 thanks to the previous link.
            int cIn = mem.R.F.GetBit(RFlags.CF) ? 1 : 0;
            int res = mem.R.A - val - cIn;
            mem.R.F = (byte)(((res & 0xFF) == 0 ? RFlags.ZNB : RFlags.NB) |
            ((mem.R.A & 0xF) - (val & 0xF) - cIn < 0 ? RFlags.HB : 0) | (res < 0 ? RFlags.CB : 0));
            mem.R.A = (byte)res;
        });

#pragma warning restore S1067 // Expressions should not be too complex

        /// <summary>
        /// Subtracts src from a.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected flags: Z 1 H C</remarks>
        public static int Sub(Opcode op, GbMemory memory) => ArithOp8Func(op, memory, (mem, val) =>
        {
            byte s = (byte)(mem.R.A - val);
            mem.R.F = (byte)((s == 0 ? RFlags.ZNB : RFlags.NB) |
            ((mem.R.A & 0xF) < (val & 0xF) ? RFlags.HB : 0) | (s > mem.R.A ? RFlags.CB : 0));
            mem.R.A = s;
        });

        /// <summary>
        /// Xors A and src.
        /// </summary>
        /// <remarks>Affected flags: Z 0 0 0</remarks>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        public static int Xor(Opcode op, GbMemory memory) => ArithOp8Func(op, memory, (mem, val) =>
        {
            mem.R.A ^= val;
            mem.R.F = mem.R.A == 0 ? RFlags.ZB : byte.MinValue;
        });
    }
}
