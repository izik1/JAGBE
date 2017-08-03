using System;

namespace JAGBE.GB.Computation.Execution.Alu
{
    // TODO: Ensure flags get assigned everywhere, they currently aren't...
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

                memory.R.F = memory.R.F.AssignBit(RFlags.Z, !memory.R.GetR8(code.Src).GetBit(code.Dest)).Res(RFlags.N).Set(RFlags.H);
                return true;
            }

            if (step == 1)
            {
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, !memory.GetMappedMemoryHl().GetBit(code.Dest)).Res(RFlags.N).Set(RFlags.H);
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
                memory.SetMappedMemory(memory.R.Hl, code.Data1.Res(code.Dest));
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

                bool oldC = memory.R.F.GetBit(RFlags.C);

                byte b = memory.R.GetR8(code.Src);
                memory.R.SetR8(code.Src, (byte)((b << 1) | (oldC ? 1 : 0)));
                memory.R.F = (b.GetBit(7) ? RFlags.CB : (byte)0).AssignBit(RFlags.Z, b == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                bool oldC = memory.R.F.GetBit(RFlags.C);
                memory.SetMappedMemory(memory.R.Hl, (byte)((code.Data1 << 1) | (oldC ? 1 : 0)));
                memory.R.F = (code.Data1.GetBit(7) ? RFlags.CB : (byte)0).AssignBit(RFlags.Z, code.Data1 == 0);
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

                memory.R.F = 0; // Clear flags.
                byte b = memory.R.GetR8(code.Src);

                memory.R.F = memory.R.F.AssignBit(RFlags.C, b.GetBit(7));
                memory.R.SetR8(code.Src, (byte)((b << 1) | (b.GetBit(7) ? 0x80 : 0)));
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, b == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.R.F = 0; // Clear flags.
                memory.R.F = memory.R.F.AssignBit(RFlags.C, code.Data1.GetBit(7));
                memory.SetMappedMemory(memory.R.Hl, (byte)((code.Data1 << 1) | (code.Data1.GetBit(7) ? 0x80 : 0)));
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
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

                bool oldC = memory.R.F.GetBit(RFlags.C);

                memory.R.F = 0; // Clear flags.
                byte b = memory.R.GetR8(code.Src);

                memory.R.F = memory.R.F.AssignBit(RFlags.C, b.GetBit(0));
                memory.R.SetR8(code.Src, (byte)((b >> 1) | (oldC ? 1 : 0)));
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, b == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                bool oldC = memory.R.F.GetBit(RFlags.C);
                memory.R.F = ((byte)0).AssignBit(RFlags.C, code.Data1.GetBit(0));
                memory.SetMappedMemory(memory.R.Hl, (byte)((code.Data1 >> 1) | (oldC ? 1 : 0)));
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
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

                memory.R.F = 0; // Clear flags.
                byte b = memory.R.GetR8(code.Src);

                memory.R.F = memory.R.F.AssignBit(RFlags.C, b.GetBit(0));
                memory.R.SetR8(code.Src, (byte)((b >> 1) | (b.GetBit(0) ? 1 : 0)));
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, b == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.R.F = 0; // Clear flags.
                memory.R.F = memory.R.F.AssignBit(RFlags.C, code.Data1.GetBit(0));
                memory.SetMappedMemory(memory.R.Hl, (byte)((code.Data1 >> 1) | (code.Data1.GetBit(0) ? 1 : 0)));
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
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
                memory.SetMappedMemory(memory.R.Hl, code.Data1.Set(code.Dest));
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

                memory.R.F = 0; // Clear flags.
                byte b = memory.R.GetR8(code.Src);

                memory.R.F = memory.R.F.AssignBit(RFlags.C, b.GetBit(7));
                memory.R.SetR8(code.Src, (byte)(b << 1));
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, b == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.R.F = 0; // Clear flags.
                memory.R.F = memory.R.F.AssignBit(RFlags.C, code.Data1.GetBit(7));
                memory.SetMappedMemory(memory.R.Hl, (byte)(code.Data1 << 1));
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
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

                memory.R.F = 0; // Clear flags.
                byte b = memory.R.GetR8(code.Src);

                memory.R.F = memory.R.F.AssignBit(RFlags.C, b.GetBit(0));
                memory.R.SetR8(code.Src, (byte)((b >> 1) | (b & (1 << 7))));
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, b == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.R.F = 0; // Clear flags.
                memory.R.F = memory.R.F.AssignBit(RFlags.C, code.Data1.GetBit(7));
                memory.SetMappedMemory(memory.R.Hl, (byte)((code.Data1 << 1) | (code.Data1 & (1 << 7))));
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
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

                memory.R.F = 0; // Clear flags.
                byte b = memory.R.GetR8(code.Src);

                memory.R.F = memory.R.F.AssignBit(RFlags.C, b.GetBit(0));
                memory.R.SetR8(code.Src, (byte)(b >> 1));
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, b == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.R.F = 0; // Clear flags.
                memory.R.F = memory.R.F.AssignBit(RFlags.C, code.Data1.GetBit(0));
                memory.SetMappedMemory(memory.R.Hl, (byte)(code.Data1 >> 1));
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
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
                memory.R.F = 0; // Clear flags.
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, b == 0);
                return true;
            }

            if (step == 1)
            {
                code.Data1 = memory.GetMappedMemoryHl();
                return false;
            }

            if (step == 2)
            {
                memory.SetMappedMemory(memory.R.Hl, (byte)((code.Data1 << 4) | (code.Data1 >> 4)));
                memory.R.F = 0; // Clear flags.
                memory.R.F = memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }
    }
}
