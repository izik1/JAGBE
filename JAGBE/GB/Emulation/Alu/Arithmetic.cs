using System;
using static JAGBE.GB.Emulation.Alu.Ops;

namespace JAGBE.GB.Emulation.Alu
{
    internal static class Arithmetic
    {
        /// <summary>
        /// Adds src and the carry flag to a.
        /// </summary>
        /// <remarks>Affected flags: Z 0 H C</remarks>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        public static bool Adc(Opcode op, GbMemory memory, int step) => ArithOp8Func(op, memory, step, (mem, val) =>
        {
            // https://github.com/eightlittlebits/elbgb/blob/dffc28001a7a01f93ef9e8abecd7161bcf03cc95/elbgb_core/CPU/LR35902.cs#L1038
            // Reimplemented on 8/26/2017 thanks to the previous link.
            int cIn = mem.R.F[RFlags.CF] ? 1 : 0;
            int res = val + cIn + mem.R.A;
            mem.R.F = ((res & 0xFF) == 0 ? RFlags.ZB : (GbUInt8)0).AssignBit(RFlags.HF,
                (mem.R.A & 0x0F) + (val & 0x0F) + cIn > 0x0F).AssignBit(RFlags.CF, res > 0xFF);
            mem.R.A = (GbUInt8)res;
        });

        /// <summary>
        /// Adds src to a.
        /// </summary>
        /// <remarks>Affected flags: Z 0 H C</remarks>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        public static bool Add(Opcode op, GbMemory memory, int step) => ArithOp8Func(op, memory, step, (mem, val) =>
        {
            byte s = (byte)(mem.R.A + val);
            mem.R.F = (s == 0 ? RFlags.ZB : (byte)0).AssignBit(RFlags.HF, mem.R.A.GetHCarry(val)).AssignBit(RFlags.CF, s < mem.R.A);
            mem.R.A = s;
        });

        public static bool AddHl(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                GbUInt16 val = mem.R.GetR16(op.Src, false);
                mem.R.F = mem.R.F.Res(RFlags.NF).AssignBit(RFlags.HF, (((mem.R.Hl & 0xFFF) + (val & 0xFFF)) & 0x1000) == 0x1000)
                    .AssignBit(RFlags.CF, val + mem.R.Hl < mem.R.Hl);
                mem.R.Hl += val;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        /// <summary>
        /// Adds a 8 bit signed value to the Stack Pointer.
        /// </summary>
        /// <remarks>Affected Flags: - - H C</remarks>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">step</exception>
        public static bool AddSpR8(Opcode op, GbMemory memory, int step)
        {
            switch (step)
            {
                case 0:
                case 2:
                    return false;

                case 1:
                    op.Data1 = memory.LdI8();
                    return false;

                case 3:
                    sbyte v = (sbyte)op.Data1;
                    memory.R.F = (((memory.R.Sp & 0x0F) + (v & 0x0F)) > 0x0F ? RFlags.HB : (GbUInt8)0)
                        .AssignBit(RFlags.CF, ((memory.R.Sp & 0xFF) + (v & 0xFF)) > 0xFF);
                    memory.R.Sp += v;
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        /// <summary>
        /// Ands A and src and stores the result into A.
        /// </summary>
        /// <remarks>Affected flags: Z 0 1 0</remarks>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        public static bool And(Opcode op, GbMemory memory, int step) => ArithOp8Func(op, memory, step, (mem, val) =>
        {
            mem.R.A &= val;
            mem.R.F = mem.R.A == 0 ? RFlags.ZHB : RFlags.HB;
        });

        /// <summary>
        /// Subracts src from a and discards the result.
        /// </summary>
        /// <remarks>Affected flags: Z 1 H -</remarks>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        public static bool Cp(Opcode op, GbMemory memory, int step) => ArithOp8Func(op, memory, step, (mem, val) =>
        {
            byte r = (byte)(mem.R.A - val);
            mem.R.F = RFlags.NB.AssignBit(RFlags.ZF, r == 0).AssignBit(RFlags.HF, mem.R.A.GetHFlagN(r)).AssignBit(RFlags.CF, r > mem.R.A);
        });

        /// <summary>
        /// Complements the A register of the given <paramref name="memory"/>
        /// </summary>
        /// <remarks>Affected Flags: Z, C = Unaffected. N,H = 1</remarks>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        public static bool Cpl(Opcode op, GbMemory memory, int step) => ArithOp8Func(op, memory, step, (mem, val) =>
        {
            mem.R.A = ~val;
            mem.R.F |= RFlags.NHB;
        });

        /// <summary>
        /// Preforms BCD conversion.
        /// </summary>
        /// <remarks>Affected Flags: Z - 0 C</remarks>
        /// <param name="op">The op.</param>
        /// <param name="mem">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">step</exception>
        public static bool Daa(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                int res = mem.R.A;
                if (mem.R.F[RFlags.NF])
                {
                    if (mem.R.F[RFlags.HF])
                    {
                        res = (res - 6) & 0xFF;
                    }

                    if (mem.R.F[RFlags.CF])
                    {
                        res -= 0x60;
                    }
                }
                else
                {
                    if (mem.R.F[RFlags.HF] || (res & 0xF) > 9)
                    {
                        res += 0x06;
                    }

                    if (mem.R.F[RFlags.CF] || res > 0x9F)
                    {
                        res += 0x60;
                    }
                }
                mem.R.F &= RFlags.NCB;

                if ((res & 0x100) == 0x100)
                {
                    mem.R.F |= RFlags.CB;
                }

                res &= 0xFF;

                if (res == 0)
                {
                    mem.R.F |= RFlags.ZB;
                }

                mem.R.A = (GbUInt8)res;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        /// <summary>
        /// Decrements src.
        /// </summary>
        /// <remarks>Affected flags: - - - -</remarks>
        /// <param name="op">The op.</param>
        /// <param name="mem">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">step != 0 &amp;&amp; step != 1</exception>
        public static bool Dec16(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                mem.R.SetR16(op.Dest, mem.R.GetR16(op.Dest, false) - 1, false);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        /// <summary>
        /// Decrements src.
        /// </summary>
        /// <remarks>Affected flags: Z 1 H -</remarks>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        public static bool Dec8(Opcode op, GbMemory memory, int step) => BitOpFunc(op, memory, step, (mem, val, dest) =>
        {
            mem.R.F = mem.R.F.AssignBit(RFlags.ZF, val == 1).Set(RFlags.NF).AssignBit(RFlags.HF, val.GetHFlagN(1));
            return (GbUInt8)(val - 1);
        });

        /// <summary>
        /// Increments src.
        /// </summary>
        /// <remarks>Affected flags: - - - -</remarks>
        /// <param name="op">The op.</param>
        /// <param name="mem">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        public static bool Inc16(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                mem.R.SetR16(op.Dest, (GbUInt16)(mem.R.GetR16(op.Dest, false) + 1), false);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        /// <summary>
        /// Increments src.
        /// </summary>
        /// <remarks>Affected flags: Z 0 H -</remarks>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        public static bool Inc8(Opcode op, GbMemory memory, int step) => BitOpFunc(op, memory, step, (mem, val, dest) =>
        {
            mem.R.F = mem.R.F.AssignBit(RFlags.ZF, val == 255).Res(RFlags.NF).AssignBit(RFlags.HF, val.GetHCarry(1));
            return (GbUInt8)(val + 1);
        });

        /// <summary>
        /// Ors A and src.
        /// </summary>
        /// <remarks>Affected flags: Z 0 0 0</remarks>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        public static bool Or(Opcode op, GbMemory memory, int step) => ArithOp8Func(op, memory, step, (mem, val) =>
        {
            mem.R.A |= val;
            mem.R.F = mem.R.A == 0 ? RFlags.ZB : (GbUInt8)0;
        });

        /// <summary>
        /// Subtracts src and the carry flag from A
        /// </summary>
        /// <remarks>Affected flags: Z 1 H C</remarks>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        public static bool Sbc(Opcode op, GbMemory memory, int step) => ArithOp8Func(op, memory, step, (mem, val) =>
        {
            // https://github.com/eightlittlebits/elbgb/blob/dffc28001a7a01f93ef9e8abecd7161bcf03cc95/elbgb_core/CPU/LR35902.cs#L1084
            // Reimplemented on 8/26/2017 thanks to the previous link.
            int cIn = mem.R.F[RFlags.CF] ? 1 : 0;
            int res = mem.R.A - val - cIn;
            mem.R.F = ((res & 0xFF) == 0 ? RFlags.ZNB : RFlags.NB).AssignBit(RFlags.HF,
                (mem.R.A & 0xF) - (val & 0xF) - cIn < 0).AssignBit(RFlags.CF, res < 0);
            mem.R.A = (GbUInt8)res;
        });

        /// <summary>
        /// Subtracts src from a.
        /// </summary>
        /// <remarks>Affected flags: Z 1 H C</remarks>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        public static bool Sub(Opcode op, GbMemory memory, int step) => ArithOp8Func(op, memory, step, (mem, val) =>
        {
            byte s = (byte)(mem.R.A - val);
            mem.R.F = RFlags.NB.AssignBit(RFlags.ZF, s == 0).AssignBit(RFlags.HF, mem.R.A.GetHFlagN(val))
            .AssignBit(RFlags.CF, s > mem.R.A);
            mem.R.A = s;
        });

        /// <summary>
        /// Xors A and src.
        /// </summary>
        /// <remarks>Affected flags: Z 0 0 0</remarks>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherwise <see langword="false"/>.</returns>
        public static bool Xor(Opcode op, GbMemory memory, int step) => ArithOp8Func(op, memory, step, (mem, val) =>
        {
            mem.R.A ^= val;
            mem.R.F = mem.R.A == 0 ? RFlags.ZB : (byte)0;
        });
    }
}
