using System;

namespace JAGBE.GB
{
    /// <summary>
    /// Provides extensions for <see cref="byte"/>
    /// </summary>
    internal static class ByteExtensions
    {
        internal static byte[] ToBytes(this Emulation.GbUInt8[] arr) => Array.ConvertAll(arr, u8 => (byte)u8);

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
