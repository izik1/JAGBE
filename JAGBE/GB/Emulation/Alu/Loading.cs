using System;

namespace JAGBE.GB.Emulation.Alu
{
    internal static class Loading
    {
        public static bool Ld8(Opcode op, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (op.Src == 6 || op.Dest == 6)
                {
                    if (op.Src == op.Dest) // By math logic, these are both 6 if this is true.
                    {
                        throw new InvalidOperationException("Opcode with dest = 6 and src = 6 is invalid for this instruction.");
                    }

                    return false;
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

                if (op.Dest == 6)
                {
                    memory.SetMappedMemory(memory.R.Hl, memory.R.GetR8(op.Src));
                    return true;
                }

                // Should never throw.
                throw new InvalidOperationException();
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool LdA16(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                    return false;

                case 1:
                    op.Data1 = mem.LdI8(); // Low byte.
                    return false;

                case 2:
                    op.Data2 = mem.LdI8(); // High byte.
                    return false;

                case 3:
                    if (op.Dest == 7)
                    {
                        mem.R.A = mem.GetMappedMemory(new GbUInt16(op.Data2, op.Data1));
                    }
                    else
                    {
                        mem.SetMappedMemory(new GbUInt16(op.Data2, op.Data1), mem.R.A);
                    }
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool LdA16Sp(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                    return false;

                case 1:
                    op.Data1 = mem.LdI8();
                    return false;

                case 2:
                    op.Data2 = mem.LdI8();
                    return false;

                case 3:
                    mem.SetMappedMemory(new GbUInt16(op.Data2, op.Data1), mem.R.Sp.LowByte);
                    return false;

                case 4:
                    mem.SetMappedMemory((ushort)(new GbUInt16(op.Data2, op.Data1) + 1), mem.R.Sp.HighByte);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool LdD16(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                    return false;

                case 1:
                    mem.R.SetR16(op.Dest, new GbUInt16(mem.R.GetR16(op.Dest, false).HighByte, mem.LdI8()), false);
                    return false;

                case 2:
                    mem.R.SetR16(op.Dest, new GbUInt16(mem.LdI8(), mem.R.GetR16(op.Dest, false).LowByte), false);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool LdD8(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                    return false;

                case 1:
                    op.Data1 = mem.LdI8();
                    if (op.Dest == 6)
                    {
                        return false;
                    }

                    mem.R.SetR8(op.Dest, op.Data1);
                    return true;

                case 2:
                    mem.SetMappedMemory(mem.R.Hl, op.Data1);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool LdH(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                    return false;

                case 1:
                    op.Data1 = mem.LdI8();
                    return false;

                case 2:
                    if (op.Dest == 7)
                    {
                        mem.R.A = mem.GetMappedMemory((ushort)(0xFF00 + op.Data1));
                    }
                    else
                    {
                        mem.SetMappedMemory((ushort)(0xFF00 + op.Data1), mem.R.A);
                    }
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool LdHlSpR8(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                    return false;

                case 1:
                    op.Data1 = mem.LdI8();
                    return false;

                case 2:
                    sbyte s = (sbyte)op.Data1;
                    mem.R.F = (((mem.R.Sp & 0xFF) + (s & 0xFF)) > 0xFF ? RFlags.CB : (GbUInt8)0)
                        .AssignBit(RFlags.HF, ((mem.R.Sp & 0x0F) + (s & 0x0F)) > 0x0F);
                    mem.R.Hl = mem.R.Sp + s;
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        public static bool LdR16(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                if (op.Src == 8)
                {
                    mem.SetMappedMemory((op.Dest == 2 || op.Dest == 3) ? mem.R.Hl : mem.R.GetR16(op.Dest, false), mem.R.A);
                }
                else if (op.Dest == 8)
                {
                    mem.R.A = mem.GetMappedMemory((op.Src == 2 || op.Src == 3) ? mem.R.Hl : mem.R.GetR16(op.Src, false));
                }
                else
                {
                    throw new ArgumentException(nameof(op));
                }
                if (op.Src == 2 || op.Dest == 2)
                {
                    mem.R.Hl++;
                }
                else if (op.Src == 3 || op.Dest == 3)
                {
                    mem.R.Hl--;
                }
                else
                {
                    // Do nothing.
                }
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool LdSpHl(Opcode op, GbMemory memory, int step)
        {
            if (step == 0)
            {
                memory.R.Sp = memory.R.Hl;
                return false;
            }

            if (step == 1)
            {
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Pop(Opcode op, GbMemory mem, int step)
        {
            if (step == 0)
            {
                return false;
            }

            if (step == 1)
            {
                GbUInt8 b = mem.Pop();
                if (op.Dest == 3)
                {
                    b &= 0xF0;
                    mem.R.F = b;
                }
                else
                {
                    mem.R.SetR8((op.Dest * 2) + 1, b);
                }

                return false;
            }

            if (step == 2)
            {
                if (op.Dest == 3)
                {
                    mem.R.A = mem.Pop();
                }
                else
                {
                    mem.R.SetR8(op.Dest * 2, mem.Pop());
                }

                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        public static bool Push(Opcode op, GbMemory mem, int step)
        {
            switch (step)
            {
                case 0:
                case 1:
                    return false;

                case 2:
                    mem.Push(mem.R.GetR16(op.Dest, true).HighByte);
                    return false;

                case 3:
                    mem.Push(mem.R.GetR16(op.Dest, true).LowByte);
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }
    }
}
