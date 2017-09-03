using System;

namespace JAGBE.GB.Emulation
{
    internal struct Sprite : IEquatable<Sprite>
    {
        internal readonly GbUInt8 Flags;
        internal readonly GbUInt8 Tile;
        internal readonly byte X;
        internal readonly byte Y;

        public Sprite(byte y, byte x, GbUInt8 tile, GbUInt8 flags)
        {
            this.Y = y;
            this.X = x;
            this.Tile = tile;
            this.Flags = flags;
        }

        public bool Equals(Sprite other) => this.Flags == other.Flags && this.Tile == other.Tile && this.X == other.X && this.Y == other.Y;
    }
}
