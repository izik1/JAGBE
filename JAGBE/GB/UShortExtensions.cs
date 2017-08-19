namespace JAGBE.GB
{
    /// <summary>
    /// Provides extension methods for <see cref="ushort"/>
    /// </summary>
    internal static class UShortExtensions
    {
        /// <summary>
        /// determines weather adding two ushorts would produce a half carry.
        /// </summary>
        /// <param name="u">The first value.</param>
        /// <param name="val">The second value.</param>
        /// <returns></returns>
        public static bool GetHalfCarry(this ushort u, ushort val) => (((u & 0xFFF) + (val & 0xFFF)) & 0x1000) == 0x1000;
    }
}
