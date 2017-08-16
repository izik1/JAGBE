using System;

namespace JAGBE.GB.Input
{
    internal interface IInputHandler
    {
        event EventHandler<InputEventArgs> OnInput;
    }
}
