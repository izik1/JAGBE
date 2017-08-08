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
            if (!mem.lcdMemory.Lcdc.GetBit(7))
            {
                if (mem.lcdMemory.LY != 0 || mem.lcdMemory.cy != 0)
                {
                    DisableLcd(mem.lcdMemory);
                }

                return;
            }

            if (mem.lcdMemory.LY < 144)
            {
                RenderLine(mem);
            }
            else if (mem.lcdMemory.LY == 144)
            {
                if (mem.lcdMemory.cy == 0)
                {
                    mem.lcdMemory.STAT &= 0xFC;
                }
                else
                {
                    if (mem.lcdMemory.cy == Cpu.DelayStep)
                    {
                        mem.SetMappedMemory(IoReg.IF, (byte)(mem.GetMappedMemory(IoReg.IF) | 1));
                        mem.lcdMemory.IRC |= mem.lcdMemory.STAT.GetBit(6) && mem.lcdMemory.LYC == 144;
                    }
                    else if (mem.lcdMemory.cy == Cpu.DelayStep * 113)
                    {
                        mem.lcdMemory.LY++;
                        mem.lcdMemory.cy = 0;
                    }
                    else
                    {
                        // Left intentionally empty
                    }
                    mem.lcdMemory.STAT = (byte)((mem.lcdMemory.STAT & 0xF8) | 1 | (mem.lcdMemory.LYC == 144 ? 0x4 : 0));
                }
            }
            else if (mem.lcdMemory.LY < 153)
            {
                if (mem.lcdMemory.cy == Cpu.DelayStep)
                {
                    if (mem.lcdMemory.STAT.GetBit(6) && mem.lcdMemory.LYC == 144)
                    {
                        mem.lcdMemory.IRC = true;
                    }
                }
                else if (mem.lcdMemory.cy == Cpu.DelayStep * 113)
                {
                    mem.lcdMemory.LY++;
                    mem.lcdMemory.cy = 0;
                }
                else
                {
                    // Left intentionally empty
                }
            }
            else
            {
                if (mem.lcdMemory.cy == Cpu.DelayStep)
                {
                    if (mem.lcdMemory.STAT.GetBit(6) && mem.lcdMemory.LYC == 144)
                    {
                        mem.lcdMemory.IRC = true;
                    }
                }
                else if (mem.lcdMemory.cy == Cpu.DelayStep * 113)
                {
                    mem.lcdMemory.LY = 0;
                    mem.lcdMemory.cy = -4;
                }
                else
                {
                    // Left intentionally empty
                }
            }

            if (mem.lcdMemory.IRC)
            {
                if (!mem.lcdMemory.PIRC)
                {
                    mem.lcdMemory.PIRC = true;
                    mem.SetMappedMemory(IoReg.IF, (byte)(mem.GetMappedMemory(IoReg.IF) | 0x2));
                }
            }
            else
            {
                mem.lcdMemory.PIRC = false;
            }
            mem.lcdMemory.IRC = false;
            mem.lcdMemory.cy += Cpu.DelayStep;
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
            int i = (mem.GetMappedMemory((ushort)((tileNumber * 16) + baseTileAddress + (y * 2))).GetBit((byte)(7 - x)) ? 1 : 0);
            return i + (mem.GetMappedMemory((ushort)((tileNumber * 16) + baseTileAddress + (y * 2) + 1)).GetBit((byte)(7 - x)) ? 2 : 0);
        }

        private static void RenderLine(GbMemory mem)
        {
            if (mem.lcdMemory.cy == 0)
            {
                mem.lcdMemory.STAT &= 0xFC;
            }
            else if (mem.lcdMemory.cy <= Cpu.DelayStep * 10)
            {
                if (mem.lcdMemory.cy == Cpu.DelayStep)
                {
                    mem.lcdMemory.STAT = (byte)((mem.lcdMemory.STAT & 0xFC) | 0x2);
                    if (mem.lcdMemory.LY != 0 && mem.lcdMemory.LYC == mem.lcdMemory.LY && (mem.lcdMemory.STAT.GetBit(6)))
                    {
                        mem.lcdMemory.IRC = true;
                    }
                }

                while (mem.lcdMemory.ObjAttriMemOffset < (mem.lcdMemory.cy / Cpu.DelayStep) * 2)// 2*sprite(4 bytes)
                {
                    mem.lcdMemory.pObjAttriMem[mem.lcdMemory.ObjAttriMemOffset] =
                        mem.GetMappedMemory((ushort)(0xFE00 + mem.lcdMemory.ObjAttriMemOffset));
                    mem.lcdMemory.ObjAttriMemOffset++;
                }
            }
            else if (mem.lcdMemory.cy == Cpu.DelayStep * 11)
            {
                mem.lcdMemory.STAT = (byte)((mem.lcdMemory.STAT & 0xFC) | 0x3);

                // HACK: pls do better. (draws the entire line at once)
#if NULLRENDERER

#elif PERLINERENDERER
                if (mem.lcdMemory.ForceNullRender)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        mem.lcdMemory.displayMemory[((Height - mem.lcdMemory.LY - 1) * Width) + i] = (int)COLORS[0];
                    }
                }
                else
                {
                    if (mem.lcdMemory.Lcdc.GetBit(0))
                    {
                        ScanLine(mem);
                    }

                    if (mem.lcdMemory.Lcdc.GetBit(5))
                    {
                        ScanLineWindow(mem);
                    }

                    if (mem.lcdMemory.Lcdc.GetBit(1))
                    {
                        ScanLineSprite(mem);
                    }
                }

#endif
            }
            else if (mem.lcdMemory.cy < Cpu.DelayStep * 63)
            {
                // Add proper pixel-by-pixel emulation here.
            }
            else if (mem.lcdMemory.cy == Cpu.DelayStep * 113)
            {
                mem.lcdMemory.LY++;
                mem.lcdMemory.cy = 0;
            }
            else
            {
                // Left intentionally empty.
            }
            if ((mem.lcdMemory.cy == 0 && mem.lcdMemory.LYC == 0) || (mem.lcdMemory.cy != 0 && mem.lcdMemory.LYC == mem.lcdMemory.LY))
            {
                mem.lcdMemory.STAT |= 4;
            }
            else
            {
                mem.lcdMemory.STAT &= 0xFB;
            }
        }

        private static void ScanLine(GbMemory mem)
        {
#if PERLINERENDERER
            ushort mapOffset = (ushort)(mem.lcdMemory.Lcdc.GetBit(3) ? 0x9C00 : 0x9800); // Base offset
            mapOffset += (ushort)((((mem.lcdMemory.SCY + mem.lcdMemory.LY) & 0xFF) >> 3) * 32);
            byte lineOffset = (byte)(mem.lcdMemory.SCX >> 3);
            byte y = (byte)((mem.lcdMemory.LY + mem.lcdMemory.SCY) & 7);
            byte x = (byte)(mem.lcdMemory.SCX & 7);

            ushort tileOffset = (ushort)(mem.lcdMemory.Lcdc.GetBit(4) ? 0x8000 : 0x8800);
            ushort tile = mem.GetMappedMemory((ushort)(lineOffset + mapOffset));
            if (!mem.lcdMemory.Lcdc.GetBit(4) && tile < 128)
            {
                tile += 256;
            }

            for (int i = 0; i < Width; i++)
            {
                // pixel part 1, pixel part 2......>>
                int index = GetPixelIndex(mem, y, x, tileOffset, tile);
                mem.lcdMemory.displayMemory[((Height - mem.lcdMemory.LY - 1) * Width) + i] =
                    (int)COLORS[(mem.lcdMemory.BgPallet >> (index * 2)) & 0x3];

                x++;
                if (x == 8)
                {
                    x = 0;
                    lineOffset = (byte)((lineOffset + 1) & 31);
                    tile = mem.GetMappedMemory((ushort)(lineOffset + mapOffset));
                    if (!mem.lcdMemory.Lcdc.GetBit(4) && tile < 128)
                    {
                        tile += 256;
                    }
                }
            }
#endif
        }

        private static void ScanLineSprite(GbMemory mem)
        {
#if PERLINERENDERER

            if (mem.lcdMemory.Lcdc.GetBit(2))
            {
                throw new NotSupportedException();
            }

            // FIXME: Doesn't handle overlapping sprites.
            for (int i = 0; i < 40; i++)
            {
                byte spriteY = (byte)(mem.GetMappedMemory((ushort)(0xFE00 + (i * 4))) - 16);
                byte spriteX = (byte)(mem.GetMappedMemory((ushort)(0xFE01 + (i * 4))) - 8);
                byte tile = (mem.GetMappedMemory((ushort)(0xFE02 + (i * 4))));
                byte flags = (mem.GetMappedMemory((ushort)(0xFE03 + (i * 4))));
                if (spriteY <= mem.lcdMemory.LY && spriteY + 8 > mem.lcdMemory.LY)
                {
                    byte pallet = flags.GetBit(4) ? mem.lcdMemory.objPallet1 : mem.lcdMemory.objPallet0;
                    int displayOffset = (160 * mem.lcdMemory.LY) + spriteX;
                    byte tileY = (byte)(flags.GetBit(6) ? (7 - (mem.lcdMemory.LY & 8)) : (mem.lcdMemory.LY & 8));
                    for (int x = 0; x < 8; x++)
                    {
                        byte tileX = (flags.GetBit(5) ? (byte)(7 - x) : (byte)x);
                        int index = GetPixelIndex(mem, tileY, tileX, 0x9800, tile);
                        if (spriteX + x >= 0 && spriteX + x < Width && index != 0 &&
                            (flags.GetBit(7) || mem.lcdMemory.displayMemory[displayOffset + x] == 0))
                        {
                            mem.lcdMemory.displayMemory[displayOffset + x] = (int)COLORS[(pallet >> (index * 2)) & 0x3];
                        }
                    }
                }
            }
#endif
        }

        private static void ScanLineWindow(GbMemory mem)
        {
#if PERLINERENDERER
            ushort mapOffset = (ushort)(mem.lcdMemory.Lcdc.GetBit(3) ? 0x9C00 : 0x9800); // Base offset
            mapOffset += (ushort)((((mem.lcdMemory.WY + mem.lcdMemory.LY) & 0xFF) >> 3) * 32);
            byte lineOffset = (byte)(mem.lcdMemory.WX >> 3);
            byte y = (byte)((mem.lcdMemory.WY + mem.lcdMemory.WY) & 7);
            byte x = (byte)(mem.lcdMemory.WX & 7);

            ushort tileOffset = (ushort)(mem.lcdMemory.Lcdc.GetBit(4) ? 0x8000 : 0x8800);
            ushort tile = mem.GetMappedMemory((ushort)(lineOffset + mapOffset));
            if (!mem.lcdMemory.Lcdc.GetBit(4) && tile < 128)
            {
                tile += 256;
            }

            for (int i = 0; i < Width; i++)
            {
                // pixel part 1, pixel part 2......>>
                int index = GetPixelIndex(mem, y, x, tileOffset, tile);
                mem.lcdMemory.displayMemory[((Height - mem.lcdMemory.LY - 1) * Width) + i] =
                    (int)COLORS[(mem.lcdMemory.BgPallet >> (index * 2)) & 0x3];

                x++;
                if (x == 8)
                {
                    x = 0;
                    lineOffset = (byte)((lineOffset + 1) & 31);
                    tile = mem.GetMappedMemory((ushort)(lineOffset + mapOffset));
                    if (!mem.lcdMemory.Lcdc.GetBit(4) && tile < 128)
                    {
                        tile += 256;
                    }
                }
            }
#endif
        }
    }
}
