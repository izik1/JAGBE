using System;
using System.Text;
using JAGBE.GB.DataTypes;
using JAGBE.Attributes;
using JAGBE.GB.Input;

namespace JAGBE.GB.Computation
{
    /// <summary>
    /// A class for keeping track of the Memory state of the Game Boy.
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
        /// The External Ram.
        /// </summary>
        internal byte[] ERam;

        /// <summary>
        /// The High Ram (stack)
        /// </summary>
        internal readonly byte[] HRam = new byte[MemoryRange.HRAMSIZE];

        /// <summary>
        /// The Interupt Master Enable register
        /// </summary>
        internal bool IME;

        /// <summary>
        /// The LCD memory
        /// </summary>
        internal LcdMemory lcdMemory = new LcdMemory();

        /// <summary>
        /// The MBC mode
        /// </summary>
        internal MemoryBankController MBCMode;

        /// <summary>
        /// The next IME value
        /// </summary>
        internal bool NextIMEValue;

        /// <summary>
        /// The Object Atribute Memory.
        /// </summary>
        internal readonly byte[] Oam = new byte[MemoryRange.OAMSIZE];

        /// <summary>
        /// The status of the cpu
        /// </summary>
        internal CpuState Status;

        /// <summary>
        /// The Video Ram.
        /// </summary>
        internal readonly byte[] VRam = new byte[MemoryRange.VRAMBANKSIZE * 2];

        /// <summary>
        /// The Work Ram.
        /// </summary>
        internal readonly byte[] WRam = new byte[MemoryRange.WRAMBANKSIZE * 8];

        /// <summary>
        /// Should low (0h-100h) writes be redirected to the boot rom?
        /// </summary>
        private bool bootMode = true;

        /// <summary>
        /// The system timer.
        /// </summary>
        private GbUInt16 sysTimer = 0;

        /// <summary>
        /// Should ERam be used?
        /// </summary>
        private bool ERamEnabled;

        /// <summary>
        /// The Interupt Enable Register
        /// </summary>
        internal byte IER;

        /// <summary>
        /// The Interupt Flags register
        /// </summary>
        internal byte IF;

        /// <summary>
        /// The joypad
        /// </summary>
        private byte Joypad;

        /// <summary>
        /// The keys of the joypad
        /// </summary>
        private byte keys = 0xFF;

        /// <summary>
        /// The mapped ram bank
        /// </summary>
        private int MappedRamBank;

        /// <summary>
        /// The mapped rom bank
        /// </summary>
        private int MappedRomBank = 1;

        /// <summary>
        /// Is ram enabled in MBC?
        /// </summary>
        private bool MbcRamMode;

        /// <summary>
        /// The previous state of <see cref="keys"/>
        /// </summary>
        private byte prevKeys = 0xFF;

        /// <summary>
        /// The previous state of timer input
        /// </summary>
        private bool PrevTimerIn;

        /// <summary>
        /// Is a TIMA Interupt scheduled?
        /// </summary>
        private bool ScheduleTimaInterupt;

        /// <summary>
        /// The tac register
        /// </summary>
        private byte Tac;

        /// <summary>
        /// The TIMA Modulo Register
        /// </summary>
        private byte TimaM;

        /// <summary>
        /// The TIMA Value Register.
        /// </summary>
        private byte TimaV;

        /// <summary>
        /// Initializes a new instance of the <see cref="GbMemory"/> class with the given <paramref name="inputHandler"/>.
        /// </summary>
        /// <param name="inputHandler">The input handler.</param>
        internal GbMemory(IInputHandler inputHandler) // Null is valid
        {
            if (inputHandler != null)
            {
                inputHandler.OnInput += this.OnInput;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GbMemory"/> class.
        /// </summary>
        internal GbMemory() : this(null)
        {
        }

        /// <summary>
        /// Gets the system timer.
        /// </summary>
        /// <value>The system timer.</value>
        public GbUInt16 SysTimer => this.sysTimer;

        /// <summary>
        /// Gets the instance's registers.
        /// </summary>
        /// <value>The instance's registers.</value>
        internal GbRegisters R { get; } = new GbRegisters();

        /// <summary>
        /// Gets value of memory at HL.
        /// </summary>
        /// <returns></returns>
        public byte GetMappedMemoryHl() => GetMappedMemory(this.R.Hl, false);

        /// <summary>
        /// Gets active rom bank.
        /// </summary>
        /// <returns></returns>
        public int GetRomBank() => (byte)(this.MappedRomBank | (!this.MbcRamMode ? this.MappedRamBank << 5 : 0));

        /// <summary>
        /// Updates the key state.
        /// </summary>
        public void UpdateKeys()
        {
            if (((GetJoypad(this.prevKeys) & 0xF) == 0xF) && (GetJoypad(this.keys) & 0xF) != 0xF)
            {
                this.IF |= 0x10;
            }
        }

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
        /// Gets the memory at <paramref name="address"/>.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        internal byte GetMappedMemory(ushort address) => GetMappedMemory(address, false);

        /// <summary>
        /// Gets the memory at <paramref name="address"/> - for use in DMAs.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        internal byte GetMappedMemoryDma(ushort address) => GetMappedMemory(address, true);

        /// <summary>
        /// Loads a 8-bit value and increments the pc.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Sets the memory at <paramref name="pointer"/> to <paramref name="value"/>.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="InvalidOperationException">MBC mode is invalid or unimplemented</exception>
        internal void SetMappedMemory(ushort pointer, byte value)
        {
            if (pointer < 0x8000)
            {
                if (this.MBCMode == MemoryBankController.None)
                {
                    // Add Ram enabler chip here.
                }
                else if (this.MBCMode == MemoryBankController.MBC1)
                {
                    if (pointer < 0x2000)
                    {
                        this.ERamEnabled = (value & 0xF) == 0xA;
                    }
                    else if (pointer < 0x4000)
                    {
                        this.MappedRomBank = (byte)((this.MappedRomBank & 0xE0) +
                            (value & 0x10) + (byte)((value & 0xF) + ((value & 0xF) == 0 ? 1 : 0)));
                    }
                    else if (pointer < 0x6000)
                    {
                        this.MappedRamBank = (byte)(value & 3);
                    }
                    else
                    {
                        this.MbcRamMode = ((value & 1) == 1);
                    }
                }
                else
                {
                    throw new InvalidOperationException("MBC mode is invalid or unimplemented");
                }
            }
            else
            {
                SetMappedMemoryCommon(pointer, value);
            }
        }

        /// <summary>
        /// Sets the memory at HL to value.
        /// </summary>
        /// <param name="value">The value.</param>
        internal void SetMappedMemoryHl(byte value) => SetMappedMemory(this.R.Hl, value);

        /// <summary>
        /// Updates the timer.
        /// </summary>
        /// <param name="cycles">The value.</param>
        internal void UpdateTimer(GbUInt16 cycles)
        {
            if (this.ScheduleTimaInterupt)
            {
                this.IF |= 4;
                this.TimaV = this.TimaM;
            }

            this.sysTimer += cycles;
            bool divBit = (this.Tac.GetBit(0) && this.sysTimer.HighByte.GetBit(1)) || this.sysTimer.LowByte.GetBit((byte)(((this.Tac & 3) * 2) + 3));
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
        /// Determines whether <paramref name="number"/> is an unused register number.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="number"/> is an unused register number;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        private static bool IsUnusedIoRegister(byte number)
        {
            if ((number != 0 && number < 0x4) || number == 0x15 || number == 0x1F)
            {
                return true;
            }

            return (number > 0x7 && number < 0x0F) || number >= 0x50;
        }

        /// <summary>
        /// Gets data from ERAM.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>ERam</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="MBCMode"/> is invalid
        /// </exception>
        private byte GetERamMemory(ushort address)
        {
            if (!this.ERamEnabled)
            {
                return 0xFF;
            }

            if (this.MBCMode == MemoryBankController.None)
            {
                return this.ERam[address + (this.MbcRamMode ? this.MappedRamBank * MemoryRange.ERAMBANKSIZE : 0)];
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
                    return GetJoypad(this.keys);

                case 0x04:
                    return this.sysTimer.HighByte;

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
        /// Gets value of the joypad.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <returns></returns>
        private byte GetJoypad(byte p1) =>
            (byte)((!this.Joypad.GetBit(5) ? (p1 & 0xF) : !this.Joypad.GetBit(4) ? ((p1 >> 4) & 0xF) : 0xFF) | 0xC0);

        /// <summary>
        /// Gets the mapped memory.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="ignoreDmaBlock">if set to <see langword="true"/> ignore dma write restriction.</param>
        /// <returns></returns>
        private byte GetMappedMemory(ushort address, bool ignoreDmaBlock)
        {
            if (!ignoreDmaBlock && this.lcdMemory.DMA < Cpu.DelayStep * 162)
            {
                if (address >= 0xFF80 && address < 0xFFFF) // 0xFF80-FFFE
                {
                    return this.HRam[address - 0xFF80];
                }

                return 0xFF;
            }

            if (address < 0x4000) // 0x0000-3FFF
            {
                return GetRomMemory(0, address);
            }

            if (address < 0x8000) // 0x4000-7FFF
            {
                return GetRomMemory(GetRomBank(), (ushort)(address - 0x4000));
            }

            if (address < 0xA000) // 0x8000-9FFF
            {
                return (this.lcdMemory.STAT & 0x3) == 3 ? (byte)0xFF : this.VRam[address - 0x8000];
            }

            if (address < 0xC000) // 0xA000-BFFF
            {
                return GetERamMemory((ushort)(address - 0xA000));
            }

            if (address < 0xE000) // 0xC000-DFFF
            {
                return this.WRam[address - 0xC000];
            }

            if (address < 0xFE00) // 0xE000-FDFF
            {
                return address < 0xF000 ? GetERamMemory((ushort)(address - 0xE000)) : this.WRam[address - 0xF000];
            }

            if (address < 0xFEA0) // 0xFE00-FE9F
            {
                return (this.lcdMemory.STAT & 0x2) == 2 ? (byte)0xFF : this.Oam[address - 0xFE00];
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

        /// <summary>
        /// Gets <paramref name="address"/> from ROM.
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
        /// Called when input is recieved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="InputEventArgs"/> instance containing the event data.</param>
        private void OnInput(object sender, InputEventArgs e)
        {
            this.prevKeys = this.keys;
            this.keys = e.value;
        }

        /// <summary>
        /// Sets <see cref="ERam"/> at <paramref name="address"/> to <paramref name="value"/>.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="value">The value.</param>
        private void SetERam(int address, byte value)
        {
            if (this.ERamEnabled)
            {
                this.ERam[address + (this.MbcRamMode ? this.MappedRamBank * MemoryRange.ERAMBANKSIZE : 0)] = value;
            }
        }

        /// <summary>
        /// Sets the io registers.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="value">The value.</param>
        private void SetIoRegisters(int pointer, byte value)
        {
            if ((pointer & 0xF0) == 0x40)
            {
                if (!this.lcdMemory.SetRegister(pointer - 0x40, value))
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
                case 0x00:
                    this.Joypad = (byte)(value & 0x30);
                    return;

                case 0x01:
                    Console.WriteLine("Attempt to write to SB (0xFF01) ignored as it is unimplemented.");
                    return;

                case 0x02:
                    Console.WriteLine("Attempt to write to SC (0xFF02) ignored as it is unimplemented.");
                    return;

                case 0x04:
                    this.sysTimer = 0;
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
        /// Sets the memory at <paramref name="pointer"/> to <paramref name="value"/>.
        /// </summary>
        /// <remarks>Common to all MBC modes</remarks>
        /// <param name="pointer">The pointer.</param>
        /// <param name="value">The value.</param>
        private void SetMappedMemoryCommon(ushort pointer, byte value)
        {
            if (pointer <= 0x9FFF)
            {
                this.VRam[pointer - 0x8000] = value;
            }
            else if (pointer <= 0xBFFF)
            {
                SetERam(pointer - 0xA000, value);
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
