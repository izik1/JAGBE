using System;

namespace JAGBE.GB.Emulation
{
    internal sealed class Opcode
    {
        internal readonly GbUInt8 Dest;
        internal readonly GbUInt8 Src;

        private readonly OpcodeFunc function;

        public Opcode(byte dest, byte src, OpcodeFunc onInvoke)
        {
            this.Dest = dest;
            this.Src = src;
            this.function = onInvoke ?? throw new ArgumentNullException(nameof(onInvoke));
        }

        private Opcode()
        {
        }

        public bool Invoke(GbMemory memory, int stepNumber) => this.function(this, memory, stepNumber);
    }
}
