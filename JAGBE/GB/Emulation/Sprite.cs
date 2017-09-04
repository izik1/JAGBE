using System;

namespace JAGBE.GB.Emulation
{
    internal struct Sprite : IEquatable<Sprite>
    {
        /// <summary>
        /// The Flags of this sprite.
        /// </summary>
        internal readonly GbUInt8 Flags;

        /// <summary>
        /// The number of the tile that this sprite uses.
        /// </summary>
        internal readonly GbUInt8 Tile;

        /// <summary>
        /// The offset from the left side of the screen in pixels.
        /// </summary>
        internal readonly byte X;

        /// <summary>
        /// The offset from the top of the screen in pixels.
        /// </summary>
        internal readonly byte Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sprite"/> struct.
        /// </summary>
        /// <param name="y">The y.</param>
        /// <param name="x">The x.</param>
        /// <param name="tile">The tile.</param>
        /// <param name="flags">The flags.</param>
        public Sprite(byte y, byte x, GbUInt8 tile, GbUInt8 flags)
        {
            this.Y = y;
            this.X = x;
            this.Tile = tile;
            this.Flags = flags;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another <see cref="Sprite"/>.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(Sprite other) => this.Flags == other.Flags && this.Tile == other.Tile && this.X == other.X && this.Y == other.Y;
    }
}
