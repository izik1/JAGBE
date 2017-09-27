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

                if (op.Src == 6)
                {
                    memory.R.SetR8(op.Dest, memory.ReadCycleHl());
                }
                else
                {
                    memory.Update();
                    memory.SetMappedMemory(memory.R.Hl, memory.R.GetR8(op.Src));
                }

                return 2;
            }

            memory.R.SetR8(op.Dest, memory.R.GetR8(op.Src));
            return 1;
        }

        public static int LdA16(Opcode op, GbMemory mem)
        {
            byte low = mem.ReadCycleI8();
            byte high = mem.ReadCycleI8();
            if (op.Dest == 7)
            {
                mem.R.A = mem.ReadCycle(new GbUInt16(high, low));
            }
            else
            {
                mem.Update();
                mem.SetMappedMemory(new GbUInt16(high, low), mem.R.A);
            }

            return 4;
        }

        public static int LdA16Sp(Opcode op, GbMemory mem)
        {
            byte low = mem.ReadCycleI8();
            byte high = mem.ReadCycleI8();
            mem.Update();
            mem.SetMappedMemory(new GbUInt16(high, low), mem.R.Sp.LowByte);
            mem.Update();
            mem.SetMappedMemory((GbUInt16)(new GbUInt16(high, low) + 1), mem.R.Sp.HighByte);
            return 5;
        }

        public static int LdD16(Opcode op, GbMemory mem)
        {
            byte low = mem.ReadCycleI8();
            mem.R.SetR16(op.Dest, new GbUInt16(mem.ReadCycleI8(), low));
            return 3;
        }

        public static int LdD8(Opcode op, GbMemory mem)
        {
            byte val = mem.ReadCycleI8();
            if (op.Dest == 6)
            {
                mem.Update();
                mem.SetMappedMemory(mem.R.Hl, val);
                return 3;
            }

            mem.R.SetR8(op.Dest, val);
            return 2;
        }

        public static int LdH(Opcode op, GbMemory mem)
        {
            if (op.Dest == 7)
            {
                mem.R.A = mem.ReadCycle(new GbUInt16(0xFF, mem.ReadCycleI8()));
            }
            else
            {
                mem.Update();
                mem.SetMappedMemory(new GbUInt16(0xFF, mem.ReadCycleI8()), mem.R.A);
            }

            return 3;
        }

        public static int LdHlSpR8(Opcode op, GbMemory mem)
        {
            sbyte s = (sbyte)mem.ReadCycleI8();
            mem.Update();
            mem.R.F = (((mem.R.Sp & 0xFF) +
                (s & 0xFF)) > 0xFF ? RFlags.CB : (byte)0).AssignBit(RFlags.HF, ((mem.R.Sp & 0x0F) + (s & 0x0F)) > 0x0F);
            mem.R.Hl = mem.R.Sp + s;
            return 3;
        }

        public static int LdR16(Opcode op, GbMemory mem)
        {
            mem.Update();
            if (op.Src == 8)
            {
                mem.SetMappedMemory((op.Dest == 2 || op.Dest == 3) ? mem.R.Hl : mem.R.GetR16Sp(op.Dest), mem.R.A);
            }
            else
            {
                mem.R.A = mem.GetMappedMemory((op.Src == 2 || op.Src == 3) ? mem.R.Hl : mem.R.GetR16Sp(op.Src));
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
            if (op.Dest == 3)
            {
                mem.R.F = (byte)(mem.ReadCyclePop() & 0xF0);
                mem.R.A = mem.ReadCyclePop();
            }
            else
            {
                mem.R.SetR8((op.Dest * 2) + 1, mem.ReadCyclePop());
                mem.R.SetR8(op.Dest * 2, mem.ReadCyclePop());
            }

            return 3;
        }

        public static int Push(Opcode op, GbMemory mem)
        {
            GbUInt16 r = mem.R.GetR16Af(op.Dest);
            mem.Update();
            mem.Update();
            mem.Push(r.HighByte);
            mem.Update();
            mem.Push(r.LowByte);
            return 4;
        }
    }
}
