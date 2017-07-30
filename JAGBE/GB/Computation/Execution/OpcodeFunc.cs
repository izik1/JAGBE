using JAGBE.GB.Computation;

namespace JAGBE.GB.Computation.Execution
{
    internal delegate bool OpcodeFunc(Opcode code, GbMemory mem, int step);
}
