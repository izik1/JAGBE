#define PERLINERENDERER

using System;
using JAGBE.GB.DataTypes;

namespace JAGBE.GB.Emulation
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
            if (!lcdMem.Lcdc[7])
            {
                if (lcdMem.LY != 0 || lcdMem.cy != 0)
                {
                    DisableLcd(lcdMem);
                }

                return;
            }

            if (lcdMem.LY < 144)
            {
                RenderLine(lcdMem, mem.VRam, mem.Oam);
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
                        mem.IF |= 1;
                        lcdMem.IRC |= lcdMem.STAT[6] && lcdMem.LYC == 144;
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
                    lcdMem.STAT = (byte)((lcdMem.STAT & 0xF8) | 1 | (lcdMem.LYC == 144 ? 0x4 : 0));
                }
            }
            else if (lcdMem.LY < 153)
            {
                if (lcdMem.cy == Cpu.DelayStep)
                {
                    if (lcdMem.STAT[6] && lcdMem.LYC == 144)
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
                    if (lcdMem.STAT[6] && lcdMem.LYC == 144)
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
                    mem.IF |= 2;
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

        private static int GetColorIndex(GbUInt8 pallet, GbUInt8[] VRam, byte y, byte x, ushort tileMapOffset, ushort tileNum)
        {
            int i = (VRam[(tileNum * 16) + tileMapOffset + (y * 2)][(byte)(7 - x)] ? 1 : 0);
            i += (VRam[(tileNum * 16) + tileMapOffset + (y * 2) + 1][(byte)(7 - x)] ? 2 : 0);
            return (pallet >> (i * 2)) & 3;
        }

        private static void RenderLine(LcdMemory lcdMem, GbUInt8[] VRam, GbUInt8[] Oam)
        {
            if (lcdMem.cy == 0)
            {
                lcdMem.STAT &= 0xFC;
            }
            else if (lcdMem.cy <= Cpu.DelayStep * 10)
            {
                if (lcdMem.cy == Cpu.DelayStep)
                {
                    lcdMem.STAT = ((lcdMem.STAT & 0xFC) | 2);
                    if (lcdMem.LY != 0 && lcdMem.LYC == lcdMem.LY && (lcdMem.STAT[6]))
                    {
                        lcdMem.IRC = true;
                    }
                }
            }
            else if (lcdMem.cy == Cpu.DelayStep * 11)
            {
                lcdMem.STAT = (byte)((lcdMem.STAT & 0xFC) | 3);

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
                    if (lcdMem.Lcdc[0])
                    {
                        ScanLine(lcdMem, VRam);
                    }

                    if (lcdMem.Lcdc[5])
                    {
                        ScanLineWindow(lcdMem, VRam);
                    }

                    if (lcdMem.Lcdc[1])
                    {
                        ScanLineSprite(lcdMem, VRam, Oam);
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

        private static void ScanLine(LcdMemory lcdMem, GbUInt8[] VRam)
        {
            // Offset from the start of VRAM to the start of the background map.
            ushort mapOffset = (ushort)(lcdMem.Lcdc[3] ? 0x1C00 : 0x1800);
            mapOffset += (ushort)((((lcdMem.SCY + lcdMem.LY) & 0xFF) >> 3) * 32);

            byte lineOffset = (byte)(lcdMem.SCX >> 3);
            byte y = (byte)((lcdMem.LY + lcdMem.SCY) & 7);
            byte x = (byte)(lcdMem.SCX & 7);

            ushort tileOffset = (ushort)(lcdMem.Lcdc[4] ? 0 : 0x800);
            ushort tile = VRam[(ushort)(lineOffset + mapOffset)];
            if (!lcdMem.Lcdc[4] && tile < 128)
            {
                tile += 256;
            }

            for (int i = 0; i < Width; i++)
            {
                lcdMem.displayMemory[((Height - lcdMem.LY - 1) * Width) + i] =
                    (int)COLORS[GetColorIndex(lcdMem.BgPallet, VRam, y, x, tileOffset, tile)];
                x++;
                if (x == 8)
                {
                    x = 0;
                    lineOffset = (byte)((lineOffset + 1) & 31);
                    tile = VRam[(ushort)(lineOffset + mapOffset)];
                    if (!lcdMem.Lcdc[4] && tile < 128)
                    {
                        tile += 256;
                    }
                }
            }
        }

#endif
#if PERLINERENDERER

        private static void ScanLineSprite(LcdMemory lcdMem, GbUInt8[] VRam, GbUInt8[] Oam)
        {
            if (lcdMem.Lcdc[2])
            {
                throw new NotSupportedException();
            }

            // FIXME: Doesn't handle overlapping sprites.
            for (int i = 0; i < 40; i++)
            {
                int oamOffset = i * 4;
                byte spriteY = (byte)(Oam[oamOffset] - 16);
                byte spriteX = (byte)(Oam[oamOffset + 1] - 8);
                GbUInt8 tile = Oam[oamOffset + 2];
                GbUInt8 flags = Oam[oamOffset + 3];
                if (spriteY <= lcdMem.LY && spriteY + 8 > lcdMem.LY)
                {
                    GbUInt8 pallet = flags[4] ? lcdMem.objPallet1 : lcdMem.objPallet0;
                    int displayOffset = ((Height - lcdMem.LY - 1) * Width) + spriteX;
                    byte tileY = (byte)(flags[6] ? (7 - (lcdMem.LY & 7)) : (lcdMem.LY & 7));
                    for (int x = 0; x < 8; x++)
                    {
                        int colorIndex = GetColorIndex(pallet, VRam, tileY, (byte)(flags[5] ? (7 - x) : x), 0, tile);
                        if (spriteX + x < Width && colorIndex != 0 && (!flags[7] || lcdMem.displayMemory[displayOffset + x] == (int)COLORS[0]))
                        {
                            lcdMem.displayMemory[displayOffset + x] = (int)COLORS[colorIndex];
                        }
                    }
                }
            }
        }

#endif
#if PERLINERENDERER

        private static void ScanLineWindow(LcdMemory lcdMem, GbUInt8[] VRam)
        {
            ushort mapOffset = (ushort)(lcdMem.Lcdc[3] ? 0x1C00 : 0x1800); // Base offset
            mapOffset += (ushort)((((lcdMem.WY + lcdMem.LY) & 0xFF) >> 3) * 32);
            byte lineOffset = (byte)(lcdMem.WX >> 3);
            byte y = (byte)((lcdMem.WY + lcdMem.WY) & 7);
            byte x = (byte)(lcdMem.WX & 7);

            ushort tileOffset = (ushort)(lcdMem.Lcdc[4] ? 0 : 0x800);
            ushort tile = VRam[(ushort)(lineOffset + mapOffset)];
            if (!lcdMem.Lcdc[4] && tile < 128)
            {
                tile += 256;
            }

            for (int i = 0; i < Width; i++)
            {
                lcdMem.displayMemory[((Height - lcdMem.LY - 1) * Width) + i] =
                    (int)COLORS[GetColorIndex(lcdMem.BgPallet, VRam, y, x, tileOffset, tile)];

                x++;
                if (x == 8)
                {
                    x = 0;
                    lineOffset = (byte)((lineOffset + 1) & 31);
                    tile = VRam[(ushort)(lineOffset + mapOffset)];
                    if (!lcdMem.Lcdc[4] && tile < 128)
                    {
                        tile += 256;
                    }
                }
            }
        }

#endif
    }
}
