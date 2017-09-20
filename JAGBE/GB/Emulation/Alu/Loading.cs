﻿using System;

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
            GbUInt8 low = mem.ReadCycleI8();
            GbUInt8 high = mem.ReadCycleI8();
            mem.Update();

            if (op.Dest == 7)
            {
                mem.R.A = mem.GetMappedMemory(new GbUInt16(high, low));
            }
            else
            {
                mem.SetMappedMemory(new GbUInt16(high, low), mem.R.A);
            }

            return 4;
        }

        public static int LdA16Sp(Opcode op, GbMemory mem)
        {
            GbUInt8 low = mem.ReadCycleI8();
            GbUInt8 high = mem.ReadCycleI8();
            mem.Update();
            mem.SetMappedMemory(new GbUInt16(high, low), mem.R.Sp.LowByte);
            mem.Update();
            mem.SetMappedMemory((GbUInt16)(new GbUInt16(high, low) + 1), mem.R.Sp.HighByte);
            return 5;
        }

        public static int LdD16(Opcode op, GbMemory mem)
        {
            GbUInt8 low = mem.ReadCycleI8();
            mem.R.SetR16(op.Dest, new GbUInt16(mem.ReadCycleI8(), low));
            return 3;
        }

        public static int LdD8(Opcode op, GbMemory mem)
        {
            GbUInt8 val = mem.ReadCycleI8();
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
            GbUInt8 val = mem.ReadCycleI8();
            mem.Update();
            if (op.Dest == 7)
            {
                mem.R.A = mem.GetMappedMemory((GbUInt16)0xFF00 + val);
            }
            else
            {
                mem.SetMappedMemory((GbUInt16)0xFF00 + val, mem.R.A);
            }

            return 3;
        }

        public static int LdHlSpR8(Opcode op, GbMemory mem)
        {
            sbyte s = (sbyte)mem.ReadCycleI8();
            mem.Update();
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
            if (op.Dest == 3)
            {
                mem.R.F = (GbUInt8)(mem.ReadCyclePop() & 0xF0);
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
            mem.Update();
            mem.Update();
            mem.Push(mem.R.GetR16(op.Dest, true).HighByte);
            mem.Update();
            mem.Push(mem.R.GetR16(op.Dest, true).LowByte);
            return 4;
        }
    }
}
