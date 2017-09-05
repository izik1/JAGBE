using System;

namespace JAGBE.GB.Emulation
{
    /// <summary>
    /// A struct for 16-bit unsigned integers with direct access to the high and low bytes.
    /// </summary>
    public struct GbUInt16 : IEquatable<GbUInt16>, IFormattable
    {
        /// <summary>
        /// determines weather adding <paramref name="val"/> to this instance would produce a half carry.
        /// </summary>
        /// <param name="val">The second value.</param>
        /// <returns></returns>
        public bool GetHalfCarry(GbUInt16 val) => (((this & 0xFFF) + (val & 0xFFF)) & 0x1000) == 0x1000;

        /// <summary>
        /// Performs an implicit conversion from <see cref="GbUInt16"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="u16">The u16.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator int(GbUInt16 u16) => u16.value;

        /// <summary>
        /// Performs an implicit conversion from <see cref="ushort"/> to <see cref="GbUInt16"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator GbUInt16(ushort value) => new GbUInt16(value);

        /// <summary>
        /// Performs an explicit conversion from <see cref="GbUInt16"/> to <see cref="ushort"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator ushort(GbUInt16 value) => value.value;

        /// <summary>
        /// Performs an explicit conversion from <see cref="int"/> to <see cref="GbUInt16"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator GbUInt16(int value) => new GbUInt16((ushort)value);

        /// <summary>
        /// The value of this instance
        /// </summary>
        private readonly ushort value;

        /// <summary>
        /// Initializes a new instance of the <see cref="GbUInt16"/> struct.
        /// </summary>
        /// <param name="value">The value of this instance.</param>
        public GbUInt16(ushort value) => this.value = value;

        /// <summary>
        /// Initializes a new instance of the <see cref="GbUInt16"/> struct.
        /// </summary>
        /// <param name="highByte">The high byte of this instance.</param>
        /// <param name="lowByte">The low byte of this instance.</param>
        public GbUInt16(GbUInt8 highByte, GbUInt8 lowByte) => this.value = (ushort)((highByte << 8) | lowByte);

        /// <summary>
        /// The high byte of this instance's value
        /// </summary>
        public GbUInt8 HighByte => (GbUInt8)(this.value >> 8);

        /// <summary>
        /// The low byte of this instance's value
        /// </summary>
        public GbUInt8 LowByte => (GbUInt8)(this.value & 0xFF);

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="left">The left hand side.</param>
        /// <param name="right">The right hand side.</param>
        /// <returns>The result of the operator.</returns>
        public static GbUInt16 operator -(GbUInt16 left, GbUInt16 right) => (GbUInt16)(left.value - right.value);

        /// <summary>
        /// Implements the operator --.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the operator.</returns>
        public static GbUInt16 operator --(GbUInt16 value) => (GbUInt16)(value.value - 1);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left hand side.</param>
        /// <param name="right">The right hand side.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(GbUInt16 left, GbUInt16 right) => !left.Equals(right);

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="left">The left hand side.</param>
        /// <param name="right">The right hand side.</param>
        /// <returns>The result of the operator.</returns>
        public static GbUInt16 operator +(GbUInt16 left, GbUInt16 right) => (GbUInt16)(left.value + right.value);

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="left">The left hand side.</param>
        /// <param name="right">The right hand side.</param>
        /// <returns>The result of the operator.</returns>
        public static GbUInt16 operator +(GbUInt16 left, sbyte right) => (GbUInt16)(left.value + right);

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="left">The left hand side.</param>
        /// <param name="right">The right hand side.</param>
        /// <returns>The result of the operator.</returns>
        public static GbUInt16 operator +(sbyte left, GbUInt16 right) => (GbUInt16)(left + right.value);

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="left">The left hand side.</param>
        /// <param name="right">The right hand side.</param>
        /// <returns>The result of the operator.</returns>
        public static int operator +(GbUInt16 left, int right) => left.value + right;

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="left">The left hand side.</param>
        /// <param name="right">The right hand side.</param>
        /// <returns>The result of the operator.</returns>
        public static int operator +(int left, GbUInt16 right) => left + right.value;

        /// <summary>
        /// Implements the operator ++.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the operator.</returns>
        public static GbUInt16 operator ++(GbUInt16 value) => (GbUInt16)(value + 1);

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left hand side.</param>
        /// <param name="right">The right hand side.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(GbUInt16 left, GbUInt16 right) => left.Equals(right);

        /// <summary>
        /// Determines whether the specified <see cref="object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="object"/> is equal to this instance;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj) => obj is GbUInt16 && this.Equals((GbUInt16)obj);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(GbUInt16 other) => this.value == other.value;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures
        /// like a hash table.
        /// </returns>
        public override int GetHashCode() => this.value;

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public override string ToString() => this.ToString("G", null);

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format of the string.</param>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public string ToString(string format) => this.ToString(format, null);

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public string ToString(string format, IFormatProvider formatProvider) => this.value.ToString(format, formatProvider);
    }
}
