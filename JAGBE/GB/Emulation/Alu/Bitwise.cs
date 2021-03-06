using static JAGBE.GB.Emulation.Alu.Ops;

namespace JAGBE.GB.Emulation.Alu
{
    /// <summary>
    /// This class contains all of the emulated Cpu's Bitwise operations.
    /// </summary>
    internal static class Bitwise
    {
        /// <summary>
        /// Checks if the dest bit of the src register is true.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        public static int Bit(Opcode code, GbMemory memory)
        {
            if (code.Src != 6)
            {
                memory.R.F = (byte)(memory.R.F.AssignBit(RFlags.ZF, !memory.R.GetR8(code.Src).GetBit(code.Dest))
                    .Res(RFlags.NF) | RFlags.HB);
                return 1;
            }

            memory.R.F = (byte)(memory.R.F.AssignBit(RFlags.ZF, !memory.ReadCycleHl().GetBit(code.Dest))
                .Res(RFlags.NF) | RFlags.HB);
            return 2;
        }

        /// <summary>
        /// Resources the specified code.
        /// </summary>
        /// <param name="opcode">The opcode.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        public static int Res(Opcode opcode, GbMemory memory)
        {
            if (opcode.Src != 6)
            {
                memory.R.SetR8(opcode.Src, memory.R.GetR8(opcode.Src).Res(opcode.Dest));
                return 1;
            }

            memory.WriteCycleHl(memory.ReadCycleHl().Res(opcode.Dest));
            return 3;
        }

        /// <summary>
        /// Rls the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        public static int Rl(Opcode code, GbMemory memory) => BitOpFunc(code, memory, (mem, val) =>
        {
            byte retVal = (byte)((val << 1) | ((mem.R.F & RFlags.CB) >> RFlags.CF));
            mem.R.F = (byte)(((val & 0x80) >> (7 - RFlags.CF)) | (retVal == 0 ? RFlags.ZB : 0));
            return retVal;
        });

        /// <summary>
        /// RLCs the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        public static int Rlc(Opcode code, GbMemory memory) => BitOpFunc(code, memory, (mem, val) =>
        {
            byte retVal = (byte)((val << 1) | (val >> 7));
            mem.R.F = (byte)(((val & 0x80) >> (7 - RFlags.CF)) | (retVal == 0 ? RFlags.ZB : 0));
            return retVal;
        });

        /// <summary>
        /// Rrs the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        public static int Rr(Opcode code, GbMemory memory) => BitOpFunc(code, memory, (mem, val) =>
        {
            byte retVal = (byte)((val >> 1) | (mem.R.F.GetBit(RFlags.CF) ? 0x80 : 0));
            mem.R.F = (byte)(((val & 1) << RFlags.CF) | (retVal == 0 ? RFlags.ZB : 0));
            return retVal;
        });

        /// <summary>
        /// RRCs the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        public static int Rrc(Opcode code, GbMemory memory) => BitOpFunc(code, memory, (mem, val) =>
        {
            byte retVal = (byte)((val >> 1) | (val << 7));
            mem.R.F = (byte)(((val & 1) << RFlags.CF) | (retVal == 0 ? RFlags.ZB : 0));
            return retVal;
        });

        /// <summary>
        /// Sets the dest bit of the src byte to true
        /// </summary>
        /// <param name="opcode">The opcode.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>Affected Flags: None.</remarks>
        public static int Set(Opcode opcode, GbMemory memory)
        {
            if (opcode.Src != 6)
            {
                memory.R.SetR8(opcode.Src, (byte)(memory.R.GetR8(opcode.Src) | (1 << opcode.Dest)));
                return 1;
            }

            memory.WriteCycleHl((byte)(memory.ReadCycleHl() | (1 << opcode.Dest)));
            return 3;
        }

        /// <summary>
        /// Shift left, bit 7 gets shifted into Carry.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        /// <remarks>
        /// Flags affected: Z is set if the result of the operation is zero. Reset N,H. C = bit 7 of
        /// The original number
        /// </remarks>
        public static int Sla(Opcode code, GbMemory memory) => BitOpFunc(code, memory, (mem, val) =>
        {
            byte retVal = (byte)(val << 1);
            mem.R.F = (byte)(((val & 0x80) >> (7 - RFlags.CF)) | (retVal == 0 ? RFlags.ZB : 0));
            return retVal;
        });

        /// <summary>
        /// Sras the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        public static int Sra(Opcode code, GbMemory memory) => BitOpFunc(code, memory, (mem, val) =>
        {
            byte retVal = (byte)((val >> 1) | (val & 0x80));
            mem.R.F = (byte)(((val & 1) << RFlags.CF) | (retVal == 0 ? RFlags.ZB : 0));
            return retVal;
        });

        /// <summary>
        /// SRLs the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        public static int Srl(Opcode code, GbMemory memory) => BitOpFunc(code, memory, (mem, val) =>
        {
            int retVal = val >> 1;
            mem.R.F = (byte)(((val & 1) << RFlags.CF) | (retVal == 0 ? RFlags.ZB : 0));
            return (byte)retVal;
        });

        /// <summary>
        /// Swaps the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <returns>The number of ticks the operation took to complete.</returns>
        public static int Swap(Opcode code, GbMemory memory) => BitOpFunc(code, memory, (mem, val) =>
        {
            mem.R.F = val == 0 ? RFlags.ZB : byte.MinValue;
            return (byte)((val << 4) | val >> 4);
        });
    }
}
