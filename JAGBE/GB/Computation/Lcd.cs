#define PERLINERENDERER

using System;

namespace JAGBE.GB.Computation
{
    internal static class Lcd
    {
        public const int Height = 144;

        public const int Width = 160;

        private static readonly uint[] COLORS =
        {
                0xFF9BBC0F,
                0xFF8BAC0F,
                0xFF306230,
                0xFF0F380F
        };

        public static void Tick(GbMemory mem)
        {
            LcdMemory lcdMem = mem.lcdMemory;
            if (!lcdMem.Lcdc.GetBit(7))
            {
                if (lcdMem.LY != 0 || lcdMem.cy != 0)
                {
                    DisableLcd(lcdMem);
                }

                return;
            }

            if (lcdMem.LY < 144)
            {
                RenderLine(mem);
            }
            else if (lcdMem.LY == 144)
            {
                if (lcdMem.cy == 0)
                {
                    lcdMem.STAT &= 0xFC;
                }
                else
                {
                    if (lcdMem.cy == Cpu.DelayStep)
                    {
                        mem.SetMappedMemory(0xFF0F, (byte)(mem.GetMappedMemoryDma(0xFF0F) | 1));
                        lcdMem.IRC |= lcdMem.STAT.GetBit(6) && lcdMem.LYC == 144;
                    }
                    else if (mem.lcdMemory.cy == Cpu.DelayStep * 113)
                    {
                        lcdMem.LY++;
                        lcdMem.cy = 0;
                    }
                    else
                    {
                        // Left intentionally empty
                    }
                    lcdMem.STAT = (byte)((lcdMem.STAT & 0xF8) | 1 | (lcdMem.LYC == 144 ? 0x4 : 0));
                }
            }
            else if (lcdMem.LY < 153)
            {
                if (lcdMem.cy == Cpu.DelayStep)
                {
                    if (lcdMem.STAT.GetBit(6) && lcdMem.LYC == 144)
                    {
                        lcdMem.IRC = true;
                    }
                }
                else if (lcdMem.cy == Cpu.DelayStep * 113)
                {
                    lcdMem.LY++;
                    lcdMem.cy = -Cpu.DelayStep;
                }
                else
                {
                    // Left intentionally empty
                }
            }
            else
            {
                if (lcdMem.cy == Cpu.DelayStep)
                {
                    if (lcdMem.STAT.GetBit(6) && lcdMem.LYC == 144)
                    {
                        lcdMem.IRC = true;
                    }
                }
                else if (lcdMem.cy == Cpu.DelayStep * 113)
                {
                    lcdMem.LY = 0;
                    lcdMem.cy = -Cpu.DelayStep;
                }
                else
                {
                    // Left intentionally empty
                }
            }

            if (lcdMem.IRC)
            {
                if (!lcdMem.PIRC)
                {
                    lcdMem.PIRC = true;
                    mem.SetMappedMemory(0xFF0F, (byte)(mem.GetMappedMemory(0xFF0F) | 0x2));
                }
            }
            else
            {
                lcdMem.PIRC = false;
            }
            lcdMem.IRC = false;
            lcdMem.cy += Cpu.DelayStep;
        }

        private static void DisableLcd(LcdMemory mem)
        {
            mem.PIRC = false;
            mem.LY = 0;
            mem.cy = 0;
            for (int i = 0; i < mem.displayMemory.Length; i++)
            {
                mem.displayMemory[i] = (int)COLORS[0];
            }
        }

        private static int GetPixelIndex(GbMemory mem, byte y, byte x, ushort baseTileAddress, ushort tileNumber)
        {
            int i = (mem.VRam[(ushort)((tileNumber * 16) + baseTileAddress + (y * 2))].GetBit((byte)(7 - x)) ? 1 : 0);
            return i + (mem.VRam[(ushort)((tileNumber * 16) + baseTileAddress + (y * 2) + 1)].GetBit((byte)(7 - x)) ? 2 : 0);
        }

        private static void RenderLine(GbMemory mem)
        {
            LcdMemory lcdMem = mem.lcdMemory;
            if (lcdMem.cy == 0)
            {
                lcdMem.STAT &= 0xFC;
            }
            else if (lcdMem.cy <= Cpu.DelayStep * 10)
            {
                if (lcdMem.cy == Cpu.DelayStep)
                {
                    lcdMem.STAT = (byte)((lcdMem.STAT & 0xFC) | 0x2);
                    if (lcdMem.LY != 0 && lcdMem.LYC == lcdMem.LY && (lcdMem.STAT.GetBit(6)))
                    {
                        lcdMem.IRC = true;
                    }
                }

                while (lcdMem.ObjAttriMemOffset < (lcdMem.cy / Cpu.DelayStep) * 2)// 2*sprite(4 bytes)
                {
                    lcdMem.pObjAttriMem[lcdMem.ObjAttriMemOffset] =
                        mem.GetMappedMemory((ushort)(0xFE00 + lcdMem.ObjAttriMemOffset));
                    lcdMem.ObjAttriMemOffset++;
                }
            }
            else if (lcdMem.cy == Cpu.DelayStep * 11)
            {
                lcdMem.STAT = (byte)((lcdMem.STAT & 0xFC) | 0x3);

#if NULLRENDERER

#elif PERLINERENDERER
                if (lcdMem.ForceNullRender)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        lcdMem.displayMemory[((Height - lcdMem.LY - 1) * Width) + i] = (int)COLORS[0];
                    }
                }
                else
                {
                    if (lcdMem.Lcdc.GetBit(0))
                    {
                        ScanLine(mem);
                    }

                    if (lcdMem.Lcdc.GetBit(5))
                    {
                        ScanLineWindow(mem);
                    }

                    if (lcdMem.Lcdc.GetBit(1))
                    {
                        ScanLineSprite(mem);
                    }
                }

#endif
            }
            else if (lcdMem.cy < Cpu.DelayStep * 63)
            {
                // Add proper pixel-by-pixel emulation here.
            }
            else if (lcdMem.cy == Cpu.DelayStep * 113)
            {
                lcdMem.LY++;
                lcdMem.cy = -Cpu.DelayStep;
            }
            else
            {
                // Left intentionally empty.
            }
            if ((lcdMem.cy == 0 && lcdMem.LYC == 0) || (lcdMem.cy != 0 && lcdMem.LYC == lcdMem.LY))
            {
                lcdMem.STAT |= 4;
            }
            else
            {
                lcdMem.STAT &= 0xFB;
            }
        }

#if PERLINERENDERER

        private static void ScanLine(GbMemory mem)
        {
            LcdMemory lcdMem = mem.lcdMemory;
            ushort mapOffset = (ushort)(lcdMem.Lcdc.GetBit(3) ? 0x1C00 : 0x1800); // Base offset
            mapOffset += (ushort)((((lcdMem.SCY + lcdMem.LY) & 0xFF) >> 3) * 32);
            byte lineOffset = (byte)(lcdMem.SCX >> 3);
            byte y = (byte)((lcdMem.LY + lcdMem.SCY) & 7);
            byte x = (byte)(lcdMem.SCX & 7);

            ushort tileOffset = (ushort)(lcdMem.Lcdc.GetBit(4) ? 0 : 0x800);
            ushort tile = mem.VRam[(ushort)(lineOffset + mapOffset)];
            if (!lcdMem.Lcdc.GetBit(4) && tile < 128)
            {
                tile += 256;
            }

            for (int i = 0; i < Width; i++)
            {
                // pixel part 1, pixel part 2......>>
                int index = GetPixelIndex(mem, y, x, tileOffset, tile);
                lcdMem.displayMemory[((Height - lcdMem.LY - 1) * Width) + i] =
                    (int)COLORS[(lcdMem.BgPallet >> (index * 2)) & 0x3];

                x++;
                if (x == 8)
                {
                    x = 0;
                    lineOffset = (byte)((lineOffset + 1) & 31);
                    tile = mem.VRam[(ushort)(lineOffset + mapOffset)];
                    if (!lcdMem.Lcdc.GetBit(4) && tile < 128)
                    {
                        tile += 256;
                    }
                }
            }
        }

#endif
#if PERLINERENDERER

        private static void ScanLineSprite(GbMemory mem)
        {
            LcdMemory lcdMem = mem.lcdMemory;
            if (lcdMem.Lcdc.GetBit(2))
            {
                throw new NotSupportedException();
            }

            // FIXME: Doesn't handle overlapping sprites.
            for (int i = 0; i < 40; i++)
            {
                int oamOffset = i * 4;
                byte spriteY = (byte)(mem.Oam[oamOffset] - 16);
                byte spriteX = (byte)(mem.Oam[oamOffset + 1] - 8);
                byte tile = mem.Oam[oamOffset + 2];
                byte flags = mem.Oam[oamOffset + 3];
                if (spriteY <= lcdMem.LY && spriteY + 8 > lcdMem.LY)
                {
                    byte pallet = flags.GetBit(4) ? lcdMem.objPallet1 : lcdMem.objPallet0;
                    int displayOffset = (160 * lcdMem.LY) + spriteX;
                    byte tileY = (byte)(flags.GetBit(6) ? (7 - (lcdMem.LY & 8)) : (lcdMem.LY & 8));
                    for (int x = 0; x < 8; x++)
                    {
                        byte tileX = (flags.GetBit(5) ? (byte)(7 - x) : (byte)x);
                        int index = GetPixelIndex(mem, tileY, tileX, 0x1800, tile);
                        if (spriteX + x < Width && index != 0 &&
                            (flags.GetBit(7) || lcdMem.displayMemory[displayOffset + x] == COLORS[0]))
                        {
                            lcdMem.displayMemory[displayOffset + x] = (int)COLORS[(pallet >> (index * 2)) & 0x3];
                        }
                    }
                }
            }
        }

#endif
#if PERLINERENDERER

        private static void ScanLineWindow(GbMemory mem)
        {
            LcdMemory lcdMem = mem.lcdMemory;
            ushort mapOffset = (ushort)(lcdMem.Lcdc.GetBit(3) ? 0x1C00 : 0x1800); // Base offset
            mapOffset += (ushort)((((lcdMem.WY + lcdMem.LY) & 0xFF) >> 3) * 32);
            byte lineOffset = (byte)(lcdMem.WX >> 3);
            byte y = (byte)((lcdMem.WY + lcdMem.WY) & 7);
            byte x = (byte)(lcdMem.WX & 7);

            ushort tileOffset = (ushort)(lcdMem.Lcdc.GetBit(4) ? 0 : 0x800);
            ushort tile = mem.VRam[(ushort)(lineOffset + mapOffset)];
            if (!lcdMem.Lcdc.GetBit(4) && tile < 128)
            {
                tile += 256;
            }

            for (int i = 0; i < Width; i++)
            {
                int index = GetPixelIndex(mem, y, x, tileOffset, tile);
                lcdMem.displayMemory[((Height - lcdMem.LY - 1) * Width) + i] =
                    (int)COLORS[(lcdMem.BgPallet >> (index * 2)) & 0x3];

                x++;
                if (x == 8)
                {
                    x = 0;
                    lineOffset = (byte)((lineOffset + 1) & 31);
                    tile = mem.GetMappedMemory((ushort)(lineOffset + mapOffset));
                    if (!lcdMem.Lcdc.GetBit(4) && tile < 128)
                    {
                        tile += 256;
                    }
                }
            }
        }

#endif
    }
}
