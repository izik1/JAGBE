using System;
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
            this.NR50 = 0;
            this.NR51 = 0;
        }

        internal GbUInt8 GetRegister(GbUInt8 num)
        {
            if (num < 0x10 || num > 0x3F)
            {
                return 0xFF;
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
