using System;

namespace JAGBE.GB.DataTypes
{
    /// <summary>
    /// Keeps track of a GameBoy's registers
    /// </summary>
    internal sealed class GbRegisters
    {
        /// <summary>
        /// Gets or sets the AF register.
        /// </summary>
        internal GbUInt16 Af { get; set; }

        /// <summary>
        /// Gets or sets the BC register.
        /// </summary>
        internal GbUInt16 Bc { get; set; }

        /// <summary>
        /// Gets or sets the DE register.
        /// </summary>
        internal GbUInt16 De { get; set; }

        /// <summary>
        /// Gets or sets the HL register.
        /// </summary>
        internal GbUInt16 Hl { get; set; }

        /// <summary>
        /// Gets or sets the Stack Pointer.
        /// </summary>
        internal GbUInt16 Sp { get; set; }

        /// <summary>
        /// Gets or sets the Program Counter.
        /// </summary>
        internal GbUInt16 Pc { get; set; }

        /// <summary>
        /// Gets or sets the A register.
        /// </summary>
        internal byte A
        {
            get => Af.HighByte;
            set => Af = new GbUInt16(value, Af.LowByte);
        }

        /// <summary>
        /// Gets or sets the F register.
        /// </summary>
        internal byte F
        {
            get => Af.LowByte;
            set => Af = new GbUInt16(Af.HighByte, value);
        }

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

                case 7:
                    return this.A;

                default:
                    throw new ArgumentException(nameof(index));
            }
        }

        /// <summary>
        /// Gets the R16.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="UseAf">if set to <c>true</c> returns AF instead of SP.</param>
        public GbUInt16 GetR16(int index, bool UseAf)
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
                    return UseAf ? this.Af : this.Sp;

                default:
                    throw new ArgumentException(nameof(index));
            }
        }

        /// <summary>
        /// Sets the R16.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="UseAf">if set to <c>true</c> returns AF instead of SP.</param>
        /// <param name="value"></param>
        public void SetR16(int index, GbUInt16 value, bool UseAf)
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
                    if (UseAf)
                    {
                        this.Af = value;
                    }
                    else
                    {
                        this.Sp = value;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

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
