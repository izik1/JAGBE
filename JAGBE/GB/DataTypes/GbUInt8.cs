using System;

namespace JAGBE.GB.DataTypes
{
    /// <summary>
    /// A struct for 8-bit unsigned integers, for use in Game Boy emulation.
    /// </summary>
    /// <seealso cref="IEquatable{GbUInt8}"/>
    /// <seealso cref="IFormattable"/>
    public struct GbUInt8 : IEquatable<GbUInt8>, IFormattable
    {
        /// <summary>
        /// Performs an explicit conversion from <see cref="ushort"/> to <see cref="GbUInt8"/>.
        /// </summary>
        /// <param name="u16">The u16.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator GbUInt8(ushort u16) => (byte)u16;

        /// <summary>
        /// Performs an implicit conversion from <see cref="GbUInt8"/> to <see cref="ushort"/>.
        /// </summary>
        /// <param name="u8">The u8.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator ushort(GbUInt8 u8) => (byte)u8;

        /// <summary>
        /// Performs an implicit conversion from <see cref="GbUInt8"/> to <see cref="byte"/>.
        /// </summary>
        /// <param name="u8">The u8.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator byte(GbUInt8 u8) => u8.value;

        /// <summary>
        /// Performs an implicit conversion from <see cref="byte"/> to <see cref="GbUInt8"/>.
        /// </summary>
        /// <param name="u8">The u8.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator GbUInt8(byte u8) => new GbUInt8(u8);

        /// <summary>
        /// Performs an implicit conversion from <see cref="GbUInt8"/> to <see cref="GbUInt16"/>.
        /// </summary>
        /// <param name="u8">The u8.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator GbUInt16(GbUInt8 u8) => new GbUInt16(0, u8.value);

        /// <summary>
        /// Performs an explicit conversion from <see cref="int"/> to <see cref="GbUInt8"/>.
        /// </summary>
        /// <param name="i32">The i32.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator GbUInt8(int i32) => (byte)i32;

        /// <summary>
        /// Performs an explicit conversion from <see cref="GbUInt16"/> to <see cref="GbUInt8"/>.
        /// </summary>
        /// <param name="u16">The u16.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator GbUInt8(GbUInt16 u16) => u16.LowByte;

        /// <summary>
        /// Represents the largest possible value of a <see cref="GbUInt8"/>.
        /// </summary>
        public static readonly GbUInt8 MaxValue = new GbUInt8(byte.MaxValue);

        /// <summary>
        /// Represents the smallest possible value of a <see cref="GbUInt8"/>.
        /// </summary>
        public static readonly GbUInt8 MinValue = new GbUInt8(byte.MinValue);

        /// <summary>
        /// The value
        /// </summary>
        private readonly byte value;

        /// <summary>
        /// Initializes a new instance of the <see cref="GbUInt8"/> struct.
        /// </summary>
        /// <param name="u8">The u8.</param>
        public GbUInt8(byte u8) => this.value = u8;

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="u8a">The u8a.</param>
        /// <param name="u8b">The u8b.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(GbUInt8 u8a, GbUInt8 u8b) => !(u8a.value == u8b.value);

        /// <summary>
        /// Implements the operator &amp;.
        /// </summary>
        /// <param name="u8a">The u8a.</param>
        /// <param name="u8b">The u8b.</param>
        /// <returns>The result of the operator.</returns>
        public static GbUInt8 operator &(GbUInt8 u8a, GbUInt8 u8b) => (GbUInt8)(u8a.value & u8b.value);

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="u8a">The u8a.</param>
        /// <param name="u8b">The u8b.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(GbUInt8 u8a, GbUInt8 u8b) => u8a.value == u8b.value;

        /// <summary>
        /// Implements the operator &gt;&gt;.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="bits">The bits.</param>
        /// <returns>The result of the operator.</returns>
        public static GbUInt8 operator >>(GbUInt8 value, int bits) => (GbUInt8)(value.value >> bits);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(GbUInt8 other) => this.value == other.value;

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> and this instance are the same type and
        /// represent the same value; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj) => obj is GbUInt8 u8 && Equals(u8);

        /// <summary>
        /// Gets the specified bit <paramref name="bitNum"/> from this instance.
        /// </summary>
        /// <param name="bitNum">The bit number.</param>
        /// <returns>
        /// <see langword="true"/> if the given <paramref name="bitNum"/> of this instance is set
        /// otherwise, <see langword="false"/>
        /// </returns>
        public bool GetBit(GbUInt8 bitNum) => ((this >> bitNum.value) & 1) == 1;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() => this.value;

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString() => ToString("G", null);

        /// <summary>
        /// Formats the value of the current instance using the specified format.
        /// </summary>
        /// <param name="format">The format to use.-or- A null reference to use the default format</param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public string ToString(string format) => this.ToString(format, null);

        /// <summary>
        /// Formats the value of the current instance using the specified format.
        /// </summary>
        /// <param name="format">
        /// The format to use.-or- A null reference to use the default format defined for the type of
        /// the <see cref="IFormattable"/> implementation.
        /// </param>
        /// <param name="formatProvider">
        /// The provider to use to format the value.-or- A null reference to obtain the numeric
        /// format information from the current locale setting of the operating system.
        /// </param>
        /// <returns>The value of the current instance in the specified format.</returns>
        public string ToString(string format, IFormatProvider formatProvider) => this.value.ToString(format, formatProvider);
    }
}
