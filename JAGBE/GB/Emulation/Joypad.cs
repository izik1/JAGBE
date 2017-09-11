using JAGBE.GB.Input;

namespace JAGBE.GB.Emulation
{
    internal sealed class Joypad
    {
        /// <summary>
        /// The joypad status register.
        /// </summary>
        internal GbUInt8 Status;

        /// <summary>
        /// The keys of the joypad
        /// </summary>
        private byte keys = 0xFF;

        /// <summary>
        /// The previous state of <see cref="keys"/>
        /// </summary>
        private byte prevKeys = 0xFF;

        internal Joypad() : this(null)
        {
        }

        internal Joypad(IInputHandler inputHandler)
        {
            // Null is valid input to this constructor since this just subscribes itself to the event
            // handler if it exists.
            if (inputHandler != null)
            {
                inputHandler.OnInput += this.OnInput;
            }
        }

        internal byte Pad => GetJoypad(this.keys);

        /// <summary>
        /// Updates the key state.
        /// </summary>
        /// <param name="memory">The memory.</param>
        internal void Update(GbMemory memory)
        {
            if (((GetJoypad(this.prevKeys) & 0xF) == 0xF) && (GetJoypad(this.keys) & 0xF) != 0xF)
            {
                memory.IF |= 0x10;
            }
        }

        /// <summary>
        /// Gets the value of the joypad.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <returns>The value of the joypad.</returns>
        private byte GetJoypad(byte p1) =>
            (byte)((!this.Status[5] ? (p1 & 0xF) : !this.Status[4] ? ((p1 >> 4) & 0xF) : 0xFF) | 0xC0);

        /// <summary>
        /// Called when input is recieved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="InputEventArgs"/> instance containing the event data.</param>
        private void OnInput(object sender, InputEventArgs e)
        {
            this.prevKeys = this.keys;
            this.keys = e.value;
        }
    }
}
