using JAGBE.GB.DataTypes;

namespace JAGBE.GB.Computation
{
    internal sealed class Timer
    {
        /// <summary>
        /// The previous state of timer input
        /// </summary>
        private bool PrevTimerIn;

        /// <summary>
        /// Is a TIMA Interupt scheduled?
        /// </summary>
        private bool ScheduleTimaInterupt;

        /// <summary>
        /// The system timer.
        /// </summary>
        private GbUInt16 sysTimer = 0;

        /// <summary>
        /// The tac register
        /// </summary>
        private byte Tac;

        /// <summary>
        /// The TIMA Modulo Register
        /// </summary>
        private byte TimaM;

        /// <summary>
        /// The TIMA Value Register.
        /// </summary>
        private byte TimaV;

        /// <summary>
        /// Gets the system timer.
        /// </summary>
        /// <value>The system timer.</value>
        public GbUInt16 SysTimer => this.sysTimer;

        internal byte GetRegister(byte number)
        {
            if (number < 4 || number > 7)
            {
                return 0xFF;
            }

            switch (number)
            {
                case 4:
                    return this.sysTimer.HighByte;

                case 5:
                    return this.TimaV;

                case 6:
                    return this.TimaM;

                default:
                    return this.Tac;
            }
        }

        internal void SetRegister(byte pointer, byte value)
        {
            if (pointer < 4 || pointer > 7)
            {
                return;
            }

            switch (pointer)
            {
                case 4:
                    this.sysTimer = 0;
                    break;

                case 5:
                    this.TimaV = value;
                    break;

                case 6:
                    this.TimaM = value;
                    break;

                default:
                    this.Tac = (byte)(value & 3);
                    break;
            }
        }

        /// <summary>
        /// Updates the timer.
        /// </summary>
        /// <param name="memory">The memory.</param>
        internal void Update(GbMemory memory)
        {
            if (this.ScheduleTimaInterupt)
            {
                memory.IF |= 4;
                this.TimaV = this.TimaM;
            }

            this.sysTimer += (GbUInt16)Cpu.DelayStep;
            bool divBit = (this.Tac.GetBit(0) && this.sysTimer.HighByte.GetBit(1)) || this.sysTimer.LowByte.GetBit((byte)(((this.Tac & 3) * 2) + 3));
            bool b = this.Tac.GetBit(1) && divBit;
            if (this.PrevTimerIn && !b)
            {
                bool bt = this.ScheduleTimaInterupt;
                this.ScheduleTimaInterupt = this.TimaV == 0xFF;
                if (!bt)
                {
                    this.TimaV++;
                }
            }

            this.PrevTimerIn = b;
        }
    }
}
