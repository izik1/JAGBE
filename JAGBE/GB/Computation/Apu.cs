using JAGBE.GB.DataTypes;
using JAGBE.Logging;

namespace JAGBE.GB.Computation
{
    internal sealed class Apu
    {
        private GbUInt8 NR10;
        private GbUInt8 NR11;
        private GbUInt8 NR12;
        private GbUInt8 NR13;
        private GbUInt8 NR14;
        private GbUInt8 NR21;
        private GbUInt8 NR22;
        private GbUInt8 NR23;
        private GbUInt8 NR24;
        private GbUInt8 NR30;
        private GbUInt8 NR31;
        private GbUInt8 NR32;
        private GbUInt8 NR33;
        private GbUInt8 NR34;
        private GbUInt8 NR41;
        private GbUInt8 NR42;
        private GbUInt8 NR43;
        private GbUInt8 NR44;
        private GbUInt8 NR50;
        private GbUInt8 NR51;
        private GbUInt8 NR52;
        private readonly GbUInt8[] WavePattern = new GbUInt8[16];

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

        internal GbUInt8 GetRegister(GbUInt8 num)
        {
            if (num < 0x10 || num > 0x3F)
            {
                return 0xFF;
            }

            if (num >= 0x30)
            {
                return this.WavePattern[num - 0x30];
            }

            switch (num)
            {
                case 0x10:
                    return this.NR10;

                case 0x11:
                    return this.NR11 | 0x3F;

                case 0x12:
                    return this.NR12;

                case 0x13:
                    return this.NR13 | 0xFF;

                case 0x14:
                    return this.NR14 | 0x87;

                case 0x16:
                    return this.NR21 | 0x3F;

                case 0x17:
                    return this.NR22;

                case 0x18:
                    return this.NR23 | 0xFF;

                case 0x19:
                    return this.NR24 | 0x87;

                case 0x1A:
                    return this.NR30 | 0x7F;

                case 0x1B:
                    return this.NR31;

                case 0x1C:
                    return this.NR32 | 0x60;

                case 0x1D:
                    return this.NR33 | 0xFF;

                case 0x1E:
                    return this.NR34 | 0x87;

                case 0x20:
                    return this.NR41 | 0x3F;

                case 0x21:
                    return this.NR42;

                case 0x22:
                    return this.NR43;

                case 0x23:
                    return this.NR44 | 0xBF;

                case 0x24:
                    return this.NR50;

                case 0x25:
                    return this.NR51;

                case 0x26:
                    return (byte)(this.NR52 | 0x70);

                default:
                    Logger.LogWarning("Possible bad Read from ALU 0x" + num.ToString("X2") + " (reg number)");
                    return 0xFF;
            }
        }

        internal bool SetRegister(byte num, GbUInt8 value)
        {
            if (num < 0x10 || num > 0x3F)
            {
                return false;
            }

            if (num >= 0x30)
            {
                this.WavePattern[num - 0x30] = value;
                return true;
            }

            switch (num)
            {
                case 0x10:
                    this.NR10 = value;
                    return true;

                case 0x11:
                    this.NR11 = value;
                    return true;

                case 0x12:
                    this.NR12 = value;
                    return true;

                case 0x13:
                    this.NR13 = value;
                    return true;

                case 0x14:
                    this.NR14 = value;
                    return true;

                case 0x16:
                    this.NR21 = value;
                    return true;

                case 0x17:
                    this.NR22 = value;
                    return true;

                case 0x18:
                    this.NR23 = value;
                    return true;

                case 0x19:
                    this.NR24 = value;
                    return true;

                case 0x1A:
                    this.NR30 = value & 0x80;
                    return true;

                case 0x1B:
                    this.NR31 = value;
                    return true;

                case 0x1C:
                    this.NR32 = value & 0x60;
                    return true;

                case 0x1D:
                    this.NR33 = value;
                    return true;

                case 0x1E:
                    this.NR34 = value;
                    return true;

                case 0x20:
                    this.NR41 = value & 0x3F;
                    return true;

                case 0x21:
                    this.NR42 = value;
                    return true;

                case 0x22:
                    this.NR43 = value;
                    return true;

                case 0x23:
                    this.NR44 = (value & 0xC0);
                    return true;

                case 0x24:
                    this.NR50 = value;
                    return true;

                case 0x25:
                    this.NR51 = value;
                    return true;

                case 0x26:
                    this.NR52 = (byte)((value & 0x80) | (this.NR52 & 0x7F));
                    if ((value & 0x80) == 0)
                    {
                        Clear();
                    }

                    return true;

                default:
                    return false;
            }
        }
    }
}
