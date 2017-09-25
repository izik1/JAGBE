namespace JAGBE.GB.Emulation
{
    internal sealed class Timer
    {
        private readonly GbMemory memory;
        private int prevTimaOverflow;

        /// <summary>
        /// The previous state of timer input
        /// </summary>
        private bool PrevTimerIn;

        /// <summary>
        /// The tac register
        /// </summary>
        private byte tac;

        /// <summary>
        /// The TIMA Value Register.
        /// </summary>
        private byte tima;

        /// <summary>
        /// Is a TIMA Interupt scheduled?
        /// </summary>
        private int timaOverflow;

        /// <summary>
        /// The TIMA Modulo Register
        /// </summary>
        private byte tma;

        internal Timer(GbMemory memory) => this.memory = memory;

        /// <summary>
        /// The system timer.
        /// </summary>
        private GbUInt16 sysTimer;

        public GbUInt8 this[byte index]
        {
            get
            {
                switch (index)
                {
                    case 4: return this.sysTimer.HighByte;
                    case 5: return this.tima;
                    case 6: return this.tma;
                    case 7: return (GbUInt8)(this.tac | 0xF8);
                    default: return GbUInt8.MaxValue;
                }
            }

            set
            {
                switch (index)
                {
                    case 4:
                        this.sysTimer = 0;
                        return;

                    case 5:
                        this.timaOverflow = 1;
                        this.tima = (byte)value;
                        return;

                    case 6:
                        this.tma = (byte)value;
                        return;

                    case 7:
                        this.tac = (byte)(value & 7);
                        return;

                    default:
                        return;
                }
            }
        }

        /// <summary>
        /// Updates the timer.
        /// </summary>
        /// <param name="TCycles">The number of clock cycles to run for.</param>
        internal void Update(int TCycles)
        {
            for (int i = 0; i < TCycles; i++)
            {
                Update();
            }
        }

        internal void Update()
        {
            if (this.timaOverflow > 0)
            {
                this.timaOverflow--;
                if (this.prevTimaOverflow != 0 && this.timaOverflow == 0)
                {
                    this.memory.IF |= 4;
                    this.tima = this.tma;
                }
            }

            this.prevTimaOverflow = this.timaOverflow;
            this.sysTimer++;
            bool b = (this.tac & 0b100) == 0b100 &&
                ((this.tac & 3) == 0 ? this.sysTimer.HighByte[1] : this.sysTimer.LowByte[(byte)(((this.tac & 3) * 2) + 1)]);
            if (this.PrevTimerIn && !b)
            {
                this.tima++;
                if (this.tima == 0)
                { // MCycle + 1 because TimaOverflow behaviour happens on the falling edge of this.
                    this.timaOverflow = Cpu.MCycle + 1;
                }
            }

            this.PrevTimerIn = b;
        }
    }
}
