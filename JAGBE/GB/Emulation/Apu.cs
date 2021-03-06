using JAGBE.Logging;

namespace JAGBE.GB.Emulation
{
    internal sealed class Apu
    {
        private byte NR10;
        private byte NR11;
        private byte NR12;
        private byte NR13;
        private byte NR14;
        private byte NR21;
        private byte NR22;
        private byte NR23;
        private byte NR24;
        private byte NR30;
        private byte NR31;
        private byte NR32;
        private byte NR33;
        private byte NR34;
        private byte NR41;
        private byte NR42;
        private byte NR43;
        private byte NR44;
        private byte NR50;
        private byte NR51;
        private byte NR52;
        private readonly byte[] WavePattern = new byte[16];

        public byte this[byte index]
        {
            get
            {
                if (index < 0x10 || index > 0x3F)
                {
                    return 0xFF;
                }

                if (index >= 0x30)
                {
                    return this.WavePattern[index - 0x30];
                }

                switch (index)
                {
                    case 0x10: return (byte)(this.NR10 | 0x80);
                    case 0x11: return (byte)(this.NR11 | 0x3F);
                    case 0x12: return this.NR12;
                    case 0x13: return (byte)(this.NR13 | 0xFF);
                    case 0x14: return (byte)(this.NR14 | 0x87);
                    case 0x16: return (byte)(this.NR21 | 0x3F);
                    case 0x17: return this.NR22;
                    case 0x18: return (byte)(this.NR23 | 0xFF);
                    case 0x19: return (byte)(this.NR24 | 0x87);
                    case 0x1A: return (byte)(this.NR30 | 0x7F);
                    case 0x1B: return this.NR31;
                    case 0x1C: return (byte)(this.NR32 | 0x9F);
                    case 0x1D: return (byte)(this.NR33 | 0xFF);
                    case 0x1E: return (byte)(this.NR34 | 0x87);
                    case 0x20: return (byte)(this.NR41 | 0xC0);
                    case 0x21: return this.NR42;
                    case 0x22: return this.NR43;
                    case 0x23: return (byte)(this.NR44 | 0xBF);
                    case 0x24: return this.NR50;
                    case 0x25: return this.NR51;
                    case 0x26: return (byte)(this.NR52 | 0x70);
                    default:
                        Logger.LogWarning("Possible bad Read from ALU 0x" + index.ToString("X2") + " (reg number)");
                        return 0xFF;
                }
            }

            set
            {
                if (!this.NR52.GetBit(7) && index != 0x20 && index != 0x26)
                {
                    return;
                }

                if (index < 0x10 || index > 0x3F)
                {
                    return;
                }

                if (index >= 0x30)
                {
                    this.WavePattern[index - 0x30] = value;
                    return;
                }

                switch (index)
                {
                    case 0x10:
                        this.NR10 = (byte)(value & 0x7F);
                        return;

                    case 0x11:
                        this.NR11 = value;
                        return;

                    case 0x12:
                        this.NR12 = value;
                        return;

                    case 0x13:
                        this.NR13 = value;
                        return;

                    case 0x14:
                        this.NR14 = value;
                        return;

                    case 0x16:
                        this.NR21 = value;
                        return;

                    case 0x17:
                        this.NR22 = value;
                        return;

                    case 0x18:
                        this.NR23 = value;
                        return;

                    case 0x19:
                        this.NR24 = value;
                        return;

                    case 0x1A:
                        this.NR30 = (byte)(value & 0x80);
                        return;

                    case 0x1B:
                        this.NR31 = value;
                        return;

                    case 0x1C:
                        this.NR32 = (byte)(value & 0x60);
                        return;

                    case 0x1D:
                        this.NR33 = value;
                        return;

                    case 0x1E:
                        this.NR34 = value;
                        return;

                    case 0x20:
                        this.NR41 = (byte)(value & 0x3F);
                        return;

                    case 0x21:
                        this.NR42 = value;
                        return;

                    case 0x22:
                        this.NR43 = value;
                        return;

                    case 0x23:
                        this.NR44 = (byte)(value & 0xC0);
                        return;

                    case 0x24:
                        this.NR50 = value;
                        return;

                    case 0x25:
                        this.NR51 = value;
                        return;

                    case 0x26:
                        this.NR52 = (byte)((value & 0x80) | (this.NR52 & 0x7F));
                        if (!value.GetBit(7))
                        {
                            Clear();
                        }

                        return;

                    default:
                        return;
                }
            }
        }

        /// <summary>
        /// Sets all registers to zero.
        /// </summary>
        internal void Clear()
        {
            this.NR10 = 0;
            this.NR11 = 0;
            this.NR12 = 0;
            this.NR13 = 0;
            this.NR14 = 0;
            this.NR21 = 0;
            this.NR22 = 0;
            this.NR23 = 0;
            this.NR24 = 0;
            this.NR30 = 0;
            this.NR31 = 0;
            this.NR32 = 0;
            this.NR33 = 0;
            this.NR34 = 0;
            this.NR41 = 0;
            this.NR42 = 0;
            this.NR43 = 0;
            this.NR44 = 0;
            this.NR50 = 0;
            this.NR51 = 0;
        }
    }
}
