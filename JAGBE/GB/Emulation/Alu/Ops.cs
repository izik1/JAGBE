using System;

namespace JAGBE.GB.Emulation.Alu
{
    /// <summary>
    /// Provides static methods for Cpu operations that follow certain patterns.
    /// </summary>
    internal static class Ops
    {
        /// <summary>
        /// A delegate for Bitwise operations.
        /// </summary>
        /// <param name="mem">The memory.</param>
        /// <param name="valIn">The input value.</param>
        /// <param name="dest">The dest.</param>
        /// <returns>The result of the operation</returns>
        internal delegate GbUInt8 BitOp(GbMemory mem, GbUInt8 valIn, GbUInt8 dest);

        /// <summary>
        /// A delegate for Arithmetic operations.
        /// </summary>
        /// <param name="mem">The memory.</param>
        /// <param name="valIn">The input value.</param>
        internal delegate void ArithOp8(GbMemory mem, GbUInt8 valIn);

        /// <summary>
        /// A framework for calling bitwise operation instructions.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <param name="operation">The operation.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="step"/> is &gt; 2</exception>
        internal static bool BitOpFunc(Opcode op, GbMemory memory, int step, BitOp operation)
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

        /// <summary>
        /// A framework for calling arithmetic operation instructions.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="step">The step.</param>
        /// <param name="operation">The operation.</param>
        /// <returns><see langword="true"/> if the operation has completed, otherise <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="step"/> is &gt; 1</exception>
        internal static bool ArithOp8Func(Opcode op, GbMemory memory, int step, ArithOp8 operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (step == 0)
            {
                if (op.Src == 6 || op.Src == 8)
                {
                    return false;
                }

                operation(memory, memory.R.GetR8(op.Src));
                return true;
            }

            if (step == 1)
            {
                operation(memory, op.Src == 6 ? memory.GetMappedMemoryHl() : memory.LdI8());
                return true;
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }
    }
}
