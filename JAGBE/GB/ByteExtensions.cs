namespace JAGBE.GB
{
    /// <summary>
    /// Extensions for <see cref="byte"/>
    /// </summary>
    internal static class ByteExtensions
    {
        /// <summary>
        /// masks b with m.
        /// </summary>
        /// <param name="b">The first byte.</param>
        /// <param name="m">The 2nd byte.</param>
        /// <returns><c><paramref name="b"/> &amp; <paramref name="m"/></c></returns>
        internal static byte Mask(this byte b, byte m) => (byte)(b & m);

        /// <summary>
        /// Determines if adding two bytes would produce a half carry
        /// </summary>
        /// <remarks>
        /// 0x0F is the largest value a nibble (4 bits) can hold which means any add that causes 2
        /// nibbles to be &gt; 0xF causes a half carry.
        /// </remarks>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        internal static bool GetHFlag(this byte b1, byte b2) => (((b1 & 0x0F) + (b2 & 0x0F)) & 0x10) == 0x10;

        /// <summary>
        /// Determines if subtracting two bytes would produce a half carry
        /// </summary>
        /// <remarks>
        /// 0x00 is the smallest value a nibble (4 bits) can hold which means any subtraction that
        /// causes 2 nibbles to be &lt; 0x0 causes a half carry. (borrow)
        /// </remarks>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static bool GetHFlagN(this byte a, byte b) => (a & 0xF) - (b & 0xF) < 0;

        /// <summary>
        /// Assigns bit number <paramref name="bit"/> of <paramref name="a"/> to <paramref name="value"/>.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="bit">The bit.</param>
        /// <param name="value">The value to set.</param>
        /// <returns></returns>
        internal static byte AssignBit(this byte a, byte bit, bool value) => (byte)(value ? a | (1 << bit) : a & ~(1 << bit));

        /// <summary>
        /// Gets the specified <paramref name="bit"/> from b.
        /// </summary>
        /// <param name="b">The byte.</param>
        /// <param name="bit">The bit number.</param>
        /// <returns>
        /// <see langword="true"/> if the given <paramref name="bit"/> of <paramref name="b"/> is set
        /// otherwise, <see langword="false"/>
        /// </returns>
        internal static bool GetBit(this byte b, byte bit) => ((b >> bit) & 0x1) == 1;

        /// <summary>
        /// Resets the specified bit.
        /// </summary>
        /// <param name="b">The byte.</param>
        /// <param name="bit">The bit.</param>
        /// <returns>
        /// a byte where bit <paramref name="bit"/> is set to 0 and everything else is the same.
        /// </returns>
        internal static byte Res(this byte b, byte bit) => (byte)(b & ~(1 << bit));

        /// <summary>
        /// Sets the specified bit.
        /// </summary>
        /// <param name="b">The byte.</param>
        /// <param name="bit">The bit.</param>
        /// <returns>
        /// a byte where bit <paramref name="bit"/> is set to 1 and everything else is the same.
        /// </returns>
        internal static byte Set(this byte b, byte bit) => (byte)(b | (1 << bit));
    }
}
