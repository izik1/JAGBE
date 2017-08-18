using System;
using JAGBE.GB.Computation;

namespace JAGBE.GB.Assembly
{
    internal static class Disassembler
    {
        private const string Reg8 = "BCDEHL_A";

        private const string ARITH = "ADDADCSUBSBCANDXOROR CP ";

        private const string CBOPS = "RLC RRC RL  RR  SLA SRA SWAPSRL ";

        private static readonly string[] nmOpStrings =
        {
            // 0x00
            "NOP", "LD BC,d16", "LD (BC),A", "INC BC", "INC B", "DEC B", "LD B,d8", "RLCA",
            "LD (a16),SP", "ADD HL,BC", "LD A,(BC)", "DEC BC", "INC C", "DEC C", "LD C,d8", "RRCA",
            "STOP", "LD DE,d16", "LD (DE),A", "INC DE", "INC D", "DEC D", "LD D,d8", "RLA",
            "JR r8", "ADD HL,DE", "LD A,(DE)", "DEC DE", "INC E", "DEC E", "LD E,d8", "RRA",
            "JR NZ,r8", "LD HL,d16", "LD (HL+),A", "INC HL", "INC H", "DEC H", "LD H,d8", "DAA",
            "JR Z,r8", "ADD HL,HL", "LD A,(HL+)", "DEC HL", "INC L", "DEC L", "LD L,d8", "CPL",
            "JR NC,r8", "LD SP,d16", "LD (HL-),A", "INC SP", "INC (HL)", "DEC (HL)", "LD (HL),d8", "SCF",
            "JR C,r8"    , "ADD HL,SP", "LD A,(HL-)", "DEC SP", "INC A"   , "DEC A"   , "LD A,d8"   , "CCF",

            // 0x40
            "", "", "" ,"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",

            // 0xC0
            "RET NZ", "POP BC", "JP NZ,a16", "JP a16", "CALL NZ,a16", "PUSH BC", "ADD A,d8", "RST 00h",
            "RET Z", "RET", "JP Z, a16", "<CB>", "CALL Z, a16", "CALL a16", "ADC A,d8", "RST 08h",
            "RET NC", "POP DE", "JP NC,a16", "UNUSED", "CALL NC,a16", "PUSH DE", "SUB A,d8", "RST 10H",
            "RET C", "RETI", "JP C,a16", "UNUSED", "CALL C,a16", "UNUSED", "SBC A,d8", "RST 18H",
            "LDH (a8),A", "POP HL", "LD (C),A", "UNUSED", "UNUSED", "PUSH HL", "AND A,d8", "RST 20H",
            "ADD SP,r8", "JP (HL)", "LD (a16),A", "UNUSED", "UNUSED", "UNUSED", "XOR A,d8", "RST 28H",
            "LDH A,(a8)", "POP AF", "LD A,(C)", "DI", "UNUSED", "PUSH AF", "OR A,d8", "RST 30H",
            "LD HL,SP+r8", "LD SP,HL", "LD A,(a16)", "EI", "UNUSED", "UNUSED", "CP A,d8", "RST 38H",
        };

        private static string GetR8(int r) => Reg8[r].ToString().Replace("_", "(HL)");

        public static string DisassembleInstruction(GbMemory memory) => DisassembleInstructionInternal(memory);

        private static string DisassembleInstructionInternal(GbMemory memory)
        {
            byte b = memory.GetMappedMemory(memory.R.Pc);
            if (b == 0xCB)
            {
                return DisassembleInstructionCb(memory.GetMappedMemory(memory.R.Pc + 1));
            }

            if (b == 0x76)
            {
                return "HALT";
            }

            if (b >= 0x40 && b < 0x80)
            {
                int dest = ((b >> 3) & 7);
                int src = (b & 7);
                return "LD " + GetR8(dest) + "," + GetR8(src);
            }

            if (b >= 80 && b < 0xC0)
            {
                return DisassembleInstructionArith(b);
            }

            return nmOpStrings[b];
        }

        private static string DisassembleInstructionArith(byte opcode)
        {
            int src = (opcode & 7);
            int dest = ((opcode >> 3) & 7);

            if (opcode < 0x80 || opcode > 0xBF)
            {
                throw new InvalidOperationException();
            }

            return ARITH.Substring(dest * 3, 3).TrimEnd(' ') + " A," + GetR8(src);
        }

        private static string DisassembleInstructionCb(byte cbCode)
        {
            int dest = ((cbCode >> 3) & 7);
            int src = (cbCode & 7);

            if (cbCode < 0x40)
            {
                return CBOPS.Substring(dest * 4, 4).TrimEnd(' ') + GetR8(dest) + "," + GetR8(src);
            }

            if (cbCode < 0x80)
            {
                return "BIT " + dest.ToString() + "," + GetR8(src);
            }

            if (cbCode < 0xA0)
            {
                return "RES " + dest.ToString() + "," + GetR8(src);
            }

            return "SET " + dest.ToString() + "," + GetR8(src);
        }
    }
}
