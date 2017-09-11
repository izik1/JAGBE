using System;
using System.IO;

namespace JAGBE.GB.Emulation
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
        private bool TimaOverflow;

        private bool PrevTimaOverflow;

        /// <summary>
        /// The system timer.
        /// </summary>
        private GbUInt16 sysTimer = 0;

        /// <summary>
        /// The tac register
        /// </summary>
        private GbUInt8 Tac;

        /// <summary>
        /// The TIMA Modulo Register
        /// </summary>
        private GbUInt8 Tma;

        /// <summary>
        /// The TIMA Value Register.
        /// </summary>
        private GbUInt8 Tima;

        /// <summary>
        /// Gets the system timer.
        /// </summary>
        /// <value>The system timer.</value>
        public GbUInt16 SysTimer => this.sysTimer;

        public GbUInt8 this[byte index]
        {
            get
            {
                switch (index)
                {
                    case 4: return this.sysTimer.HighByte;
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
                        this.sysTimer = 0;
                        return;

                    case 5:
                        this.TimaOverflow = false;
                        this.Tima = value;
                        return;

                    case 6:
                        this.Tma = value;
                        return;

                    case 7:
                        this.Tac = (value & 7);
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
        internal void Update(GbMemory memory)
        {
            if (this.PrevTimaOverflow)
            {
                memory.IF |= 4;
                this.Tima = this.Tma;
            }

            this.PrevTimaOverflow = this.TimaOverflow;
            this.TimaOverflow = false;
            this.sysTimer += (GbUInt16)Cpu.DelayStep;

            bool b = this.Tac[2] &&
                ((this.Tac & 3) == 0 ? this.sysTimer.HighByte[1] : this.sysTimer.LowByte[(byte)((((int)this.Tac & 3) * 2) + 1)]);
            if (this.PrevTimerIn && !b)
            {
                this.TimaOverflow = this.Tima == 0xFF;
                if (!this.PrevTimaOverflow)
                {
                    this.Tima++;
                }
            }

            this.PrevTimerIn = b;
        }

        internal void SaveState(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(this.PrevTimerIn);
            binaryWriter.Write(this.TimaOverflow);
            binaryWriter.Write(this.PrevTimaOverflow);
            binaryWriter.Write((ushort)this.sysTimer);
            binaryWriter.Write((byte)this.Tac);
            binaryWriter.Write((byte)this.Tma);
            binaryWriter.Write((byte)this.Tima);
        }
    }
}
