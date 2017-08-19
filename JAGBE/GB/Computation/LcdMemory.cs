using System;

namespace JAGBE.GB.Computation
{
    internal sealed class LcdMemory
    {
        public byte BgPallet;
        public int DMA = Cpu.DelayStep * 162;
        public ushort DMAAddress;
        public byte DMAValue;
        internal int cy;
        internal int[] displayMemory = new int[Lcd.Width * Lcd.Height];
        internal bool ForceNullRender;
        internal bool IRC;
        internal byte LY;
        internal byte LYC;
        internal byte ObjAttriMemOffset;
        internal byte objPallet0;
        internal byte objPallet1;
        internal bool PIRC;
        internal byte SCX;
        internal byte SCY;
        internal byte STAT;
        internal byte WX;
        internal byte WY;
        internal byte Lcdc;

        internal byte GetRegister(byte number)
        {
            switch (number)
            {
                case 0x40:
                    return this.Lcdc;

                case 0x41:
                    return (byte)(this.STAT | 0x80);

                case 0x42:
                    return this.SCY;

                case 0x43:
                    return this.SCX;

                case 0x44:
                    return this.LY;

                case 0x45:
                    return this.LYC;

                case 0x46:
                    return ((DataTypes.GbUInt16)(this.DMAAddress)).HighByte;

                case 0x47:
                    return this.BgPallet;

                case 0x48:
                    return this.objPallet0;

                case 0x49:
                    return this.objPallet1;

                case 0x4A:
                    return this.WY;

                case 0x4B:
                    return this.WX;

                default:
                    Console.WriteLine("Possible bad Read from LCD 0x" + number.ToString("X2") + " (reg number)");
                    return 0xFF;
            }
        }

        internal bool SetRegisters(int pointer, byte value)
        {
            switch (pointer)
            {
                case 0x0:
                    this.Lcdc = value;
                    break;

                case 0x1:
                    this.STAT = (byte)(this.STAT | (value & 0x78));
                    break;

                case 0x2:
                    this.SCY = value;
                    break;

                case 0x3:
                    this.SCX = value;
                    break;

                case 0x5:
                    this.LYC = value;
                    break;

                case 0x6:
                    this.DMAAddress = (ushort)(value << 8);
                    this.DMA = 0;
                    break;

                case 0x7:
                    this.BgPallet = value;
                    break;

                case 0x8:
                    this.objPallet0 = (byte)(value & 0xFC);
                    break;

                case 0x9:
                    this.objPallet1 = (byte)(value & 0xFC);
                    break;

                case 0xA:
                    this.WY = value;
                    break;

                case 0xB:
                    this.WX = value;
                    break;

                default:
                    return false;
            }

            return true;
        }
    }
}
