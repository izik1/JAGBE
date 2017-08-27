using System;
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
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherise <see langword="false"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="step"/> is &gt; 1 or &lt; 0
        /// </exception>
        public static bool Bit(Opcode code, GbMemory memory, int step)
        {
            if (step == 0)
            {
                if (code.Src == 6)
                {
                    return false;
                }

                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, !memory.R.GetR8(code.Src)[(byte)code.Dest]).Res(RFlags.NF).Set(RFlags.HF);
                return true;
            }

            if (step == 1)
            {
                memory.R.F = memory.R.F.AssignBit(RFlags.ZF, !memory.GetMappedMemoryHl()[(byte)code.Dest]).Res(RFlags.NF).Set(RFlags.HF);
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        /// <summary>
        /// Resources the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherise <see langword="false"/></returns>
        public static bool Res(Opcode code, GbMemory memory, int step) => BitOpFunc(code, memory, step, (m, val, dest) => val.Res((byte)dest));

        /// <summary>
        /// Rls the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherise <see langword="false"/></returns>
        public static bool Rl(Opcode code, GbMemory memory, int step) => BitOpFunc(code, memory, step, (mem, val, dest) =>
        {
            byte retVal = (byte)((val << 1) | (mem.R.F[RFlags.CF] ? 1 : 0));
            mem.R.F = (val[7] ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
            return retVal;
        });

        /// <summary>
        /// RLCs the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherise <see langword="false"/></returns>
        public static bool Rlc(Opcode code, GbMemory memory, int step) => BitOpFunc(code, memory, step, (mem, val, dest) =>
        {
            byte retVal = (byte)((val << 1) | (val[7] ? 1 : 0));
            mem.R.F = (val[7] ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
            return retVal;
        });

        /// <summary>
        /// Rrs the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherise <see langword="false"/></returns>
        public static bool Rr(Opcode code, GbMemory memory, int step) => BitOpFunc(code, memory, step, (mem, val, dest) =>
        {
            byte retVal = (byte)((val >> 1) | (mem.R.F[RFlags.CF] ? 0x80 : 0));
            mem.R.F = (val[0] ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
            return retVal;
        });

        /// <summary>
        /// RRCs the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherise <see langword="false"/></returns>
        public static bool Rrc(Opcode code, GbMemory memory, int step) => BitOpFunc(code, memory, step, (mem, val, dest) =>
        {
            byte retVal = (byte)((val >> 1) | (val[0] ? 0x80 : 0));
            mem.R.F = (val[0] ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
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
        public static bool Set(Opcode code, GbMemory memory, int step) => BitOpFunc(code, memory, step, (mem, val, dest) => val.Set(dest));

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
        public static bool Sla(Opcode code, GbMemory memory, int step) => BitOpFunc(code, memory, step, (mem, val, dest) =>
        {
            byte retVal = (byte)(val << 1);
            mem.R.F = (val[7] ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
            return retVal;
        });

        /// <summary>
        /// Sras the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherise <see langword="false"/></returns>
        public static bool Sra(Opcode code, GbMemory memory, int step) => BitOpFunc(code, memory, step, (mem, val, dest) =>
        {
            byte retVal = (byte)((val >> 1) | (val & (1 << 7)));
            mem.R.F = (val[0] ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
            return retVal;
        });

        /// <summary>
        /// SRLs the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherise <see langword="false"/></returns>
        public static bool Srl(Opcode code, GbMemory memory, int step) => BitOpFunc(code, memory, step, (mem, val, dest) =>
        {
            byte retVal = (byte)(val >> 1);
            mem.R.F = (val[0] ? RFlags.CB : (byte)0).AssignBit(RFlags.ZF, retVal == 0);
            return retVal;
        });

        /// <summary>
        /// Swaps the specified code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherise <see langword="false"/></returns>
        public static bool Swap(Opcode code, GbMemory memory, int step) => BitOpFunc(code, memory, step, (mem, val, dest) =>
        {
            byte retVal = (byte)((val << 4) | (val >> 4));
            mem.R.F = retVal == 0 ? RFlags.ZB : (byte)0;
            return retVal;
        });
    }
}
