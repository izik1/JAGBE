﻿using System;

namespace JAGBE.GB.Computation
{
    internal sealed class Apu
    {
        private byte NR50;
        private byte NR51;
        private byte NR52;
        private readonly byte[] WavePattern = new byte[16];

        internal void Clear()
        {
            this.NR50 = 0;
            this.NR51 = 0;
        }

        internal byte GetRegister(byte num)
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
                    Console.WriteLine("Possible bad Read from ALU 0x" + num.ToString("X2") + " (reg number)");
                    return 0xFF;
            }
        }

        internal bool SetRegister(byte num, byte value)
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
                    if (!this.NR52.GetBit(7))
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
