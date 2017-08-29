using System;

namespace JAGBE.GB.Emulation
{
    internal sealed class Lcd
    {
        /// <summary>
        /// The height of the Game Boy LCD screen.
        /// </summary>
        public const int Height = 144;

        /// <summary>
        /// The width of the Game Boy LCD screen.
        /// </summary>
        public const int Width = 160;

        /// <summary>
        /// The pallet of the background
        /// </summary>
        public GbUInt8 BgPallet;

        /// <summary>
        /// The DMA cycle number
        /// </summary>
        public int DMA = Cpu.DelayStep * 162;

        /// <summary>
        /// The DMA address
        /// </summary>
        public GbUInt16 DMAAddress;

        /// <summary>
        /// The DMA value
        /// </summary>
        public GbUInt8 DMAValue;

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
        /// The LCDC register
        /// </summary>
        internal GbUInt8 Lcdc;

        /// <summary>
        /// The Current scan line the lcd is drawing
        /// </summary>
        internal GbUInt8 LY;

        /// <summary>
        /// The number to use when comparing to <see cref="LY"/>
        /// </summary>
        internal GbUInt8 LYC;

        /// <summary>
        /// The first object pallet
        /// </summary>
        internal GbUInt8 objPallet0;

        /// <summary>
        /// The second object pallet
        /// </summary>
        internal GbUInt8 objPallet1;

        /// <summary>
        /// The Previous state of <see cref="IRC"/>
        /// </summary>
        internal bool PIRC;

        /// <summary>
        /// The Scroll X register
        /// </summary>
        internal GbUInt8 SCX;

        /// <summary>
        /// The Scroll Y register
        /// </summary>
        internal GbUInt8 SCY;

        /// <summary>
        /// The STAT register
        /// </summary>
        internal GbUInt8 STAT;

        /// <summary>
        /// The Window W register
        /// </summary>
        internal GbUInt8 WX;

        /// <summary>
        /// The Window Y register
        /// </summary>
        internal GbUInt8 WY;

        public GbUInt8 this[byte index]
        {
            get
            {
                switch (index)
                {
                    case 0x40: return this.Lcdc;
                    case 0x41: return this.STAT | 0x80;
                    case 0x42: return this.SCY;
                    case 0x43: return this.SCX;
                    case 0x44: return this.LY;
                    case 0x45: return this.LYC;
                    case 0x46: return this.DMAAddress.HighByte;
                    case 0x47: return this.BgPallet;
                    case 0x48: return this.objPallet0;
                    case 0x49: return this.objPallet1;
                    case 0x4A: return this.WY;
                    case 0x4B: return this.WX;
                    default: return 0xFF;
                }
            }

            set
            {
                switch (index)
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
                        return;
                }
            }
        }

        /// <summary>
        /// The colors that should be displayed.
        /// </summary>
        private static readonly uint[] COLORS =
        {
                0xFF9BBC0F,
                0xFF8BAC0F,
                0xFF306230,
                0xFF0F380F
        };

        /// <summary>
        /// Converts display memory to a byte representation.
        /// </summary>
        /// <param name="displayMem">The display memory.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static byte[] DisplayToBytes(int[] displayMem)
        {
            int displayMemLength = displayMem.Length;
            if ((displayMemLength & 3) != 0)
            {
                throw new InvalidOperationException();
            }

            byte[] arr = new byte[displayMemLength / 4];

            for (int i = 0; i < displayMemLength; i++)
            {
                int j = i & 3;
                int data = 0;
                bool valid = false;
                for (int k = 0; k < 4; k++)
                {
                    if ((int)COLORS[k] == displayMem[i])
                    {
                        data = k;
                        valid = true;
                    }
                }

                if (!valid)
                {
                    throw new InvalidOperationException();
                }

                arr[i / 4] |= (byte)(data << j);
            }

            return arr;
        }

        /// <summary>
        /// Ticks the LCD.
        /// </summary>
        /// <param name="mem">The memory.</param>
        public void Tick(GbMemory mem)
        {
            if (!this.Lcdc[7])
            {
                if (this.LY != 0 || this.cy != 0 || this.displayMemory[0] == 0)
                {
                    this.Disable();
                }

                return;
            }

            if (this.LY < 144)
            {
                RenderLine(mem.VRam, mem.Oam);
            }
            else if (this.LY == 144)
            {
                if (this.cy == 0)
                {
                    this.STAT &= 0xFC;
                }
                else
                {
                    if (this.cy == Cpu.DelayStep)
                    {
                        mem.IF |= 1;
                        this.IRC |= this.STAT[6] && this.LYC == 144;
                    }
                    else if (this.cy == Cpu.DelayStep * 113)
                    {
                        this.LY++;
                        this.cy = -Cpu.DelayStep;
                    }
                    else
                    {
                        // Left intentionally empty
                    }
                    this.STAT = (byte)((this.STAT & 0xF8) | 1 | (this.LYC == 144 ? 0x4 : 0));
                }
            }
            else if (this.LY < 153)
            {
                if (this.cy == Cpu.DelayStep)
                {
                    if (this.STAT[6] && this.LYC == 144)
                    {
                        this.IRC = true;
                    }
                }
                else if (this.cy == Cpu.DelayStep * 113)
                {
                    this.LY++;
                    this.cy = -Cpu.DelayStep;
                }
                else
                {
                    // Left intentionally empty
                }
            }
            else
            {
                if (this.cy == Cpu.DelayStep)
                {
                    if (this.STAT[6] && this.LYC == 144)
                    {
                        this.IRC = true;
                    }
                }
                else if (this.cy == Cpu.DelayStep * 113)
                {
                    this.LY = 0;
                    this.cy = -Cpu.DelayStep;
                }
                else
                {
                    // Left intentionally empty
                }
            }

            if (this.IRC)
            {
                if (!this.PIRC)
                {
                    this.PIRC = true;
                    mem.IF |= 2;
                }
            }
            else
            {
                this.PIRC = false;
            }
            this.IRC = false;
            this.cy += Cpu.DelayStep;
        }

        private void Disable()
        {
            this.PIRC = false;
            this.LY = 0;
            this.cy = 0;
            for (int i = 0; i < this.displayMemory.Length; i++)
            {
                this.displayMemory[i] = (int)COLORS[0];
            }
        }

        private static int GetColorIndex(GbUInt8 pallet, GbUInt8[] VRam, byte y, byte x, ushort tileMapOffset, ushort tileNum)
        {
            int i = (VRam[(tileNum * 16) + tileMapOffset + (y * 2)][(byte)(7 - x)] ? 1 : 0);
            i += (VRam[(tileNum * 16) + tileMapOffset + (y * 2) + 1][(byte)(7 - x)] ? 2 : 0);
            return (pallet >> (i * 2)) & 3;
        }

        private void RenderLine(GbUInt8[] VRam, GbUInt8[] Oam)
        {
            if (this.cy == 0)
            {
                this.STAT &= 0xFC;
            }
            else if (this.cy <= Cpu.DelayStep * 10)
            {
                if (this.cy == Cpu.DelayStep)
                {
                    this.STAT = ((this.STAT & 0xFC) | 2);
                    if (this.LY != 0 && this.LYC == this.LY && (this.STAT[6]))
                    {
                        this.IRC = true;
                    }
                }
            }
            else if (this.cy == Cpu.DelayStep * 11)
            {
                this.STAT = (byte)((this.STAT & 0xFC) | 3);

                if (this.ForceNullRender)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        this.displayMemory[((Height - this.LY - 1) * Width) + i] = (int)COLORS[0];
                    }
                }
                else
                {
                    if (this.Lcdc[0])
                    {
                        ScanLine(VRam);
                    }

                    if (this.Lcdc[5])
                    {
                        ScanLineWindow(VRam);
                    }

                    if (this.Lcdc[1])
                    {
                        ScanLineSprite(VRam, Oam);
                    }
                }
            }
            else if (this.cy < Cpu.DelayStep * 63)
            {
                // Add proper pixel-by-pixel emulation here.
            }
            else if (this.cy == Cpu.DelayStep * 113)
            {
                this.LY++;
                this.cy = -Cpu.DelayStep;
            }
            else
            {
                // Left intentionally empty.
            }
            if ((this.cy == 0 && this.LYC == 0) || (this.cy != 0 && this.LYC == this.LY))
            {
                this.STAT |= 4;
            }
            else
            {
                this.STAT &= 0xFB;
            }
        }

        /// <summary>
        /// Scans a line.
        /// </summary>
        /// <param name="VRam">The vram.</param>
        private void ScanLine(GbUInt8[] VRam)
        {
            // Offset from the start of VRAM to the start of the background map.
            ushort mapOffset = (ushort)(this.Lcdc[3] ? 0x1C00 : 0x1800);
            mapOffset += (ushort)((((this.SCY + this.LY) & 0xFF) >> 3) * 32);

            byte lineOffset = (byte)(this.SCX >> 3);
            byte y = (byte)((this.LY + this.SCY) & 7);
            byte x = (byte)(this.SCX & 7);

            ushort tileOffset = (ushort)(this.Lcdc[4] ? 0 : 0x800);
            ushort tile = VRam[(ushort)(lineOffset + mapOffset)];
            if (!this.Lcdc[4] && tile < 128)
            {
                tile += 256;
            }

            for (int i = 0; i < Width; i++)
            {
                this.displayMemory[((Height - this.LY - 1) * Width) + i] =
                    (int)COLORS[GetColorIndex(this.BgPallet, VRam, y, x, tileOffset, tile)];
                x++;
                if (x == 8)
                {
                    x = 0;
                    lineOffset = (byte)((lineOffset + 1) & 31);
                    tile = VRam[(ushort)(lineOffset + mapOffset)];
                    if (!this.Lcdc[4] && tile < 128)
                    {
                        tile += 256;
                    }
                }
            }
        }

        /// <summary>
        /// Scans the sprites of a line.
        /// </summary>
        /// <param name="VRam">The v ram.</param>
        /// <param name="Oam">The oam.</param>
        /// <exception cref="NotSupportedException"></exception>
        private void ScanLineSprite(GbUInt8[] VRam, GbUInt8[] Oam)
        {
            if (this.Lcdc[2])
            {
                throw new NotSupportedException();
            }

            // FIXME: Doesn't handle overlapping sprites.
            for (int i = 0; i < 40; i++)
            {
                ScanSprite(VRam, Oam, i);
            }
        }

        /// <summary>
        /// Scans a sprite.
        /// </summary>
        /// <param name="VRam">The v ram.</param>
        /// <param name="Oam">The oam.</param>
        /// <param name="spriteNumber">The sprite number.</param>
        private void ScanSprite(GbUInt8[] VRam, GbUInt8[] Oam, int spriteNumber)
        {
            int oamOffset = spriteNumber * 4;
            byte spriteY = (byte)(Oam[oamOffset] - 16);
            byte spriteX = (byte)(Oam[oamOffset + 1] - 8);
            GbUInt8 tile = Oam[oamOffset + 2];
            GbUInt8 flags = Oam[oamOffset + 3];
            if (spriteY <= this.LY && spriteY + 8 > this.LY)
            {
                GbUInt8 pallet = flags[4] ? this.objPallet1 : this.objPallet0;
                int displayOffset = ((Height - this.LY - 1) * Width) + spriteX;
                byte tileY = (byte)(flags[6] ? (7 - (this.LY & 7)) : (this.LY & 7));
                for (int x = 0; x < 8; x++)
                {
                    int colorIndex = GetColorIndex(pallet, VRam, tileY, (byte)(flags[5] ? (7 - x) : x), 0, tile);
                    if (spriteX + x < Width && colorIndex != 0 && (!flags[7] || this.displayMemory[displayOffset + x] == (int)COLORS[0]))
                    {
                        this.displayMemory[displayOffset + x] = (int)COLORS[colorIndex];
                    }
                }
            }
        }

        /// <summary>
        /// Scans the window of a line.
        /// </summary>
        /// <param name="VRam">The v ram.</param>
        /// <exception cref="NotSupportedException"></exception>
        private void ScanLineWindow(GbUInt8[] VRam)
        {
            ushort mapOffset = (ushort)(this.Lcdc[3] ? 0x1C00 : 0x1800); // Base offset
            mapOffset += (ushort)((((this.WY + this.LY) & 0xFF) >> 3) * 32);
            byte lineOffset = (byte)(this.WX >> 3);
            byte y = (byte)((this.WY + this.WY) & 7);
            byte x = (byte)(this.WX & 7);

            ushort tileOffset = (ushort)(this.Lcdc[4] ? 0 : 0x800);
            ushort tile = VRam[(ushort)(lineOffset + mapOffset)];
            if (!this.Lcdc[4] && tile < 128)
            {
                tile += 256;
            }

            for (int i = 0; i < Width; i++)
            {
                this.displayMemory[((Height - this.LY - 1) * Width) + i] =
                    (int)COLORS[GetColorIndex(this.BgPallet, VRam, y, x, tileOffset, tile)];

                x++;
                if (x == 8)
                {
                    x = 0;
                    lineOffset = (byte)((lineOffset + 1) & 31);
                    tile = VRam[(ushort)(lineOffset + mapOffset)];
                    if (!this.Lcdc[4] && tile < 128)
                    {
                        tile += 256;
                    }
                }
            }
        }
    }
}
