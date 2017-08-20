using System;

namespace JAGBE.GB.Input
{
    /// <summary>
    /// An interface allows for generic registered input functions
    /// </summary>
    internal interface IInputHandler
    {
        /// <summary>
        /// Occurs when Input is recieved.
        /// </summary>
        event EventHandler<InputEventArgs> OnInput;
    }
}
