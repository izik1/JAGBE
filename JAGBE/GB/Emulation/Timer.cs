namespace JAGBE.GB.Emulation
{
    internal sealed class Timer
    {
        private int PrevTimaOverflow;

        /// <summary>
        /// The previous state of timer input
        /// </summary>
        private bool PrevTimerIn;

        /// <summary>
        /// The system timer.
        /// </summary>
        internal GbUInt16 SysTimer { get; private set; } = 0;

        /// <summary>
        /// The tac register
        /// </summary>
        private GbUInt8 Tac;

        /// <summary>
        /// The TIMA Value Register.
        /// </summary>
        private GbUInt8 Tima;

        /// <summary>
        /// Is a TIMA Interupt scheduled?
        /// </summary>
        private int TimaOverflow;

        /// <summary>
        /// The TIMA Modulo Register
        /// </summary>
        private GbUInt8 Tma;

        public GbUInt8 this[byte index]
        {
            get
            {
                switch (index)
                {
                    case 4: return this.SysTimer.HighByte;
                    case 5: return this.Tima;
                    case 6: return this.Tma;
                    case 7: return this.Tac | 0xF8;
                    default: return 0xFF;
                }
            }

            set
            {
                switch (index)
                {
                    case 4:
                        this.SysTimer = 0;
                        return;

                    case 5:
                        this.TimaOverflow = 0;
                        this.Tima = value;
                        return;

                    case 6:
                        this.Tma = value;
                        return;

                    case 7:
                        this.Tac = (GbUInt8)(value & 7);
                        return;

                    default:
                        return;
                }
            }
        }

        /// <summary>
        /// Updates the timer.
        /// </summary>
        /// <param name="memory">The memory.</param>
        /// <param name="TCycles">The number of clock cycles to run for.</param>
        internal void Update(GbMemory memory, int TCycles)
        {
            for (int i = 0; i < TCycles; i++)
            {
                Update(memory);
            }
        }

        internal void Update(GbMemory memory)
        {
            if (this.TimaOverflow > 0)
            {
                this.TimaOverflow--;
            }

            if (this.PrevTimaOverflow != 0 && this.TimaOverflow == 0)
            {
                memory.IF |= 4;
                this.Tima = this.Tma;
            }

            this.PrevTimaOverflow = this.TimaOverflow;
            this.SysTimer++;
            bool b = this.Tac[2] &&
                ((this.Tac & 3) == 0 ? this.SysTimer.HighByte[1] : this.SysTimer.LowByte[(byte)(((this.Tac & 3) * 2) + 1)]);
            if (this.PrevTimerIn && !b)
            {
                this.Tima++;
                if (this.Tima == GbUInt8.MinValue)
                { // MCycle + 1 because TimaOverflow behaviour happens on the falling edge of this.
                    this.TimaOverflow = Cpu.MCycle + 1;
                }
            }

            this.PrevTimerIn = b;
        }
    }
}
