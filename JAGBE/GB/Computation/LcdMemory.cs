using System;

namespace JAGBE.GB.Computation
{
    internal sealed class LcdMemory
    {
        /// <summary>
        /// The pallet of the background
        /// </summary>
        public byte BgPallet;

        /// <summary>
        /// The DMA cycle number
        /// </summary>
        public int DMA = Cpu.DelayStep * 162;

        /// <summary>
        /// The DMA address
        /// </summary>
        public ushort DMAAddress;

        /// <summary>
        /// The DMA value
        /// </summary>
        public byte DMAValue;

        internal int cy;

        /// <summary>
        /// The display memory
        /// </summary>
        internal int[] displayMemory = new int[Lcd.Width * Lcd.Height];

        /// <summary>
        /// Should the LCD be forced to skip rendering
        /// </summary>
        internal bool ForceNullRender;

        /// <summary>
        /// Is the LCD requesting an interupt
        /// </summary>
        internal bool IRC;

        /// <summary>
        /// The Current scan line the lcd is drawing
        /// </summary>
        internal byte LY;

        /// <summary>
        /// The number to use when comparing to <see cref="LY"/>
        /// </summary>
        internal byte LYC;

        /// <summary>
        /// The first object pallet
        /// </summary>
        internal byte objPallet0;

        /// <summary>
        /// The second object pallet
        /// </summary>
        internal byte objPallet1;

        /// <summary>
        /// The Previous state of <see cref="IRC"/>
        /// </summary>
        internal bool PIRC;

        /// <summary>
        /// The Scroll X register
        /// </summary>
        internal byte SCX;

        /// <summary>
        /// The Scroll Y register
        /// </summary>
        internal byte SCY;

        /// <summary>
        /// The STAT register
        /// </summary>
        internal byte STAT;

        /// <summary>
        /// The Window W register
        /// </summary>
        internal byte WX;

        /// <summary>
        /// The Window Y register
        /// </summary>
        internal byte WY;

        /// <summary>
        /// The LCDC register
        /// </summary>
        internal byte Lcdc;

        /// <summary>
        /// Gets value of the given <paramref name="number"/>.
        /// </summary>
        /// <param name="number">The register number.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Sets the register number <paramref name="pointer"/> to <paramref name="value"/>.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        internal void SetRegister(int pointer, byte value)
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
                    Console.WriteLine("Invalid LCD write (" + pointer.ToString("X2") + ")");
                    break;
            }
        }
    }
}
