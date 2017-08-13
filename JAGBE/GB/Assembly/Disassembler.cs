using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JAGBE.GB.Computation;
using JAGBE.GB.DataTypes;

namespace JAGBE.GB.Assembly
{
    internal static class Disassembler
    {
        private const string Regs = "BCDEHL_A"; // index 6 ( _ ) is unused.

        public static string DisassembleInstruction(GbMemory memory)
        {
            byte b = memory.GetMappedMemory(memory.R.Pc);
            if (b == 0xCB)
            {
                return DisassenbleCb(memory);
            }

            if (b == 0)
            {
                return "NOP";
            }

            if (b == 7)
            {
                return "RCLA";
            }

            if (b == 0xF)
            {
                return "RCRA";
            }

            if (b == 0x17)
            {
                return "RLA";
            }

            if (b == 0x1F)
            {
                return "RRA";
            }

            byte dest = (byte)((b >> 3) & 7);
            byte src = (byte)(b & 7);

            string Arithmetic(string baseName) => baseName + " " +
                (src == 6 ? "(" + memory.R.Hl.ToString("X4") + ")" : Regs[src].ToString());

            switch (b & 0xF0)
            {
                case 0x40:
                case 0x50:
                case 0x60:
                case 0x70:
                    if (b == 0x76)
                    {
                        return "HALT";
                    }
                    return "LD " + (dest == 6 ? "(" + memory.R.Hl.ToString("X4") + "), " : Regs[dest].ToString() + ", ") +
                        (src == 6 ? "(" + memory.R.Hl.ToString("X4") + ")" : Regs[src].ToString());

                case 0x80:
                    return Arithmetic(!b.GetBit(3) ? "ADD" : "ADC");

                case 0x90:
                    return Arithmetic(!b.GetBit(3) ? "SUB" : "SBC");

                case 0xA0:
                    return Arithmetic(!b.GetBit(3) ? "AND" : "XOR");

                case 0xB0:
                    return Arithmetic(!b.GetBit(3) ? "OR" : "CP");

                default:
                    return UnknownInstruction(b);
            }
        }

        private static string DisassenbleCb(GbMemory memory)
        {
            byte b = memory.GetMappedMemory(memory.R.Pc + 1);
            byte src = (byte)(b & 7);
            byte dest = (byte)((b >> 3) & 7);
            string BrsString(string baseName) => baseName + " " + dest.ToString() +
                        (src == 6 ? (", (" + memory.R.Hl.ToString("X4") + ")") : ", " + Regs[src].ToString());
            string BitString(string baseName) => baseName + " " +
                (src == 6 ? (", (" + memory.R.Hl.ToString("X4") + ")") : ", " + Regs[src].ToString());
            switch (b & 0xF0)
            {
                case 0x00:
                    return BitString(!b.GetBit(3) ? "RLC" : "RRC");

                case 0x10:
                    return BitString(!b.GetBit(3) ? "RL" : "RR");

                case 0x20:
                    return BitString(!b.GetBit(3) ? "SLA" : "SRA");

                case 0x30:
                    return BitString(!b.GetBit(3) ? "SWAP" : "SRL");

                case 0x40:
                case 0x50:
                case 0x60:
                case 0x70:
                    return BrsString("BIT");

                case 0x80:
                case 0x90:
                case 0xA0:
                case 0xB0:
                    return BrsString("RES");

                case 0xC0:
                case 0xD0:
                case 0xE0:
                case 0xF0:
                    return BrsString("SET");

                default:
                    return UnknownInstructionCb(b);
            }
        }

        private static string UnknownInstruction(byte instruction) => "Unknown Instruction {0x" + instruction.ToString("X2") + "}";

        private static string UnknownInstructionCb(byte instruction) => "Unknown Instruction {0xCB" + instruction.ToString("X2") + "}";

        internal static string DisassembleAllInstructions()
        {
            GbMemory m = new GbMemory
            {
                Rom = new byte[3]
            };
            StringBuilder builder = new StringBuilder(256 * 256 * 10);
            m.SetMappedMemory(0xFF50, 1); // disable boot rom.
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    m.Rom[0] = (byte)i;
                    m.Rom[1] = (byte)j;
                    builder.AppendLine(DisassembleInstruction(m));
                    if (i != 0xCB)
                    {
                        break;
                    }
                }
            }

            return builder.ToString();
        }
    }
}
