namespace JAGBE.GB.Emulation
{
    internal delegate bool OpcodeFunc(Opcode code, GbMemory mem, int step);
}
