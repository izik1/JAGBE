using System;
using JAGBE.Logging;

namespace JAGBE.GB.Emulation
{
    /// <summary>
    /// A static class for handling cartrage related functions.
    /// </summary>
    internal static class Cart
    {
        public static void CopyRom(byte[] bootRom, byte[] cartRom, GbMemory mem)
        {
            Buffer.BlockCopy(bootRom, 0, mem.BootRom, 0, 0x100);
            if (mem.Rom.Length < cartRom.Length)
            {
                Logger.LogError("Given rom is bigger than it says it should be.");
                throw new InvalidOperationException();
            }

            Buffer.BlockCopy(cartRom, 0, mem.Rom, 0, cartRom.Length); // Buffer copy because I guess it might be faster?
            if (mem.Rom.Length > cartRom.Length)
            {
                Logger.LogWarning("Given rom is shorter than it says it is, 0xFF filling leftover space.");
                for (int i = cartRom.Length; i < mem.Rom.Length; i++)
                {
                    mem.Rom[i] = 0xFF;
                }
            }
        }

        public static int GetRamSize(byte headerRamSize)
        {
            switch (headerRamSize)
            {
                case 01: return 0x800; // 2KB
                case 02: return MemoryRange.ERAMBANKSIZE;
                case 03: return MemoryRange.ERAMBANKSIZE * 4;
                case 04: return MemoryRange.ERAMBANKSIZE * 16;
                case 05: return MemoryRange.ERAMBANKSIZE * 8;
                default: return 0;
            }
        }
    }
}
