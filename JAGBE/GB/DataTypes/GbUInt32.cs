using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAGBE.GB.DataTypes
{
    internal struct GbUInt32
    {
        GbUInt16 HighWord { get => new GbUInt16((ushort)(Value & 0xFFFF0000 >> sizeof(uint) * 8)); }

        GbUInt16 LowWord { get => new GbUInt16((ushort)(Value & 0x0000FFFF)); }

        public GbUInt32(uint value) => this.Value = value;

        public GbUInt32(GbUInt16 highWord, GbUInt16 lowWord) => this.Value = ((highWord << sizeof(ushort) * 2) + lowWord);

        public uint Value { get; }
    }
}
