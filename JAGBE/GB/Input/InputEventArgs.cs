using System;

namespace JAGBE.GB.Input
{
    /// <summary>
    /// Represents data for an event triggered by input.
    /// </summary>
    /// <seealso cref="EventArgs"/>
    internal sealed class InputEventArgs : EventArgs
    {
        /// <summary>
        /// The value of this instance
        /// </summary>
        public readonly byte Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputEventArgs"/> class.
        /// </summary>
        /// <param name="val">The value.</param>
        public InputEventArgs(byte val) => Value = val;
    }
}
