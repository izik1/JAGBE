using System;

namespace JAGBE.GB.Emulation.Alu
{
    /// <summary>
    /// Provides static methods for Cpu operations that follow certain patterns.
    /// </summary>
    internal static class Ops
    {
        /// <summary>
        /// A delegate for Arithmetic operations.
        /// </summary>
        /// <param name="mem">The memory.</param>
        /// <param name="valIn">The input value.</param>
        internal delegate void ArithOp8(GbMemory mem, byte valIn);

        /// <summary>
        /// A delegate for Bitwise operations.
        /// </summary>
        /// <param name="mem">The memory.</param>
        /// <param name="valIn">The input value.</param>
        /// <returns>The result of the operation</returns>
        internal delegate byte BitOp(GbMemory mem, byte valIn);

        /// <summary>
        /// A framework for calling arithmetic operation instructions.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="operation">The operation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null</exception>
        internal static int ArithOp8Func(Opcode op, GbMemory memory, ArithOp8 operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (op.Src != 6 && op.Src != 8)
            {
                operation(memory, memory.R.GetR8(op.Src));
                return 1;
            }

            operation(memory, op.Src == 6 ? memory.ReadCycleHl() : memory.ReadCycleI8());
            return 2;
        }

        /// <summary>
        /// A framework for calling bitwise operation instructions.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="memory">The memory.</param>
        /// <param name="operation">The operation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="operation"/> is null</exception>
        internal static int BitOpFunc(Opcode op, GbMemory memory, BitOp operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (op.Src != 6)
            {
                memory.R.SetR8(op.Src, operation(memory, memory.R.GetR8(op.Src)));
                return 1;
            }

            byte val = memory.ReadCycleHl();
            memory.Update();
            memory.SetMappedMemoryHl(operation(memory, val));
            return 3;
        }
    }
}
