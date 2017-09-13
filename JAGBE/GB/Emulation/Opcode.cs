using System;

namespace JAGBE.GB.Emulation
{
    internal sealed class Opcode
    {
        /// <summary>
        /// The destination operand.
        /// </summary>
        internal readonly GbUInt8 Dest;

        /// <summary>
        /// The source operand.
        /// </summary>
        internal readonly GbUInt8 Src;

        /// <summary>
        /// The function.
        /// </summary>
        private readonly OpcodeFunc function;

        /// <summary>
        /// Initializes a new instance of the <see cref="Opcode"/> class.
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="src">The source.</param>
        /// <param name="onInvoke">The on invoke.</param>
        /// <exception cref="ArgumentNullException">onInvoke</exception>
        public Opcode(byte dest, byte src, OpcodeFunc onInvoke)
        {
            this.Dest = dest;
            this.Src = src;
            this.function = onInvoke ?? throw new ArgumentNullException(nameof(onInvoke));
        }

        /// <summary>
        /// Invokes the <see cref="function"/> of this instance with the given arguments.
        /// </summary>
        /// <param name="memory">The memory.</param>
        public int Invoke(GbMemory memory) => this.function(this, memory);
    }
}
