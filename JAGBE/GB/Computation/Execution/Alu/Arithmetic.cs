using System;
using JAGBE.GB.DataTypes;

namespace JAGBE.GB.Computation.Execution.Alu
{
    internal static class Arithmetic
    {
        private delegate void Op8(GbMemory mem, byte valIn);

        public static bool Adc(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                if (op.Src == 6 || op.Src == 8)
                {
                    return false;
                }

                bool c = mem.R.F.GetBit(RFlags.CF);
                byte b = (byte)(mem.R.GetR8(op.Src) + (c ? 1 : 0));
                byte s = (byte)(mem.R.A + b);
                mem.R.F = (s == 0 ? RFlags.ZB : (byte)0).AssignBit(
                    RFlags.HF, mem.R.A.GetHFlag(b)).AssignBit(RFlags.CF, (c ? s - 1 : s) < mem.R.A);
                mem.R.A = s;

                return true;
            }

            if (step == 1)
            {
                bool c = mem.R.F.GetBit(RFlags.CF);
                byte b = (byte)((op.Src == 6 ? mem.GetMappedMemoryHl() : mem.LdI8()) + (c ? 1 : 0));
                byte s = (byte)(mem.R.A + b);

                mem.R.F = (s == 0 ? RFlags.ZB : (byte)0).AssignBit(
                    RFlags.HF, mem.R.A.GetHFlag(b)).AssignBit(RFlags.CF, (c ? s - 1 : s) < mem.R.A);

                mem.R.A = s;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Add(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                if (op.Src == 6 || op.Src == 8)
                {
                    return false;
                }

                byte s = (byte)(mem.R.A + mem.R.GetR8(op.Src));
                mem.R.F = (s == 0 ? RFlags.ZB : (byte)0).AssignBit(
                    RFlags.HF, mem.R.A.GetHFlag(mem.R.GetR8(op.Src))).AssignBit(RFlags.CF, mem.R.A > s);
                mem.R.A = s;
                return true;
            }

            if (step == 1)
            {
                byte b = (op.Src == 6 ? mem.GetMappedMemoryHl() : mem.LdI8());
                byte s = (byte)(mem.R.A + b);
                mem.R.F = (s == 0 ? RFlags.ZB : (byte)0).AssignBit(
                    RFlags.HF, mem.R.A.GetHFlag(b)).AssignBit(RFlags.CF, mem.R.A > s);
                mem.R.A = s;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool AddHl(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                ushort val = mem.R.GetR16(op.Src, false);
                mem.R.F = mem.R.F.Res(RFlags.NF).AssignBit(RFlags.HF, val.GetHalfCarry(mem.R.Hl)).AssignBit(RFlags.CF, val + mem.R.Hl < mem.R.Hl);
                mem.R.Hl += val;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool And(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                if (op.Src == 6 || op.Src == 8)
                {
                    return false;
                }

                byte r = (byte)(mem.R.A & mem.R.GetR8(op.Src));
                mem.R.A = r;
                mem.R.F = (byte)((r == 0 ? RFlags.ZB : 0) | RFlags.HB);
                return true;
            }

            if (step == 1)
            {
                byte r = (byte)(mem.R.A & (op.Src == 6 ? mem.GetMappedMemoryHl() : mem.LdI8()));
                mem.R.A = r;
                mem.R.F = (byte)((r == 0 ? RFlags.ZB : 0) | RFlags.HB);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Cp(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                if (op.Src == 6 || op.Src == 8)
                {
                    return false;
                }

                byte r = (byte)(mem.R.A - mem.R.GetR8(op.Src));
                mem.R.F = RFlags.NB.AssignBit(RFlags.ZF, r == 0).AssignBit(RFlags.HF, mem.R.A.GetHFlagN(r)).AssignBit(RFlags.CF, r > mem.R.A);
                return true;
            }

            if (step == 1)
            {
                byte r = (byte)(mem.R.A - (op.Src == 6 ? mem.GetMappedMemoryHl() : mem.LdI8()));
                mem.R.F = RFlags.NB.AssignBit(RFlags.ZF, r == 0).AssignBit(RFlags.HF, mem.R.A.GetHFlagN(r)).AssignBit(RFlags.CF, r > mem.R.A);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        /// <summary>
        /// Complements the A register of the given <paramref name="mem"/>
        /// </summary>
        /// <remarks>Affected Flags: Z, C = Unaffected. N,H = 1</remarks>
        /// <param name="op">The op.</param>
        /// <param name="mem">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">step</exception>
        public static bool Cpl(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                mem.R.A = (byte)(~mem.R.A);
                mem.R.F |= RFlags.NHB;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Daa(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
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

                mem.R.A = (byte)res;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

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

        public static bool Dec8(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                    if (op.Dest == 6)
                    {
                        return false;
                    }

                    byte b = mem.R.GetR8(op.Dest);
                    mem.R.F = mem.R.F.AssignBit(RFlags.ZF, b == 1).Set(RFlags.NF).AssignBit(RFlags.HF, b.GetHFlagN(1));
                    mem.R.SetR8(op.Dest, (byte)(b - 1));
                    return true;

                case 1:
                    op.Data1 = mem.GetMappedMemoryHl();
                    return false;

                case 2:
                    mem.R.F = mem.R.F.AssignBit(RFlags.ZF, op.Data1 == 1).Set(RFlags.NF).AssignBit(RFlags.HF, op.Data1.GetHFlagN(1));
                    mem.SetMappedMemoryHl((byte)(op.Data1 - 1));
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool Inc16(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                mem.R.SetR16(op.Dest, mem.R.GetR16(op.Dest, false) + 1, false);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Inc8(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                    if (op.Dest == 6)
                    {
                        return false;
                    }

                    byte b = mem.R.GetR8(op.Dest);
                    mem.R.F = mem.R.F.AssignBit(RFlags.ZF, b == 255).Res(RFlags.NF).AssignBit(RFlags.HF, b.GetHFlag(1));
                    mem.R.SetR8(op.Dest, (byte)(b + 1));
                    return true;

                case 1:
                    op.Data1 = mem.GetMappedMemoryHl();
                    return false;

                case 2:
                    mem.R.F = mem.R.F.AssignBit(RFlags.ZF, op.Data1 == 255).Res(RFlags.NF).AssignBit(RFlags.HF, op.Data1.GetHFlag(1));
                    mem.SetMappedMemoryHl((byte)(op.Data1 + 1));
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool Or(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                if (op.Src == 6 || op.Src == 8)
                {
                    return false;
                }

                byte r = (byte)(mem.R.A | mem.R.GetR8(op.Src));
                mem.R.A = r;
                mem.R.F = r == 0 ? RFlags.ZB : (byte)0;
                return true;
            }

            if (step == 1)
            {
                byte r = (byte)(mem.R.A | (op.Src == 6 ? mem.GetMappedMemoryHl() : mem.LdI8()));
                mem.R.A = r;
                mem.R.F = r == 0 ? RFlags.ZB : (byte)0;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Sub(Opcode op, GbMemory memory, int step) => Operate8(op, memory, step, (mem, val) =>
        {
            byte s = (byte)(memory.R.A - val);
            mem.R.F = RFlags.NB.AssignBit(RFlags.ZF, s == 0).AssignBit(RFlags.HF, mem.R.A.GetHFlagN(val)).AssignBit(RFlags.CF, s > mem.R.A);
            mem.R.A = s;
        });

        public static bool Xor(Opcode op, GbMemory memory, int step) => Operate8(op, memory, step, (mem, val) =>
        {
            mem.R.A ^= val;
            mem.R.F = memory.R.A == 0 ? RFlags.ZB : (byte)0;
        });

        private static bool Operate8(Opcode op, GbMemory memory, int step, Op8 operation)
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
