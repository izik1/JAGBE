using System;

namespace JAGBE.GB.Computation
{
    internal sealed class Opcode
    {
        internal byte data1;
        internal byte data2;
        internal readonly byte Dest;
        internal readonly byte Src;

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
