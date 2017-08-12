using System;
using System.Text;
using JAGBE.GB.DataTypes;
using JAGBE.Attributes;

namespace JAGBE.GB.Computation
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// * General Memory Map <br/>
    /// * 0000-3FFF 16KB ROM Bank 00 (in cartridge, private at bank 00) <br/>
    /// * 4000-7FFF 16KB ROM Bank 01..NN(in cartridge, switchable bank number) <br/>
    /// * 8000-9FFF 8KB Video RAM(VRAM)(switchable bank 0-1 in CGB Mode) <br/>
    /// * A000-BFFF 8KB External RAM(in cartridge, switchable bank, if any) <br/>
    /// * C000-CFFF 4KB Work RAM Bank 0(WRAM) <br/>
    /// * D000-DFFF 4KB Work RAM Bank 1(WRAM)(switchable bank 1-7 in CGB Mode) <br/>
    /// * E000-FDFF Same as 0xC000-DDFF(ECHO)(typically not used) <br/>
    /// * FE00-FE9F Sprite Attribute Table(OAM) <br/>
    /// * FEA0-FEFF Not Usable <br/>
    /// * FF00-FF7F IO Ports <br/>
    /// * FF80-FFFE High RAM(HRAM) <br/>
    /// * FFFF Interrupt Enable Register <br/>
    /// </remarks>
    internal sealed class GbMemory
    {
        /// <summary>
        /// The ROM
        /// </summary>
        public byte[] Rom;

        /// <summary>
        /// The boot rom
        /// </summary>
        internal readonly byte[] BootRom = new byte[0x100];

        /// <summary>
        /// The High Ram (stack)
        /// </summary>
        internal readonly byte[] HRam = new byte[MemoryRange.HRAMSIZE];

        /// <summary>
        /// The Interupt Master Enable register
        /// </summary>
        internal bool IME;

        private GbUInt16 Div = 0;

        internal LcdMemory lcdMemory = new LcdMemory();

        internal bool NextIMEValue;

        private bool PrevTimerIn;
        private bool ScheduleTimaInterupt;

        internal void UpdateTimer(GbUInt16 value)
        {
            if (this.ScheduleTimaInterupt)
            {
                this.IF |= 4;
                this.TimaV = this.TimaM;
            }

            this.Div += value;
            bool divBit = (this.Tac.GetBit(0) && this.Div.HighByte.GetBit(1)) || this.Div.LowByte.GetBit((byte)(((this.Tac & 3) * 2) + 3));
            bool b = this.Tac.GetBit(1) && divBit;
            if (this.PrevTimerIn && !b)
            {
                bool bt = this.ScheduleTimaInterupt;
                this.ScheduleTimaInterupt = this.TimaV == 0xFF;
                if (!bt)
                {
                    this.TimaV++;
                }
            }

            this.PrevTimerIn = b;
        }

        /// <summary>
        /// The Object Atribute Memory.
        /// </summary>
        internal readonly byte[] Oam = new byte[MemoryRange.OAMSIZE];

        internal CpuState Status;

        /// <summary>
        /// The Video Ram.
        /// </summary>
        internal readonly byte[] VRam = new byte[MemoryRange.VRAMBANKSIZE * 2];

        /// <summary>
        /// The Work Ram.
        /// </summary>
        internal readonly byte[] WRam = new byte[MemoryRange.WRAMBANKSIZE * 8];

        private bool bootMode = true;

        /// <summary>
        /// The External Ram.
        /// </summary>
        private readonly byte[] ERam = new byte[MemoryRange.ERAMBANKSIZE];

        /// <summary>
        /// Should ERam be used?
        /// </summary>
        private readonly bool ERamEnabled;

        /// <summary>
        /// The Interupt Enable Register
        /// </summary>
        private byte IER;

        /// <summary>
        /// The Interupt Flags register
        /// </summary>
        private byte IF;

        /// <summary>
        /// The mapped ram bank
        /// </summary>
        private int MappedRamBank;

        /// <summary>
        /// The mapped rom bank
        /// </summary>
        private int MappedRomBank = 1;

        /// <summary>
        /// The MBC mode
        /// </summary>
        private MemoryBankController MBCMode;

        private byte Tac;
        private byte TimaM;
        private byte TimaV;

        /// <summary>
        /// Gets the instance's registers.
        /// </summary>
        /// <value>The instance's registers.</value>
        internal GbRegisters R { get; } = new GbRegisters();

        public byte GetMappedMemoryHl() => GetMappedMemory(this.R.Hl);

        /// <summary>
        /// Dumps the currently mapped ram.
        /// </summary>
        /// <returns>The currently mapped ram</returns>
        internal byte[] DumpRam()
        {
            byte[] b = new byte[0x10000];
            for (int i = 0; i < 0x10000; i++)
            {
                b[i] = GetMappedMemory((ushort)i);
            }

            return b;
        }

        /// <summary>
        /// Dumps the registers.
        /// </summary>
        /// <returns>The registers as a string.</returns>
        internal string DumpRegisters()
        {
            StringBuilder builder = new StringBuilder(37);
            builder.AppendLine("Dumping Registers");
            builder.AppendLine(this.R.Af.Value.ToString("X4"));
            builder.AppendLine(this.R.Bc.Value.ToString("X4"));
            builder.AppendLine(this.R.De.Value.ToString("X4"));
            builder.AppendLine(this.R.Hl.Value.ToString("X4"));
            builder.Append(this.R.Sp.Value.ToString("X4"));
            return builder.ToString();
        }

        /// <summary>
        /// Gets the memory at the address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        internal byte GetMappedMemory(ushort address)
        {
            if (address < 0x4000) // 0x0000-3FFF
            {
                return GetRomMemory(0, address);
            }

            if (address < 0x8000) // 0x4000-7FFF
            {
                return GetRomMemory(this.MappedRomBank, (ushort)(address - 0x4000));
            }

            if (address < 0xA000) // 0x8000-9FFF
            {
                return this.VRam[address - 0x8000];
            }

            if (address < 0xC000) // 0xA000-BFFF
            {
                return GetERamMemory(this.MappedRamBank, (ushort)(address - 0xA000));
            }

            if (address < 0xE000) // 0xC000-DFFF
            {
                return this.WRam[address - 0xC000];
            }

            if (address < 0xFE00) // 0xE000-FDFF
            {
                if (address < 0xF000)
                {
                    return GetERamMemory(this.MappedRamBank, (ushort)(address - 0xE000));
                }

                return this.WRam[address - 0xF000];
            }

            if (address < 0xFEA0) // 0xFE00-FE9F
            {
                return this.Oam[address - 0xFE00];
            }

            if (address < 0xFF00) // 0xFEA0-FEFF
            {
                return 0x00;
            }

            if (address < 0xFF80) // 0xFF00-FF7F
            {
                return GetIoReg((byte)(address - 0xFF00));
            }

            if (address < 0xFFFF) // 0xFF80-FFFE
            {
                return this.HRam[address - 0xFF80];
            }

            return this.IER; // 0xFFFF
        }

        internal byte LdI8() => GetMappedMemory(this.R.Pc++);

        /// <summary>
        /// Pops a 8-bit value from the stack.
        /// </summary>
        /// <returns></returns>
        internal byte Pop() => GetMappedMemory(this.R.Sp++);

        /// <summary>
        /// Pushes the specified <paramref name="value"/> onto the stack.
        /// </summary>
        /// <param name="value">The value.</param>
        internal void Push(byte value) => SetMappedMemory(--this.R.Sp, value);

        /// <summary>
        /// Pushes the specified <paramref name="value"/> onto the stack.
        /// </summary>
        /// <param name="value">The value.</param>
        internal void Push(GbUInt16 value)
        {
            this.Push(value.LowByte);
            this.Push(value.HighByte);
        }

        internal void SetMappedMemory(ushort pointer, byte value)
        {
            if (this.MBCMode == MemoryBankController.None)
            {
                SetMappedMemoryNoMbc(pointer, value);
            }
            else
            {
                throw new InvalidOperationException("MBC mode is invalid or unimplemented");
            }
        }

        internal void SetMappedMemoryHl(byte value) => SetMappedMemory(this.R.Hl, value);

        private static bool IsUnusedIoRegister(byte number)
        {
            if ((number != 0 && number < 0x4) || number == 0x15 || number == 0x1F)
            {
                return true;
            }

            return (number > 0x7 && number < 0x0F) || number >= 0x50;
        }

        private static void UnimplementedRead(string identifer) => Console.WriteLine("Attempt to read " + identifer + " —Unimplemented");

        private static void UnimplementedWrite(string identifier) => Console.WriteLine("Write to " + identifier + " unimplemented");

        /// <summary>
        /// Gets data from ERAM.
        /// </summary>
        /// <param name="bank">The bank of ERAM memory.</param>
        /// <param name="address">The address.</param>
        /// <returns>ERam</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="MBCMode"/> is invalid
        /// </exception>
        private byte GetERamMemory(int bank, ushort address)
        {
            if (!this.ERamEnabled)
            {
                return 0xFF;
            }

            if (this.MBCMode == MemoryBankController.None)
            {
                return this.ERam[address + (bank * MemoryRange.ERAMBANKSIZE)];
            }

            throw new InvalidOperationException("Unsuported or unimplemented " + nameof(MemoryBankController));
        }

        /// <summary>
        /// Gets the value of the <paramref name="number"/>'th IO Register.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns>The value of the <paramref name="number"/>'th IO Register.</returns>
        private byte GetIoReg(byte number)
        {
            if (IsUnusedIoRegister(number))
            {
                return 0xFF;
            }

            if ((number & 0xF0) == 0x40)
            {
                return this.lcdMemory.GetRegister(number);
            }

            switch (number)
            {
                case 0x00:
                    return GetJoypad();

                case 0x04:
                    return this.Div.HighByte;

                case 0x05:
                    return this.TimaV;

                case 0x06:
                    return this.TimaM;

                case 0x07:
                    return (byte)(this.Tac | 0xFC);

                case 0x0F:
                    return (byte)(this.IF | 0xE0);

                default:
                    Console.WriteLine("Possible bad Read from IO 0x" + number.ToString("X2") + " (reg)");
                    return 0xFF;
            }
        }

        /// <summary>
        /// Gets A value representing the state of the Joypad.
        /// </summary>
        /// <returns></returns>
        [Stub] private byte GetJoypad() => 0xFF;

        /// <summary>
        /// Gets <paramref name="address"/> from <paramref name="bank"/> of ROM.
        /// </summary>
        /// <param name="bank">The ROM bank.</param>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        private byte GetRomMemory(int bank, ushort address)
        {
            if (this.bootMode && bank == 0 && address < 0x100)
            {
                return this.BootRom[address];
            }

            if (this.Rom == null || (address + (bank * MemoryRange.ROMBANKSIZE) >= this.Rom.Length))
            {
                return 0xFF;
            }

            return this.Rom[address + (bank * MemoryRange.ROMBANKSIZE)];
        }

        /// <summary>
        /// Gets the timer control.
        /// </summary>
        /// <returns></returns>
        [Stub]
        private byte GetTimerControl()
        {
            UnimplementedRead("Timer_Crl");
            return 0xFF;
        }

        /// <summary>
        /// Gets the timer counter.
        /// </summary>
        /// <returns></returns>
        [Stub]
        private byte GetTimerCounter()
        {
            UnimplementedRead("Timer_Ctr");
            return 0xFF;
        }

        /// <summary>
        /// Gets the timer modulo.
        /// </summary>
        /// <returns></returns>
        [Stub]
        private byte GetTimerModulo()
        {
            UnimplementedRead("Timer_Mod");
            return 0xFF;
        }

        [Stub]
        private void SetERam(int address, byte value) => UnimplementedWrite("External ram");

        /// <summary>
        /// Sets the io registers.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="value">The value.</param>
        private void SetIoRegisters(int pointer, byte value)
        {
            if ((pointer & 0xF0) == 0x40)
            {
                if (!this.lcdMemory.SetRegisters(pointer - 0x40, value))
                {
                    Console.WriteLine(
                        "Failed write (LCD Rg): " +
                        pointer.ToString("X2") +
                        " (ptr) " +
                        this.R.Pc.ToString("X4") +
                        " (pc) " +
                        value.ToString("X2") +
                        " (value)");
                }

                return;
            }

            switch (pointer)
            {
                case 0x01:
                    Console.WriteLine("Attempt to write to SB (0xFF01) ignored as it is unimplemented.");
                    return;

                case 0x02:
                    Console.WriteLine("Attempt to write to SC (0xFF02) ignored as it is unimplemented.");
                    return;

                case 0x04:
                    this.Div = 0;
                    return;

                case 0x05:
                    this.TimaV = value;
                    return;

                case 0x06:
                    this.TimaM = value;
                    return;

                case 0x07:
                    this.Tac = (byte)(value & 3);
                    return;

                case 0x0F:
                    this.IF = (byte)(value & 0x1F);
                    return;

                case 0x50:
                    this.bootMode = false;
                    return;

                default:
                    Console.WriteLine("Failed write (IO Reg): " +
                        pointer.ToString("X2") +
                        " (ptr) " + this.R.Pc.ToString("X4") +
                        " (pc) " +
                        value.ToString("X2") +
                        " (value)");
                    return;
            }
        }

        /// <summary>
        /// Sets the mapped memory If there is no MBC.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="value">The value.</param>
        private void SetMappedMemoryNoMbc(ushort pointer, byte value)
        {
            if (pointer < 0x8000)
            {
                // Put ram chip enabler here if needed.
                return;
            }

            if (pointer <= 0x9FFF)
            {
                this.VRam[pointer - 0x8000] = value;
            }
            else if (pointer <= 0xBFFF)
            {
                if (this.ERamEnabled)
                {
                    SetERam(pointer - 0xA000, value);
                }
            }
            else if (pointer <= 0xDFFF)
            {
                this.WRam[pointer - 0xC000] = value;
            }
            else if (pointer <= 0xFDFF)
            {
                this.WRam[pointer - 0xE000] = value;
            }
            else if (pointer <= 0xFE9F)
            {
                this.Oam[pointer - 0xFE00] = value;
            }
            else if (pointer <= 0xFEFF)
            {
                // Just return, this is ignored on DMG.
            }
            else if (pointer <= 0xFF7F)
            {
                SetIoRegisters(pointer - 0xFF00, value);
            }
            else if (pointer <= 0xFFFE)
            {
                this.HRam[pointer - 0xFF80] = value;
            }
            else // 0xFFFF
            {
                this.IER = value;
            }
        }
    }
}
