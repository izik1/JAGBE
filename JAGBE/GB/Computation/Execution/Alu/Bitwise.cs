using System;

namespace JAGBE.GB.Computation.Execution.Alu
{
    internal static class Bitwise
    {
        private delegate byte Op(GbMemory mem, byte valIn, byte dest);

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

        public static bool Res(Opcode code, GbMemory memory, int step) => Operate(code, memory, step, (m, val, dest) => val.Res(dest));

        public static bool Rl(Opcode code, GbMemory memory, int step) =>
            Operate(code, memory, step, (mem, val, dest) =>
            {
                byte retVal = (byte)((val << 1) | (mem.R.F.GetBit(RFlags.CF) ? 1 : 0));
                mem.R.F = (val.GetBit(7) ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
                return retVal;
            });

        public static bool Rlc(Opcode code, GbMemory memory, int step) =>
            Operate(code, memory, step, (mem, val, dest) =>
            {
                byte retVal = (byte)(val << 1);
                mem.R.F = (val.GetBit(7) ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
                return retVal;
            });

        public static bool Rr(Opcode code, GbMemory memory, int step) =>
            Operate(code, memory, step, (mem, val, dest) =>
            {
                byte retVal = (byte)((val >> 1) | (mem.R.F.GetBit(RFlags.CF) ? 0x80 : 0));
                mem.R.F = (val.GetBit(0) ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
                return retVal;
            });

        public static bool Rrc(Opcode code, GbMemory memory, int step) =>
            Operate(code, memory, step, (mem, val, dest) =>
            {
                byte retVal = (byte)((val >> 1) | (val.GetBit(0) ? 1 : 0));
                mem.R.F = (val.GetBit(0) ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
                return retVal;
            });

        /// <summary>
        /// Sets the dest bit of the src byte to true
        /// </summary>
        /// <remarks>Affected Flags: None.</remarks>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns>
        /// <see langword="true"/> if the operation is complete, <see langword="false"/> otherwise
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">step</exception>
        public static bool Set(Opcode code, GbMemory memory, int step) => Operate(code, memory, step, (mem, val, dest) => val.Set(dest));

        /// <summary>
        /// Shift left, bit 7 gets shifted into Carry.
        /// </summary>
        /// <remarks>
        /// Flags affected: Z is set if the result of the operation is zero. Reset N,H. C = bit 7 of
        /// The original number
        /// </remarks>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns>
        /// The state of the operation <see langword="true"/> if complete, <see langword="false"/> otherwise
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">step</exception>
        public static bool Sla(Opcode code, GbMemory memory, int step) =>
            Operate(code, memory, step, (mem, val, dest) =>
            {
                byte retVal = (byte)(val << 1);
                mem.R.F = (val.GetBit(7) ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
                return retVal;
            });

        public static bool Sra(Opcode code, GbMemory memory, int step) =>
            Operate(code, memory, step, (mem, val, dest) =>
            {
                byte retVal = (byte)((val >> 1) | (val & (1 << 7)));
                mem.R.F = (val.GetBit(0) ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
                return retVal;
            });

        public static bool Srl(Opcode code, GbMemory memory, int step) =>
            Operate(code, memory, step, (mem, val, dest) =>
            {
                byte retVal = (byte)(val >> 1);
                mem.R.F = (val.GetBit(0) ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
                return retVal;
            });

        public static bool Swap(Opcode code, GbMemory memory, int step) =>
            Operate(code, memory, step, (mem, val, dest) =>
            {
                byte retVal = (byte)((val << 4) | (val >> 4));
                mem.R.F = retVal == 0 ? RFlags.ZB : (byte)0;
                return retVal;
            });

        private static bool Operate(Opcode op, GbMemory memory, int step, Op operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            switch (step)
            {
                case 0:
                    if (op.Src == 6)
                    {
                        return false;
                    }

                    memory.R.SetR8(op.Src, operation(memory, memory.R.GetR8(op.Src), op.Dest));
                    return true;

                case 1:
                    op.Data1 = memory.GetMappedMemoryHl();
                    return false;

                case 2:
                    memory.SetMappedMemoryHl(operation(memory, op.Data1, op.Dest));
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }
    }
}
