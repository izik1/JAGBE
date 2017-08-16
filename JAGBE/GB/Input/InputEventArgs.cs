using System;

namespace JAGBE.GB.Input
{
    internal sealed class InputEventArgs : EventArgs
    {
        public readonly byte value;

        public InputEventArgs(byte val) => this.value = val;
    }
}
