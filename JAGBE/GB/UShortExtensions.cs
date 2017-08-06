namespace JAGBE.GB
{
    internal static class UShortExtensions
    {
        public static bool GetHalfCarry(this ushort a, ushort b) => (((a & 0xFFF) + (b & 0xFFF)) & 0x1000) == 0x1000;
    }
}
