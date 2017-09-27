using System;

namespace JAGBE.GB.Emulation
{
    /// <summary>
    /// Keeps track of a GameBoy's registers
    /// </summary>
#pragma warning disable S3898 // Value types should implement "IEquatable<T>"

    internal struct GbRegisters
#pragma warning restore S3898 // Value types should implement "IEquatable<T>"
    {
        /// <summary>
        /// Gets or sets the A register.
        /// </summary>
        internal byte A;

        /// <summary>
        /// Gets or sets the BC register.
        /// </summary>
        internal GbUInt16 Bc;

        /// <summary>
        /// Gets or sets the DE register.
        /// </summary>
        internal GbUInt16 De;

        /// <summary>
        /// Gets or sets the F register.
        /// </summary>
        internal byte F;

        /// <summary>
        /// Gets or sets the HL register.
        /// </summary>
        internal GbUInt16 Hl;

        /// <summary>
        /// Gets or sets the Program Counter.
        /// </summary>
        internal GbUInt16 Pc;

        /// <summary>
        /// Gets or sets the Stack Pointer.
        /// </summary>
        internal GbUInt16 Sp;

        /// <summary>
        /// Gets or sets the B register.
        /// </summary>
        internal byte B
        {
            get => Bc.HighByte;
            set => Bc = new GbUInt16(value, Bc.LowByte);
        }

        /// <summary>
        /// Gets or sets the C register.
        /// </summary>
        internal byte C
        {
            get => Bc.LowByte;
            set => Bc = new GbUInt16(Bc.HighByte, value);
        }

        /// <summary>
        /// Gets or sets the D register.
        /// </summary>
        internal byte D
        {
            get => De.HighByte;
            set => De = new GbUInt16(value, De.LowByte);
        }

        /// <summary>
        /// Gets or sets the E register.
        /// </summary>
        internal byte E
        {
            get => De.LowByte;
            set => De = new GbUInt16(De.HighByte, value);
        }

        /// <summary>
        /// Gets or sets the H register.
        /// </summary>
        internal byte H
        {
            get => Hl.HighByte;
            set => Hl = new GbUInt16(value, Hl.LowByte);
        }

        /// <summary>
        /// Gets or sets the L register.
        /// </summary>
        internal byte L
        {
            get => Hl.LowByte;
            set => Hl = new GbUInt16(Hl.HighByte, value);
        }

        public GbUInt16 GetR16Af(int index)
        {
            switch (index)
            {
                case 0:
                    return this.Bc;

                case 1:
                    return this.De;

                case 2:
                    return this.Hl;

                case 3:
                    return new GbUInt16(this.A, this.F);

                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Gets the 16-bit register at <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="index"/> is less than 0 or greater than 3
        /// </exception>
        public GbUInt16 GetR16Sp(int index)
        {
            switch (index)
            {
                case 0:
                    return this.Bc;

                case 1:
                    return this.De;

                case 2:
                    return this.Hl;

                case 3:
                    return this.Sp;

                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Gets the 8-bit register at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"><paramref name="index"/> == 6</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero or greater than seven
        /// </exception>
        public byte GetR8(int index)
        {
            switch (index)
            {
                case 0:
                    return this.B;

                case 1:
                    return this.C;

                case 2:
                    return this.D;

                case 3:
                    return this.E;

                case 4:
                    return this.H;

                case 5:
                    return this.L;

                case 6:
                    throw new ArgumentException(nameof(index));

                case 7:
                    return this.A;

                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Sets the 16-bit register at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="index"/> is less than 0 or greater than 3
        /// </exception>
        public void SetR16(int index, GbUInt16 value)
        {
            switch (index)
            {
                case 0:
                    this.Bc = value;
                    break;

                case 1:
                    this.De = value;
                    break;

                case 2:
                    this.Hl = value;
                    break;

                case 3:
                    this.Sp = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Sets the 8-bit register at <paramref name="index"/> to <paramref name="value"/>.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"><paramref name="index"/> == 6</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero or greater than seven
        /// </exception>
        public void SetR8(int index, byte value)
        {
            switch (index)
            {
                case 0:
                    this.B = value;
                    break;

                case 1:
                    this.C = value;
                    break;

                case 2:
                    this.D = value;
                    break;

                case 3:
                    this.E = value;
                    break;

                case 4:
                    this.H = value;
                    break;

                case 5:
                    this.L = value;
                    break;

                case 6:
                    throw new ArgumentException(nameof(index));

                case 7:
                    this.A = value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
