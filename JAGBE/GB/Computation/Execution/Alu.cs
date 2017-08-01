using System;
using JAGBE.GB.DataTypes;

namespace JAGBE.GB.Computation.Execution
{
    // TODO: ensure everything works.
    internal static class Alu
    {
        public static bool Bit(Opcode code, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (code.Src == 6)
                {
                    return false;
                }

                memory.R.F.AssignBit(RFlags.Z, memory.R.GetR8(code.Src).GetBit(code.Dest));
                memory.R.F.AssignBit(RFlags.N, false);
                memory.R.F.AssignBit(RFlags.H, true);
                return true;
            }

            if (step == 1)
            {
                memory.R.F.AssignBit(RFlags.Z, memory.GetMappedMemoryHl().GetBit(code.Dest));
                memory.R.F.AssignBit(RFlags.N, false);
                memory.R.F.AssignBit(RFlags.H, true);
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
                memory.R.F.AssignBit(RFlags.Z, b == 0);
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
                memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
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

                memory.R.F.AssignBit(RFlags.C, b.GetBit(0));
                memory.R.SetR8(code.Src, (byte)(b >> 1));
                memory.R.F.AssignBit(RFlags.Z, b == 0);
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
                memory.R.F.AssignBit(RFlags.C, code.Data1.GetBit(0));
                memory.SetMappedMemory(memory.R.Hl, (byte)(code.Data1 >> 1));
                memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
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

                memory.R.F.AssignBit(RFlags.C, b.GetBit(7));
                memory.R.SetR8(code.Src, (byte)(b << 1));
                memory.R.F.AssignBit(RFlags.Z, b == 0);
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
                memory.R.F.AssignBit(RFlags.C, code.Data1.GetBit(7));
                memory.SetMappedMemory(memory.R.Hl, (byte)(code.Data1 << 1));
                memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
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

                memory.R.F.AssignBit(RFlags.C, b.GetBit(0));
                memory.R.SetR8(code.Src, (byte)((b >> 1) | (b & (1 << 7))));
                memory.R.F.AssignBit(RFlags.Z, b == 0);
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
                memory.R.F.AssignBit(RFlags.C, code.Data1.GetBit(7));
                memory.SetMappedMemory(memory.R.Hl, (byte)((code.Data1 << 1) | (code.Data1 & (1 << 7))));
                memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
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

                memory.R.F.AssignBit(RFlags.C, b.GetBit(7));
                memory.R.SetR8(code.Src, (byte)((b << 1) | (b.GetBit(7) ? 0x80 : 0)));
                memory.R.F.AssignBit(RFlags.Z, b == 0);
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
                memory.R.F.AssignBit(RFlags.C, code.Data1.GetBit(7));
                memory.SetMappedMemory(memory.R.Hl, (byte)((code.Data1 << 1) | (code.Data1.GetBit(7) ? 0x80 : 0)));
                memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
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

                memory.R.F.AssignBit(RFlags.C, b.GetBit(0));
                memory.R.SetR8(code.Src, (byte)((b >> 1) | (b.GetBit(0) ? 1 : 0)));
                memory.R.F.AssignBit(RFlags.Z, b == 0);
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
                memory.R.F.AssignBit(RFlags.C, code.Data1.GetBit(0));
                memory.SetMappedMemory(memory.R.Hl, (byte)((code.Data1 >> 1) | (code.Data1.GetBit(0) ? 1 : 0)));
                memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
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

                memory.R.F.AssignBit(RFlags.C, b.GetBit(0));
                memory.R.SetR8(code.Src, (byte)((b >> 1) | (oldC ? 1 : 0)));
                memory.R.F.AssignBit(RFlags.Z, b == 0);
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
                memory.R.F = 0; // Clear flags.
                memory.R.F.AssignBit(RFlags.C, code.Data1.GetBit(0));
                memory.SetMappedMemory(memory.R.Hl, (byte)((code.Data1 >> 1) | (oldC ? 1 : 0)));
                memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
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

                memory.R.F = 0; // Clear flags.
                byte b = memory.R.GetR8(code.Src);

                memory.R.F.AssignBit(RFlags.C, b.GetBit(7));
                memory.R.SetR8(code.Src, (byte)((b << 1) | (oldC ? 0x80 : 0)));
                memory.R.F.AssignBit(RFlags.Z, b == 0);
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
                memory.R.F = 0; // Clear flags.
                memory.R.F.AssignBit(RFlags.C, code.Data1.GetBit(7));
                memory.SetMappedMemory(memory.R.Hl, (byte)((code.Data1 << 1) | (oldC ? 0x80 : 0)));
                memory.R.F.AssignBit(RFlags.Z, code.Data1 == 0);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

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

                memory.SetMappedMemory(memory.R.Hl, memory.R.GetR8(op.Src));
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
