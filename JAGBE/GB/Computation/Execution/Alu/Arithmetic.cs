using System;
using JAGBE.GB.DataTypes;

namespace JAGBE.GB.Computation.Execution.Alu
{
    internal static class Arithmetic
    {
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
                    RFlags.H, mem.R.A.GetHFlag(s)).AssignBit(RFlags.C, mem.R.A > s);
                mem.R.A = s;
                return true;
            }

            if (step == 1)
            {
                byte s = (byte)(mem.R.A + (op.Src == 6 ? mem.GetMappedMemoryHl() : mem.LdI8()));
                mem.R.F = (s == 0 ? RFlags.ZB : (byte)0).AssignBit(
                    RFlags.H, mem.R.A.GetHFlag(s)).AssignBit(RFlags.C, mem.R.A > s);
                mem.R.A = s;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Adc(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                if (op.Src == 6 || op.Src == 8)
                {
                    return false;
                }

                bool c = mem.R.F.GetBit(RFlags.C);
                byte s = (byte)((c ? 1 : 0) + mem.R.A + mem.R.GetR8(op.Src));
                mem.R.F = (s == 0 ? RFlags.ZB : (byte)0).AssignBit(
                    RFlags.H, mem.R.A.GetHFlag(s)).AssignBit(RFlags.C, c ? s == mem.R.A : s < mem.R.A);
                mem.R.A = s;

                return true;
            }

            if (step == 1)
            {
                byte s;
                bool c = mem.R.F.GetBit(RFlags.C);
                s = op.Src == 6 ? (byte)((c ? 1 : 0) + mem.R.A + mem.GetMappedMemoryHl()) : (byte)((c ? 1 : 0) + mem.R.A + mem.LdI8());

                mem.R.F = (s == 0 ? RFlags.ZB : (byte)0).AssignBit(
                    RFlags.H, mem.R.A.GetHFlag(s)).AssignBit(RFlags.C, c ? s == mem.R.A : s < mem.R.A);

                mem.R.A = s;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Sub(Opcode op, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (op.Src == 6 || op.Src == 8)
                {
                    return false;
                }

                byte s = (byte)(memory.R.A - memory.R.GetR8(op.Src));
                memory.R.F = RFlags.NB.AssignBit(RFlags.Z, s == 0).AssignBit(
                    RFlags.H, memory.R.A.GetHFlagN(s)).AssignBit(RFlags.C, s > memory.R.A);
                memory.R.A = s;
                return true;
            }

            if (step == 1)
            {
                byte s = (byte)(memory.R.A - (op.Src == 6 ? memory.GetMappedMemoryHl() : memory.LdI8()));
                memory.R.F = RFlags.NB.AssignBit(RFlags.Z, s == 0).AssignBit(
                    RFlags.H, memory.R.A.GetHFlagN(s)).AssignBit(RFlags.C, s > memory.R.A);
                memory.R.A = s;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

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

        public static bool Cp(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                if (op.Src == 6 || op.Src == 8)
                {
                    return false;
                }

                byte r = (byte)(mem.R.A - mem.R.GetR8(op.Src));
                mem.R.F = RFlags.NB.AssignBit(RFlags.Z, r == 0).AssignBit(RFlags.H, mem.R.A.GetHFlagN(r)).AssignBit(RFlags.C, r > mem.R.A);
                return true;
            }

            if (step == 1)
            {
                byte r = op.Src == 6 ? mem.GetMappedMemoryHl() : mem.LdI8();
                mem.R.F = RFlags.NB.AssignBit(RFlags.Z, r == 0).AssignBit(RFlags.H, mem.R.A.GetHFlagN(r)).AssignBit(RFlags.C, r > mem.R.A);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Inc8(Opcode op, GbMemory mem, int step)
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
                mem.SetMappedMemory(mem.R.Hl, (byte)(op.Data1 + 1));

                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
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

        public static bool Dec8(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                if (op.Dest == 6)
                {
                    return false;
                }

                byte b = mem.R.GetR8(op.Dest);
                mem.R.F = mem.R.F.AssignBit(RFlags.Z, b == 1).Set(RFlags.N).AssignBit(RFlags.H, b.GetHFlagN((byte)(b - 1)));
                mem.R.SetR8(op.Dest, (byte)(b - 1));
                return true;
            }

            if (step == 1)
            {
                op.Data1 = mem.GetMappedMemoryHl();

                return false;
            }

            if (step == 2)
            {
                mem.R.F = mem.R.F.AssignBit(RFlags.Z, op.Data1 == 1)
                    .Res(RFlags.N).AssignBit(RFlags.H, op.Data1.GetHFlag((byte)(op.Data1 - 1)));
                mem.R.SetR8(op.Dest, (byte)(op.Data1 - 1));

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
                mem.R.SetR16(op.Dest, (GbUInt16)(mem.R.GetR16(op.Dest, false) - 1), false);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }
    }
}
