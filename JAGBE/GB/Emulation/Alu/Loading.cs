using System;

namespace JAGBE.GB.Emulation.Alu
{
    internal static class Loading
    {
        public static int Ld8(Opcode op, GbMemory memory)
        {
            if (op.Src == 6 || op.Dest == 6)
            {
                if (op.Src == op.Dest) // By math logic, these are both 6 if this is true.
                {
                    throw new InvalidOperationException("Opcode with dest = 6 and src = 6 is invalid for this instruction.");
                }

                memory.Update();
                if (op.Src == 6)
                {
                    memory.R.SetR8(op.Dest, memory.GetMappedMemoryHl());
                }
                else
                {
                    memory.SetMappedMemory(memory.R.Hl, memory.R.GetR8(op.Src));
                }

                return 2;
            }

            memory.R.SetR8(op.Dest, memory.R.GetR8(op.Src));
            return 1;
        }

        public static int LdA16(Opcode op, GbMemory mem)
        {
            mem.Update();
            op.Data1 = mem.LdI8(); // Low byte.
            mem.Update();
            op.Data2 = mem.LdI8(); // High byte.
            mem.Update();

            if (op.Dest == 7)
            {
                mem.R.A = mem.GetMappedMemory(new GbUInt16(op.Data2, op.Data1));
            }
            else
            {
                mem.SetMappedMemory(new GbUInt16(op.Data2, op.Data1), mem.R.A);
            }

            return 4;
        }

        public static int LdA16Sp(Opcode op, GbMemory mem)
        {
            mem.Update();
            op.Data1 = mem.LdI8();
            mem.Update();
            op.Data2 = mem.LdI8();
            mem.Update();
            mem.SetMappedMemory(new GbUInt16(op.Data2, op.Data1), mem.R.Sp.LowByte);
            mem.Update();
            mem.SetMappedMemory((ushort)(new GbUInt16(op.Data2, op.Data1) + 1), mem.R.Sp.HighByte);
            return 5;
        }

        public static int LdD16(Opcode op, GbMemory mem)
        {
            mem.Update();
            mem.R.SetR16(op.Dest, new GbUInt16(mem.R.GetR16(op.Dest, false).HighByte, mem.LdI8()), false);
            mem.Update();
            mem.R.SetR16(op.Dest, new GbUInt16(mem.LdI8(), mem.R.GetR16(op.Dest, false).LowByte), false);
            return 3;
        }

        public static int LdD8(Opcode op, GbMemory mem)
        {
            mem.Update();
            op.Data1 = mem.LdI8();
            if (op.Dest == 6)
            {
                mem.Update();
                mem.SetMappedMemory(mem.R.Hl, op.Data1);
                return 3;
            }

            mem.R.SetR8(op.Dest, op.Data1);
            return 2;
        }

        public static int LdH(Opcode op, GbMemory mem)
        {
            mem.Update();
            op.Data1 = mem.LdI8();
            mem.Update();
            if (op.Dest == 7)
            {
                mem.R.A = mem.GetMappedMemory((GbUInt16)0xFF00 + op.Data1);
            }
            else
            {
                mem.SetMappedMemory((GbUInt16)0xFF00 + op.Data1, mem.R.A);
            }

            return 3;
        }

        public static int LdHlSpR8(Opcode op, GbMemory mem)
        {
            mem.Update();
            op.Data1 = mem.LdI8();
            mem.Update();
            sbyte s = (sbyte)op.Data1;
            mem.R.F = (((mem.R.Sp & 0xFF) +
                (s & 0xFF)) > 0xFF ? RFlags.CB : (GbUInt8)0).AssignBit(RFlags.HF, ((mem.R.Sp & 0x0F) + (s & 0x0F)) > 0x0F);
            mem.R.Hl = mem.R.Sp + s;
            return 3;
        }

        public static int LdR16(Opcode op, GbMemory mem)
        {
            mem.Update();
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

            return 2;
        }

        public static int LdSpHl(Opcode op, GbMemory memory)
        {
            memory.Update();
            memory.R.Sp = memory.R.Hl;
            return 2;
        }

        public static int Pop(Opcode op, GbMemory mem)
        {
            mem.Update();
            if (op.Dest == 3)
            {
                mem.R.F = (GbUInt8)(mem.Pop() & 0xF0);
            }
            else
            {
                mem.R.SetR8((op.Dest * 2) + 1, mem.Pop());
            }

            mem.Update();
            if (op.Dest == 3)
            {
                mem.R.A = mem.Pop();
            }
            else
            {
                mem.R.SetR8(op.Dest * 2, mem.Pop());
            }

            return 3;
        }

        public static int Push(Opcode op, GbMemory mem)
        {
            mem.Update();
            mem.Update();
            mem.Push(mem.R.GetR16(op.Dest, true).HighByte);
            mem.Update();
            mem.Push(mem.R.GetR16(op.Dest, true).LowByte);
            return 4;
        }
    }
}
