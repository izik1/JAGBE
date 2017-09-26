namespace JAGBE.GB.Emulation
{
    /// <summary>
    /// Represents The Game Boy DMG Timer subsystem.
    /// </summary>
    internal sealed class Timer
    {
        /// <summary>
        /// A reference to the bound memory.
        /// </summary>
        /// <remarks>This field is for preformance.</remarks>
        private readonly GbMemory memory;

        /// <summary>
        /// The value of <see cref="timaOverflow"/> 1 tick ago.
        /// </summary>
        private int prevTimaOverflow;

        /// <summary>
        /// The previous state of timer input.
        /// </summary>
        /// <remarks>
        /// <see cref="tac"/> increments on the falling edge of this. This field is <see
        /// langword="true"/> if <see cref="tac"/> bit 2 is 1 and depending on <see cref="tac"/> bits
        /// 1-0 <see cref="tac"/> for more informantion.
        /// </remarks>
        /// <seealso cref="tac"/>
        private bool prevTimerIn;

        /// <summary>
        /// The timer control register.
        /// </summary>
        /// <remarks>
        /// Lower 3 bits are R/W upper 5 bits are fixed to 1. Bit 2 enables the timer, bits 1-0
        /// control the frequency. The way the frequency works is by selecting a bit of <see
        /// cref="sysTimer"/> if <see cref="tac"/> bits 1-0 is 0b00 then it selects bit 9, 0b01
        /// selects bit 3, 0b10 bit 5 and 0b11 selects bit 7.
        /// </remarks>
        private byte tac;

        /// <summary>
        /// The TIMA counter register.
        /// </summary>
        /// <remarks>
        /// Increments whenever <see cref="prevTimerIn"/> is <see langword="true"/> but <see
        /// cref="tac"/> selection is <see langword="false"/>. On overflow has an M-Cycle gap before
        /// being reloaded with <see cref="tma"/> any writes to this register during that time cancel
        /// the overflow and prevent <see cref="GbMemory.IF"/> flag from being set, however a write
        /// during the M-Cycle after said gap will be ignored.
        /// </remarks>
        /// <seealso cref="Timer"/>
        private byte tima;

        /// <summary>
        /// Stores the number of cycles until <see cref="tima"/> gets reloaded during an overflow.
        /// </summary>
        private int timaOverflow;

        /// <summary>
        /// The value loaded into <see cref="tima"/> when it overflows.
        /// </summary>
        /// <remarks>
        /// If this register is written to while it is loading into <see cref="tima"/> the value of
        /// the write will be written to both this register and <see cref="tima"/>.
        /// </remarks>
        /// <seealso cref="timaOverflow"/>
        private byte tma;

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer"/> class.
        /// </summary>
        /// <param name="memory">The memory.</param>
        internal Timer(GbMemory memory) => this.memory = memory;

        /// <summary>
        /// The DMG's internal counter register.
        /// </summary>
        /// <remarks>
        /// Always increments by one every tick. Reading from 0xFF04 with return the upper 8 bits of
        /// this register. Writing to 0xFF04 will reset this register to 0.
        /// </remarks>
        private GbUInt16 sysTimer;

        public byte this[byte index]
        {
            get
            {
                switch (index)
                {
                    case 4: return this.sysTimer.HighByte;
                    case 5: return this.tima;
                    case 6: return this.tma;
                    case 7: return (byte)(this.tac | 0xF8);
                    default: return byte.MaxValue;
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
                        this.tima = value;
                        return;

                    case 6:
                        this.tma = value;
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
        /// Ticks the internal timer and handles other behaviour.
        /// </summary>
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
            bool b = (this.tac & 0b100) == 0b100 && (this.tac == 0b100 ? this.sysTimer.HighByte.GetBit(1) :
                (this.sysTimer & (1 << (((this.tac & 3) * 2) + 1))) > 0);
            if (this.prevTimerIn && !b)
            {
                this.tima++;
                if (this.tima == 0)
                {   // MCycle + 1 because TimaOverflow behaviour happens on the falling edge of this.
                    this.timaOverflow = Cpu.MCycle + 1;
                }
            }

            this.prevTimerIn = b;
        }
    }
}
