using System;
using System.Collections.Generic;

namespace JAGBE.GB.Emulation
{
    /// <summary>
    /// Home of the LCD subsystem.
    /// </summary>
    internal sealed class Lcd
    {
        /// <summary>
        /// The height of the Game Boy LCD screen.
        /// </summary>
        private const int Height = 144;

        /// <summary>
        /// The width of the Game Boy LCD screen.
        /// </summary>
        private const int Width = 160;

        /// <summary>
        /// The pallet of the background
        /// </summary>
        private GbUInt8 BgPallet;

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

        private int cy;

        /// <summary>
        /// The display memory
        /// </summary>
        internal readonly int[] displayMemory = new int[Width * Height];

        /// <summary>
        /// Should the LCD be forced to skip rendering
        /// </summary>
        internal bool ForceNullRender;

        /// <summary>
        /// Is the LCD requesting an interupt
        /// </summary>
        private bool IRC;

        /// <summary>
        /// The LCDC register
        /// </summary>
        private GbUInt8 Lcdc;

        /// <summary>
        /// The Current scan line the lcd is drawing
        /// </summary>
        private GbUInt8 LY;

        /// <summary>
        /// The number to use when comparing to <see cref="LY"/>
        /// </summary>
        private GbUInt8 LYC;

        /// <summary>
        /// The first object pallet
        /// </summary>
        private GbUInt8 objPallet0;

        /// <summary>
        /// The second object pallet
        /// </summary>
        private GbUInt8 objPallet1;

        /// <summary>
        /// The Previous state of <see cref="IRC"/>
        /// </summary>
        private bool PIRC;

        /// <summary>
        /// The Scroll X register
        /// </summary>
        private GbUInt8 SCX;

        /// <summary>
        /// The Scroll Y register
        /// </summary>
        private GbUInt8 SCY;

        /// <summary>
        /// The STAT register
        /// </summary>
        internal GbUInt8 STAT;

        /// <summary>
        /// The Window W register
        /// </summary>
        private GbUInt8 WX;

        /// <summary>
        /// The Window Y register
        /// </summary>
        private GbUInt8 WY;

        /// <summary>
        /// The color the Game Boy displays as white.
        /// </summary>
        /// <remarks>
        /// White gets it's own const because it's the only color that is ever directly needed.
        /// </remarks>
        private const int WHITE = unchecked((int)0xFF9BBC0F);

        /// <summary>
        /// The colors that should be displayed.
        /// </summary>
        private static readonly int[] COLORS =
        {
                WHITE,
                unchecked((int)0xFF8BAC0F),
                unchecked((int)0xFF306230),
                unchecked((int)0xFF0F380F)
        };

        private readonly List<Sprite> visibleSprites = new List<Sprite>(10);

        /// <summary>
        /// Initializes a new instance of the <see cref="Lcd"/> class.
        /// </summary>
        public Lcd()
        {
            for (int i = 0; i < this.displayMemory.Length; i++)
            {
                this.displayMemory[i] = WHITE;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="GbUInt8"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <value>The <see cref="GbUInt8"/>.</value>
        /// <param name="index">The index.</param>
        /// <returns>the <see cref="GbUInt8"/> at the specified <paramref name="index"/>.</returns>
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
                        this.STAT = (value & 0x78) | (this.STAT & 3);
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
                        this.DMAAddress = (GbUInt16)(value << 8);
                        this.DMA = 0;
                        break;

                    case 0x7:
                        this.BgPallet = value;
                        break;

                    case 0x8:
                        this.objPallet0 = value & 0xFC;
                        break;

                    case 0x9:
                        this.objPallet1 = value & 0xFC;
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
        /// Converts display memory to a byte representation.
        /// </summary>
        /// <returns><see cref="displayMemory"/> as a byte array.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public byte[] DisplayToBytes()
        {
            byte[] arr = new byte[this.displayMemory.Length / 4];
            for (int i = 0; i < this.displayMemory.Length; i++)
            {
                int data = Array.IndexOf(COLORS, this.displayMemory[i]);
                if (data < 0)
                {
                    throw new InvalidOperationException("Display memory @" + i.ToString() + " is not a supported color");
                }

                arr[i / 4] |= (byte)(data << (i & 3));
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
                        this.STAT |= 1;
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

                    if (this.LYC == 144)
                    {
                        this.STAT |= 4;
                    }
                    else
                    {
                        this.STAT &= 0xFB;
                    }
                }
            }
            else
            {
                if (this.cy == Cpu.DelayStep)
                {
                    this.IRC |= this.STAT[6] && this.LYC == 144;
                }
                else if (this.cy == Cpu.DelayStep * 113)
                {
                    this.LY = (GbUInt8)((this.LY + 1) % 154);
                    this.cy = -Cpu.DelayStep;
                }
                else
                {
                    // Left intentionally empty
                }
            }

            if (this.IRC && !this.PIRC)
            {
                mem.IF |= 2;
            }
            else
            {
                this.PIRC = false;
            }
            this.IRC = false;
            this.cy += Cpu.DelayStep;
        }

        /// <summary>
        /// Gets the index of the color.
        /// </summary>
        /// <param name="pallet">The pallet.</param>
        /// <param name="VRam">The v ram.</param>
        /// <param name="y">The y.</param>
        /// <param name="x">The x.</param>
        /// <param name="tileMapOffset">The tile map offset.</param>
        /// <param name="tileNum">The tile number.</param>
        /// <returns>The index in <see cref="COLORS"/> that a pixel of a given tile is.</returns>
        private static int GetColorIndex(GbUInt8 pallet, GbUInt8[] VRam, byte y, byte x, ushort tileMapOffset, ushort tileNum)
        {
            int i = ((int)VRam[(tileNum * 16) + tileMapOffset + (y * 2)] >> (7 - x) & 1);
            i += ((int)VRam[(tileNum * 16) + tileMapOffset + (y * 2) + 1] >> (7 - x) & 1) * 2;
            return ((int)pallet >> (i * 2)) & 3;
        }

        private static bool IsSpritePixelVisible(int x, int colorIndex, bool priority, int dispMemAtOffset) =>
            x < Width && colorIndex != 0 && (priority || dispMemAtOffset == WHITE);

        /// <summary>
        /// Turns off the lcd.
        /// </summary>
        private void Disable()
        {
            this.PIRC = false;
            this.LY = 0;
            this.cy = 0;
            for (int i = 0; i < this.displayMemory.Length; i++)
            {
                this.displayMemory[i] = WHITE;
            }
        }

        private bool DrawSprite(GbUInt8[] VRam)
        {
            if (this.visibleSprites.Count > 0)
            {
                int indexOfBestSprite = 0;
                for (int j = this.visibleSprites.Count - 1; j >= 0; j--)
                {
                    if (this.visibleSprites[j].X > this.visibleSprites[indexOfBestSprite].X)
                    {
                        indexOfBestSprite = j;
                    }
                }

                ScanSprite(VRam, this.visibleSprites[indexOfBestSprite]);
                this.visibleSprites.RemoveAt(indexOfBestSprite);
                return true;
            }

            return false;
        }

        private bool IsSpriteVisible(Sprite sprite, GbUInt8[] VRam)
        {
            if (sprite.Y <= this.LY && sprite.Y + 8 > this.LY)
            {
                GbUInt8 pallet = sprite.Flags[4] ? this.objPallet1 : this.objPallet0;
                int displayOffset = ((Height - this.LY - 1) * Width) + sprite.X;
                byte tileY = (byte)(sprite.Flags[6] ? (7 - (this.LY & 7)) : (this.LY & 7));
                for (int x = 0; x < 8; x++)
                {
                    int colorIndex = GetColorIndex(pallet, VRam, tileY, (byte)(sprite.Flags[5] ? (7 - x) : x), 0, sprite.Tile);
                    if (IsSpritePixelVisible(sprite.X + x, colorIndex, !sprite.Flags[7], this.displayMemory[displayOffset + x]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Renders a line.
        /// </summary>
        /// <param name="VRam">The v ram.</param>
        /// <param name="Oam">The oam.</param>
        private void RenderLine(GbUInt8[] VRam, GbUInt8[] Oam)
        {
            if ((this.cy != 0 && this.LYC == this.LY) || (this.cy == 0 && this.LYC == 0))
            {
                this.STAT |= 4;
            }
            else
            {
                this.STAT &= 0xFB;
            }

            if (this.cy == 0)
            {
                this.STAT &= 0xFC;
            }
            else if (this.cy <= Cpu.DelayStep * 10)
            {
                if (this.cy == Cpu.DelayStep)
                {
                    this.STAT |= 2;
                    this.IRC |= this.STAT[6] && this.LY != 0 && this.LYC == this.LY;
                }
            }
            else if (this.cy == Cpu.DelayStep * 11)
            {
                this.STAT |= 3;
                if (!this.ForceNullRender)
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
            else if (this.cy == Cpu.DelayStep * 113)
            {
                this.LY++;
                this.cy = -Cpu.DelayStep;
            }
            else
            {
                // Left intentionally empty.
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
                    COLORS[GetColorIndex(this.BgPallet, VRam, y, x, tileOffset, tile)];
                x++;
                if (x == 8)
                {
                    x = 0;
                    lineOffset = (byte)((lineOffset + 1) & 31);
                    tile = VRam[lineOffset + mapOffset];
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

            int spritesDrawn = 0;
            for (int offset = 0; offset < 160 && spritesDrawn < 10; offset += 4)
            {
                // OAM goes Y,X,Tile,Flags.
                Sprite sprite = new Sprite((byte)(Oam[offset + 1] - 8), (byte)(Oam[offset] - 16), Oam[offset + 2], Oam[offset + 3]);

                if (IsSpriteVisible(sprite, VRam))
                {
                    this.visibleSprites.Add(sprite);
                    spritesDrawn++;
                }
            }

            for (int i = 0; i < 10; i++)
            {
                if (!DrawSprite(VRam))
                {
                    return;
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
            byte y = (byte)((this.WY + this.LY) & 7);
            byte x = (byte)(this.WX & 7);

            ushort tileOffset = (ushort)(this.Lcdc[4] ? 0 : 0x800);
            ushort tile = VRam[lineOffset + mapOffset];
            if (!this.Lcdc[4] && tile < 128)
            {
                tile += 256;
            }

            for (int i = 0; i < Width; i++)
            {
                this.displayMemory[((Height - this.LY - 1) * Width) + i] =
                   COLORS[GetColorIndex(this.BgPallet, VRam, y, x, tileOffset, tile)];

                x++;
                if (x == 8)
                {
                    x = 0;
                    lineOffset = (byte)((lineOffset + 1) & 31);
                    tile = VRam[lineOffset + mapOffset];
                    if (!this.Lcdc[4] && tile < 128)
                    {
                        tile += 256;
                    }
                }
            }
        }

        /// <summary>
        /// Scans a sprite.
        /// </summary>
        /// <param name="VRam">The v ram.</param>
        /// <param name="sprite">The sprite.</param>
        private void ScanSprite(GbUInt8[] VRam, Sprite sprite)
        {
            if (sprite.Y <= this.LY && sprite.Y + 8 > this.LY)
            {
                GbUInt8 pallet = sprite.Flags[4] ? this.objPallet1 : this.objPallet0;
                int displayOffset = ((Height - this.LY - 1) * Width) + sprite.X;
                byte tileY = (byte)(sprite.Flags[6] ? (7 - (this.LY & 7)) : (this.LY & 7));
                for (int x = 0; x < 8; x++)
                {
                    int colorIndex = GetColorIndex(pallet, VRam, tileY, (byte)(sprite.Flags[5] ? (7 - x) : x), 0, sprite.Tile);
                    if (IsSpritePixelVisible(sprite.X + x, colorIndex, !sprite.Flags[7], this.displayMemory[displayOffset + x]))
                    {
                        this.displayMemory[displayOffset + x] = COLORS[colorIndex];
                    }
                }
            }
        }
    }
}
