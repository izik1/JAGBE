using System;
using JAGBE.GB.DataTypes;
using JAGBE.Logging;

namespace JAGBE.GB.Computation
{
    internal sealed class Apu
    {
        private GbUInt8 NR50;
        private GbUInt8 NR51;
        private GbUInt8 NR52;
        private readonly GbUInt8[] WavePattern = new GbUInt8[16];

        internal void Clear()
        {
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
                case 0x24:
                    return this.NR50;

                case 0x25:
                    return this.NR51;

                case 0x26:
                    return (byte)(this.NR52 | 0x70);

                default:
                    Logger.LogLine(7, "Possible bad Read from ALU 0x" + num.ToString("X2") + " (reg number)");
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
