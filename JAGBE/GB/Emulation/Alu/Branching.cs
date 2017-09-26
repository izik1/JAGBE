namespace JAGBE.GB.Emulation.Alu
{
    internal static class Branching
    {
        public static int Call(Opcode op, GbMemory mem)
        {
            byte low = mem.ReadCycleI8();
            byte high = mem.ReadCycleI8();
            if (op.Src != 0 && GetConditionalJumpState(op.Dest, op.Src, mem.R.F))
            {
                return 3;
            }

            mem.Update();
            mem.Update();
            mem.Push(mem.R.Pc.HighByte);
            mem.Update();
            mem.Push(mem.R.Pc.LowByte);
            mem.R.Pc = new GbUInt16(high, low);
            return 6;
        }

        public static int Jp(Opcode op, GbMemory mem)
        {
            byte low = mem.ReadCycleI8();
            byte high = mem.ReadCycleI8();
            if (op.Src != 0 && GetConditionalJumpState(op.Dest, op.Src, mem.R.F))
            {
                return 3;
            }

            mem.Update();
            mem.R.Pc = new GbUInt16(high, low);
            return 4;
        }

        public static int Jr8(Opcode op, GbMemory mem)
        {
            mem.Update();
            if (op.Src != 0 && GetConditionalJumpState(op.Dest, op.Src, mem.R.F))
            {
                mem.R.Pc++;
                return 2;
            }

            mem.R.Pc += (sbyte)mem.ReadCycleI8();
            mem.R.Pc++;
            return 3;
        }

        public static int Ret(Opcode op, GbMemory mem)
        {
            byte low = mem.ReadCyclePop();
            mem.R.Pc = new GbUInt16(mem.ReadCyclePop(), low);
            mem.Update();
            mem.IME |= op.Dest != 0; // Unlike EI IME gets enabled right away.
            mem.NextIMEValue = mem.IME;
            return 4;
        }

        /// <summary>
        /// Conditional Return.
        /// </summary>
        /// <param name="op">The opcode.</param>
        /// <param name="mem">The memory.</param>
        public static int RetC(Opcode op, GbMemory mem)
        {
            mem.Update();
            if (GetConditionalJumpState(op.Dest, op.Src, mem.R.F))
            {
                return 2;
            }

            byte low = mem.ReadCyclePop();
            mem.R.Pc = new GbUInt16(mem.ReadCyclePop(), low);
            mem.Update();
            return 5;
        }

        public static int Rst(Opcode op, GbMemory mem)
        {
            mem.Update();
            mem.Update();
            mem.Push(mem.R.Pc.HighByte);
            mem.Update();
            mem.Push(mem.R.Pc.LowByte);
            mem.R.Pc = new GbUInt16(0, (byte)(op.Dest * 8));
            return 4;
        }

        /// <summary>
        /// <see langword="true"/> is shouldn't jump.
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="src">The source.</param>
        /// <param name="flags">The flags.</param>
        /// <returns></returns>
        private static bool GetConditionalJumpState(byte dest, byte src, byte flags) =>
            flags.GetBit(src == 1 ? RFlags.ZF : RFlags.CF) ^ (dest != 0);
    }
}
