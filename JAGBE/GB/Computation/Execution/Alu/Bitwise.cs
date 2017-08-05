using System;

namespace JAGBE.GB.Computation.Execution.Alu
{
    internal static class Bitwise
    {
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
            if (step == 0)
            {
                if (code.Src == 6)
                {
                    return false;
                }

                memory.R.SetR8(code.Src, memory.R.GetR8(code.Src).Res(code.Dest));
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.SetMappedMemoryHl(code.Data1.Res(code.Dest));
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Rl(Opcode code, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (code.Src == 6)
                {
                    return false;
                }

                bool oldC = memory.R.F.GetBit(RFlags.CF);

                byte b = memory.R.GetR8(code.Src);
                memory.R.SetR8(code.Src, (byte)((b << 1) | (oldC ? 1 : 0)));
                memory.R.F = (b.GetBit(7) ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, memory.R.GetR8(code.Src) == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                bool oldC = memory.R.F.GetBit(RFlags.CF);
                memory.SetMappedMemoryHl((byte)((code.Data1 << 1) | (oldC ? 1 : 0)));
                memory.R.F = (code.Data1.GetBit(7) ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, memory.GetMappedMemoryHl() == 0);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Rlc(Opcode code, GbMemory memory, int step)
        {
            if (step == 0)
            {
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
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.R.F = code.Data1.GetBit(7) ? RFlags.CB : (byte)0;
                code.Data1 <<= 1;
                memory.SetMappedMemoryHl(code.Data1);
                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, code.Data1 == 0);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Rr(Opcode code, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (code.Src == 6)
                {
                    return false;
                }

                bool oldC = memory.R.F.GetBit(RFlags.CF);

                byte b = memory.R.GetR8(code.Src);

                memory.R.F = b.GetBit(0) ? RFlags.CB : (byte)0;
                memory.R.SetR8(code.Src, (byte)((b >> 1) | (oldC ? 1 : 0)));
                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, b == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                bool oldC = memory.R.F.GetBit(RFlags.CF);
                memory.R.F = code.Data1.GetBit(0) ? RFlags.CB : (byte)0;
                memory.SetMappedMemoryHl((byte)((code.Data1 >> 1) | (oldC ? 1 : 0)));
                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, code.Data1 == 0);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Rrc(Opcode code, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (code.Src == 6)
                {
                    return false;
                }

                byte b = memory.R.GetR8(code.Src);

                memory.R.F = b.GetBit(0) ? RFlags.CF : (byte)0;

                memory.R.SetR8(code.Src, (byte)((b >> 1) | (b.GetBit(0) ? 1 : 0)));
                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, b == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.R.F = code.Data1.GetBit(0) ? RFlags.CF : (byte)0;
                memory.SetMappedMemoryHl((byte)((code.Data1 >> 1) | (code.Data1.GetBit(0) ? 1 : 0)));
                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, code.Data1 == 0);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Set(Opcode code, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (code.Src == 6)
                {
                    return false;
                }

                memory.R.SetR8(code.Src, memory.R.GetR8(code.Src).Set(code.Dest));
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.SetMappedMemoryHl(code.Data1.Set(code.Dest));
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Sla(Opcode code, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (code.Src == 6)
                {
                    return false;
                }

                byte b = memory.R.GetR8(code.Src);

                memory.R.F = b.GetBit(7) ? RFlags.CB : (byte)0;
                memory.R.SetR8(code.Src, (byte)(b << 1));
                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, b == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.R.F = code.Data1.GetBit(7) ? RFlags.CB : (byte)0;
                memory.SetMappedMemoryHl((byte)(code.Data1 << 1));
                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, code.Data1 == 0);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Sra(Opcode code, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (code.Src == 6)
                {
                    return false;
                }

                byte b = memory.R.GetR8(code.Src);

                memory.R.F = b.GetBit(0) ? RFlags.CB : (byte)0;
                memory.R.SetR8(code.Src, (byte)((b >> 1) | (b & (1 << 7))));
                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, b == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.R.F = code.Data1.GetBit(0) ? RFlags.CB : (byte)0;
                memory.SetMappedMemoryHl((byte)((code.Data1 << 1) | (code.Data1 & (1 << 7))));
                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, code.Data1 == 0);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Srl(Opcode code, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (code.Src == 6)
                {
                    return false;
                }

                byte b = memory.R.GetR8(code.Src);

                memory.R.F = b.GetBit(0) ? RFlags.CB : (byte)0;
                memory.R.SetR8(code.Src, (byte)(b >> 1));
                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, b == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.R.F = code.Data1.GetBit(0) ? RFlags.CB : (byte)0;
                memory.SetMappedMemoryHl((byte)(code.Data1 >> 1));
                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, code.Data1 == 0);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Swap(Opcode code, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (code.Src == 6)
                {
                    return false;
                }

                byte b = memory.R.GetR8(code.Src);
                memory.R.SetR8(code.Src, (byte)((b << 4) | (b >> 4)));
                memory.R.F = b == 0 ? RFlags.ZB : (byte)0;
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.SetMappedMemoryHl((byte)((code.Data1 << 4) | (code.Data1 >> 4)));
                memory.R.F = code.Data1 == 0 ? RFlags.ZB : (byte)0;
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }
    }
}
