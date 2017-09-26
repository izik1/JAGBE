namespace JAGBE.GB.Emulation
{
    internal static class ByteExtensions
    {
        internal static bool GetBit(this byte b, int bit) => (b & (1 << bit)) > 0;

        internal static byte AssignBit(this byte b, int bit, bool value) => (byte)(value ? b | (1 << bit) : Res(b, bit));

        internal static byte Res(this byte b, int bit) => (byte)(b & ~(1 << bit));
    }
}
