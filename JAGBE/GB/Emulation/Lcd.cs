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
        /// The display memory
        /// </summary>
        internal readonly int[] displayMemory = new int[Width * Height];

        /// <summary>
        /// The Object Atribute Memory.
        /// </summary>
        internal readonly GbUInt8[] Oam = new GbUInt8[MemoryRange.OAMSIZE];

        /// <summary>
        /// The Video Ram.
        /// </summary>
        internal readonly byte[] VRam = new byte[MemoryRange.VRAMBANKSIZE * 2];

        /// <summary>
        /// The height of the Game Boy LCD screen.
        /// </summary>
        private const int Height = 144;

        /// <summary>
        /// The color the Game Boy displays as white.
        /// </summary>
        /// <remarks>
        /// White gets it's own const because it's the only color that is ever directly needed.
        /// </remarks>
        private const int WHITE = unchecked((int)0xFF9BBC0F);

        /// <summary>
        /// The width of the Game Boy LCD screen.
        /// </summary>
        private const int Width = 160;

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

        /// <summary>
        /// The pallet of the background
        /// </summary>
        private byte bgPallet;

        private int cy;

        private bool disabled;

        /// <summary>
        /// The DMA cycle number
        /// </summary>
        private int Dma;

        private GbUInt16 DmaLdAddr;

        private int dmaLdTimer = -1;

        private int dmaMod;

        /// <summary>
        /// The LCDC register
        /// </summary>
        private GbUInt8 Lcdc;

        /// <summary>
        /// The Current scan line the lcd is drawing
        /// </summary>
        private int LY;

        /// <summary>
        /// The number to use when comparing to <see cref="LY"/>
        /// </summary>
        private byte LYC;

        /// <summary>
        /// The first object pallet
        /// </summary>
        private byte objPallet0;

        /// <summary>
        /// The second object pallet
        /// </summary>
        private byte objPallet1;

        private bool PIRC;

        /// <summary>
        /// The Scroll X register
        /// </summary>
        private byte SCX;

        /// <summary>
        /// The Scroll Y register
        /// </summary>
        private byte SCY;

        private int STATMode;

        /// <summary>
        /// The STAT register
        /// </summary>
        private GbUInt8 STATUpper;

        private readonly List<Sprite> visibleSprites = new List<Sprite>(10);

        private int windowLy;

        /// <summary>
        /// The Window W register
        /// </summary>
        private GbUInt8 WX;

        /// <summary>
        /// The Window Y register
        /// </summary>
        private GbUInt8 WY;

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
        /// The DMA address
        /// </summary>
        internal GbUInt16 DmaAddress { get; private set; }

        private readonly bool[] currLinePixelsTransparent = new bool[160];

        internal bool DmaMode { get; private set; }

        internal bool OamBlocked => this.STATMode > 1;

        internal bool VRamBlocked => this.STATMode == 3;

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
                    case 0x41: return (GbUInt8)((int)this.STATUpper | 0x80 | this.STATMode);
                    case 0x42: return this.SCY;
                    case 0x43: return this.SCX;
                    case 0x44: return (GbUInt8)this.LY;
                    case 0x45: return this.LYC;
                    case 0x46: return this.DmaAddress.HighByte;
                    case 0x47: return this.bgPallet;
                    case 0x48: return this.objPallet0;
                    case 0x49: return this.objPallet1;
                    case 0x4A: return this.WY;
                    case 0x4B: return this.WX;
                    default: return GbUInt8.MaxValue;
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
                        this.STATUpper = (GbUInt8)(value & 0x78);
                        break;

                    case 0x2:
                        this.SCY = (byte)value;
                        break;

                    case 0x3:
                        this.SCX = (byte)value;
                        break;

                    case 0x5:
                        this.LYC = (byte)value;
                        break;

                    case 0x6:
                        this.DmaLdAddr = (GbUInt16)(value << 8);
                        this.dmaLdTimer = 4;
                        break;

                    case 0x7:
                        this.bgPallet = (byte)value;
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
        /// Converts display memory to a byte representation.
        /// </summary>
        /// <returns><see cref="displayMemory"/> as a byte array.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public byte[] DisplayToBytes() => DisplayToBytes(new byte[this.displayMemory.Length / 4]);

        public byte[] DisplayToBytes(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (buffer.Length != (Width * Height) / 4)
            {
                throw new ArgumentException(nameof(this.displayMemory) + " must have a length of 160*144");
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0;
            }

            for (int i = 0; i < this.displayMemory.Length; i++)
            {
                int data = Array.IndexOf(COLORS, this.displayMemory[i]);
                if (data < 0)
                {
                    throw new InvalidOperationException("Display memory @" + i.ToString() + " is not a supported color");
                }

                buffer[i / 4] |= (byte)(data << (i & 3));
            }

            return buffer;
        }

        public void Tick(GbMemory mem, int TCycles)
        {
            for (int i = 0; i < TCycles; i++)
            {
                Tick(mem);
            }
        }

        /// <summary>
        /// Ticks the LCD.
        /// </summary>
        /// <param name="mem">The memory.</param>
        public void Tick(GbMemory mem)
        {
            TickDma(mem);
            if (!this.Lcdc[7])
            {
                this.Disable();
                return;
            }

            bool IRC = false;
            this.disabled = false;
            if (this.LY < 144)
            {
                IRC = UpdateLine();
            }
            else if (this.LY == 144)
            {
                if (this.cy == 0)
                {
                    IRC |= this.STATUpper[3];
                    this.STATMode = 0;
                }
                else
                {
                    IRC |= this.STATUpper[4] || this.STATUpper[5];
                    if (this.cy == Cpu.MCycle)
                    {
                        mem.IF |= 1;
                        this.STATMode = 1;
                    }
                    else if (this.cy == (Cpu.MCycle * 113) + 3)
                    {
                        this.LY++;
                        this.windowLy = 0;
                        this.cy = -1;
                    }
                    else
                    {
                        // Left intentionally empty
                    }

                    if (LyCompare())
                    {
                        this.STATUpper |= 4;
                        IRC |= this.STATUpper[6];
                    }
                    else
                    {
                        this.STATUpper = (GbUInt8)(this.STATUpper & 0xFB);
                    }
                }
            }
            else
            {
                if (LyCompare())
                {
                    this.STATUpper |= 4;
                    IRC |= this.STATUpper[6];
                }
                else
                {
                    this.STATUpper = (GbUInt8)(this.STATUpper & 0xFB);
                }

                IRC |= this.STATUpper[4] || this.STATUpper[5];
                if (this.cy == (Cpu.MCycle * 113) + 3)
                {
                    this.LY = (GbUInt8)((this.LY + 1) % 154);
                    this.cy = -1;
                }
            }

            if (IRC && !this.PIRC)
            {
                mem.IF |= 2;
            }

            this.PIRC = IRC;
            this.cy++;
        }

        internal Sprite ReadSprite(int offset) => new Sprite(
            (byte)(this.Oam[offset + 1] - 8),
            (byte)(this.Oam[offset] - 16),
            (byte)this.Oam[offset + 2],
            this.Oam[offset + 3]);

        private static bool IsSpritePixelVisible(int x, int palletIndex, bool priority, bool displayMemTransparent) =>
            x < Width && palletIndex != 0 && (priority || displayMemTransparent);

        /// <summary>
        /// Turns off the lcd.
        /// </summary>
        private void Disable()
        {
            if (this.disabled)
            {
                return;
            }

            this.disabled = true;
            this.PIRC = false;
            this.LY = 0;
            this.cy = 0;
            this.windowLy = 0;
            for (int i = 0; i < this.displayMemory.Length; i++)
            {
                this.displayMemory[i] = WHITE;
            }

            this.STATUpper = (GbUInt8)(this.STATUpper & 0x78);
        }

        private bool DrawSprite()
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

                ScanSprite(this.visibleSprites[indexOfBestSprite]);
                this.visibleSprites.RemoveAt(indexOfBestSprite);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the index of the color.
        /// </summary>
        /// <param name="pallet">The pallet.</param>
        /// <param name="y">The y.</param>
        /// <param name="x">The x.</param>
        /// <param name="tileNum">The tile number.</param>
        /// <returns>The index in <see cref="COLORS"/> that a pixel of a given tile is.</returns>
        private int GetColorIndex(int pallet, int y, int x, ushort tileNum) => GetColorIndex(pallet, GetPalletIndex(y, x, tileNum));

        private int GetPalletIndex(int y, int x, ushort tileNum)
        {
            int i = this.VRam[(tileNum * 16) + (y * 2)] >> (7 - x) & 1;
            return i + ((this.VRam[(tileNum * 16) + (y * 2) + 1] >> (7 - x) & 1) * 2);
        }

        private static int GetColorIndex(int pallet, int index) => (pallet >> (index * 2)) & 3;

        private bool IsSpriteVisible(Sprite sprite)
        {
            if (sprite.Y <= this.LY && sprite.Y + 8 > this.LY)
            {
                int tileY = (sprite.Flags[6] ? (7 - (this.LY & 7)) : (this.LY & 7));
                for (int x = 0; x < 8; x++)
                {
                    int colorIndex = GetPalletIndex(tileY, (byte)(sprite.Flags[5] ? (7 - x) : x), sprite.Tile);

                    // 0 is a hack to make sure that x is always < width
                    if (IsSpritePixelVisible(0, colorIndex, !sprite.Flags[7],
                        sprite.X + x >= Width || this.currLinePixelsTransparent[sprite.X + x]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool LyCompare() => (this.cy != 0 && this.LYC == this.LY) || (this.cy == 0 && this.LYC == 0);

        private void RenderLine()
        {
            if (this.Lcdc[0])
            {
                ScanLine();
            }

            if (this.Lcdc[5])
            {
                ScanLineWindow();
            }

            if (this.Lcdc[1])
            {
                ScanLineSprite();
            }
        }

        /// <summary>
        /// Scans a line.
        /// </summary>
        private void ScanLine()
        {
            // Offset from the start of VRAM to the start of the background map.
            ushort mapOffset = (ushort)(this.Lcdc[3] ? 0x1C00 : 0x1800);
            mapOffset += (ushort)((((this.SCY + this.LY) & 0xFF) >> 3) * 32);
            int lineOffset = (this.SCX >> 3);
            int y = ((this.LY + this.SCY) & 7) * 2;
            int x = this.SCX & 7;
            ushort tile = this.VRam[lineOffset + mapOffset];
            bool isTilesetMode1 = this.Lcdc[4];
            int pallet = this.bgPallet;
            if (!isTilesetMode1 && tile < 128)
            {
                tile += 256;
            }

            int pIndexUpper = this.VRam[(tile * 16) + y];
            int pIndexLower = this.VRam[(tile * 16) + y + 1];
            for (int i = 0; i < Width; i++)
            {
                int index = ((pIndexUpper >> (7 - x) & 1) | ((pIndexLower >> (7 - x) & 1) * 2)) * 2;
                this.displayMemory[((Height - this.LY - 1) * Width) + i] = COLORS[(pallet >> index) & 3];
                this.currLinePixelsTransparent[i] = index == 0;
                x++;
                if (x == 8)
                {
                    x = 0;
                    lineOffset = ((lineOffset + 1) & 31);
                    tile = this.VRam[lineOffset + mapOffset];
                    if (!isTilesetMode1 && tile < 128)
                    {
                        tile += 256;
                    }

                    pIndexUpper = this.VRam[(tile * 16) + y];
                    pIndexLower = this.VRam[(tile * 16) + y + 1];
                }
            }
        }

        /// <summary>
        /// Scans the sprites of a line.
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        private void ScanLineSprite()
        {
            if (this.Lcdc[2])
            {
                throw new NotSupportedException();
            }

            int spritesDrawn = 0;
            for (int offset = 0; offset < 160 && spritesDrawn < 10; offset += 4)
            {
                Sprite sprite = ReadSprite(offset);
                if (IsSpriteVisible(sprite))
                {
                    this.visibleSprites.Add(sprite);
                    spritesDrawn++;
                }
            }

            while (DrawSprite())
            {
                // Draw a sprite.
            }
        }

        /// <summary>
        /// Scans the window of a line.
        /// </summary>
        private void ScanLineWindow()
        {
            if (this.WX < 7 || this.WX > 166)
            {
                this.windowLy++;
                return;
            }

            ushort mapOffset = (ushort)(this.Lcdc[6] ? 0x1C00 : 0x1800); // Base offset
            mapOffset += (ushort)((((this.WY + this.windowLy) & 0xFF) >> 3) * 32);
            int lineOffset = this.WX - 7;
            int y = (this.WY + this.windowLy) & 7;
            int x = (this.WX - 7) & 7;
            ushort tile = this.VRam[lineOffset + mapOffset];
            if (!this.Lcdc[4] && tile < 128)
            {
                tile += 256;
            }

            for (int i = this.WX - 7; i < Width; i++)
            {
                int colorIndex = GetColorIndex(this.bgPallet, y, x, tile);
                if (colorIndex != 0)
                {
                    this.displayMemory[((Height - this.LY - 1) * Width) + i - (this.WX - 7)] = COLORS[colorIndex];
                }

                x++;
                if (x == 8)
                {
                    x = 0;
                    lineOffset = ((lineOffset + 1) & 31);
                    tile = this.VRam[lineOffset + mapOffset];
                    if (!this.Lcdc[4] && tile < 128)
                    {
                        tile += 256;
                    }
                }
            }

            this.windowLy++;
        }

        /// <summary>
        /// Scans a sprite.
        /// </summary>
        /// <param name="sprite">The sprite.</param>
        private void ScanSprite(Sprite sprite)
        {
            if (sprite.Y <= this.LY && sprite.Y + 8 > this.LY)
            {
                int pallet = sprite.Flags[4] ? this.objPallet1 : this.objPallet0;
                int displayOffset = ((Height - this.LY - 1) * Width) + sprite.X;
                int tileY = (sprite.Flags[6] ? (7 - (this.LY & 7)) : (this.LY & 7));
                for (int x = 0; x < 8 && sprite.X + x < Width; x++)
                {
                    int palletIndex = GetPalletIndex(tileY, sprite.Flags[5] ? (7 - x) : x, sprite.Tile);
                    if (IsSpritePixelVisible(sprite.X + x, palletIndex, !sprite.Flags[7], this.currLinePixelsTransparent[sprite.X + x]))
                    {
                        this.displayMemory[displayOffset + x] = COLORS[GetColorIndex(pallet, palletIndex)];
                    }
                }
            }
        }

        private void TickDma(GbMemory memory)
        {
            if (this.dmaMod == 0)
            {
                this.dmaMod = 3;
                if (this.Dma == 0)
                {
                    this.DmaMode = false;
                }
                else
                {
                    this.DmaMode = true;
                    this.Oam[160 - this.Dma] = memory.GetMappedMemoryDma(this.DmaAddress++);
                    this.Dma--;
                }
            }
            else
            {
                this.dmaMod--;
            }

            if (this.dmaLdTimer > 0)
            {
                this.dmaLdTimer--;
            }

            if (this.dmaLdTimer == 0)
            {
                this.Dma = 160;
                this.DmaAddress = this.DmaLdAddr;
                this.dmaLdTimer = -1;
            }
        }

        /// <summary>
        /// Updates a line.
        /// </summary>
        private bool UpdateLine()
        {
            bool IRC = false;
            if (LyCompare())
            {
                this.STATUpper |= 4;
                IRC |= this.STATUpper[6];
            }
            else
            {
                this.STATUpper = (GbUInt8)(this.STATUpper & 0xFB);
            }

            switch (this.cy)
            {
                case Cpu.MCycle:
                    this.STATMode = 2;
                    IRC |= this.STATUpper[5];
                    break;

                case Cpu.MCycle * 10:
                    this.STATMode = 3;
                    RenderLine();
                    break;

                case 0:
                case Cpu.MCycle * 53:
                    IRC |= this.STATUpper[3];
                    this.STATMode = 0;
                    break;

                case (Cpu.MCycle * 113) + 3:
                    this.LY++;
                    IRC |= this.STATUpper[3];
                    this.cy = -1;
                    break;

                default: // Nothing interesting is happening this cycle.
                    IRC |= (this.STATMode == 2 && this.STATUpper[5]) || (this.STATMode == 0 && this.STATUpper[3]);
                    break;
            }

            return IRC;
        }
    }
}
