using System;
using JAGBE.GB.Input;
using JAGBE.Logging;

namespace JAGBE.GB.Emulation
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

        internal bool HaltBugged;

        /// <summary>
        /// The High Ram (stack)
        /// </summary>
        internal readonly byte[] HRam = new byte[MemoryRange.HRAMSIZE];

        /// <summary>
        /// The Interupt Enable Register
        /// </summary>
        internal byte IER;

        /// <summary>
        /// The Interupt Flags register
        /// </summary>
        internal byte IF;

        /// <summary>
        /// The Interupt Master Enable register
        /// </summary>
        internal bool IME;

        internal readonly Joypad joypad;

        /// <summary>
        /// The LCD memory
        /// </summary>
        internal Lcd lcd;

        /// <summary>
        /// The MBC mode
        /// </summary>
        internal MemoryBankController MBCMode;

        /// <summary>
        /// The next IME value
        /// </summary>
        internal bool NextIMEValue;

        /// <summary>
        /// Gets the instance's registers.
        /// </summary>
        /// <value>The instance's registers.</value>
        internal GbRegisters R = new GbRegisters();

        internal int RomBanks;

        /// <summary>
        /// The status of the cpu
        /// </summary>
        internal CpuState Status;

        /// <summary>
        /// The Work Ram.
        /// </summary>
        internal readonly byte[] WRam = new byte[MemoryRange.WRAMBANKSIZE * 8];

        private readonly Apu apu = new Apu();

        /// <summary>
        /// Should low (0h-100h) writes be redirected to the boot rom?
        /// </summary>
        private bool bootMode = true;

        /// <summary>
        /// Should ERam be used?
        /// </summary>
        private bool ERamEnabled;

        /// <summary>
        /// The mapped ram bank
        /// </summary>
        private int MappedRamBank;

        /// <summary>
        /// The mapped rom bank
        /// </summary>
        private int MappedRomBank = 1;

        private int Mbc1ModeFlag;

        /// <summary>
        /// The timer
        /// </summary>
        private readonly Timer timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GbMemory"/> class with the given <paramref name="inputHandler"/>.
        /// </summary>
        /// <param name="inputHandler">The input handler.</param>
        internal GbMemory(IInputHandler inputHandler)
        {
            this.lcd = new Lcd(this);
            this.timer = new Timer(this);
            this.joypad = new Joypad(inputHandler);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GbMemory"/> class.
        /// </summary>
        internal GbMemory() : this(null)
        {
        }

        /// <summary>
        /// Gets value of memory at HL.
        /// </summary>
        /// <returns></returns>
        public byte GetMappedMemoryHl() => GetMappedMemory(this.R.Hl, false);

        /// <summary>
        /// Gets active rom bank.
        /// </summary>
        /// <returns></returns>
        public int GetRomBank() => (this.RomBanks - 1) & (this.MappedRomBank | (this.MappedRamBank << 5));

        public void Update(int TCycles)
        {
            for (int i = 0; i < TCycles; i++)
            {
                this.lcd.Tick();
                this.timer.Update();
            }
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update() => Update(Cpu.MCycle);

        /// <summary>
        /// Gets the memory at <paramref name="address"/>.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        internal byte GetMappedMemory(GbUInt16 address) => GetMappedMemory(address, false);

        /// <summary>
        /// Gets the memory at <paramref name="address"/> - for use in DMAs.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        internal byte GetMappedMemoryDma(GbUInt16 address) => GetMappedMemory(address, true);

        /// <summary>
        /// Loads a 8-bit value and increments the pc.
        /// </summary>
        /// <returns></returns>
        internal byte LdI8() => GetMappedMemory(this.R.Pc++, false);

        internal byte ReadCycle(GbUInt16 address)
        {
            this.Update(4); // should be 3.
            byte val = GetMappedMemory(address, false);
            this.Update(0); // should be 1.
            return val;
        }

        internal byte ReadCycleI8() => ReadCycle(this.R.Pc++);

        internal byte ReadCycleHl() => ReadCycle(this.R.Hl);

        internal byte ReadCyclePop() => this.ReadCycle(this.R.Sp++);

        /// <summary>
        /// Pushes the specified <paramref name="value"/> onto the stack.
        /// </summary>
        /// <param name="value">The value.</param>
        internal void Push(byte value) => SetMappedMemory(--this.R.Sp, value);

        /// <summary>
        /// Sets the memory at <paramref name="pointer"/> to <paramref name="value"/>.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="InvalidOperationException">MBC mode is invalid or unimplemented</exception>
        internal void SetMappedMemory(GbUInt16 pointer, byte value)
        {
            // Handle bus conflicts.
            if (this.lcd.DmaMode && ((pointer >= 0xFE00 && pointer < 0xFEA0) || HasBusConflict(pointer, this.lcd.DmaAddress)))
            {
                return;
            }

            if (pointer < 0x8000)
            {
                if (this.MBCMode == MemoryBankController.None)
                {
                    // Add Ram enabler chip here.
                }
                else if (this.MBCMode == MemoryBankController.MBC1)
                {
                    SetMappedMemoryMbc1(pointer, value);
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

        private static bool HasBusConflict(GbUInt16 a1, GbUInt16 a2) =>
            (UsesMainBus(a1) && UsesMainBus(a2)) || (UsesVRam(a1) && UsesVRam(a2));

        private static bool UsesMainBus(GbUInt16 address) => address < 0x8000 || (address >= 0xA000 && address < 0xFE00);

        private static bool UsesVRam(GbUInt16 address) => address >= 0x8000 && address < 0xA000;

        /// <summary>
        /// Gets data from ERAM.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>ERam</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="MBCMode"/> is invalid
        /// </exception>
        private byte GetERamMemory(GbUInt16 address)
        {
            if (!this.ERamEnabled)
            {
                return 0xFF;
            }

            return this.ERam[GetERamAddress(address)];
        }

        /// <summary>
        /// Gets the value of the <paramref name="number"/>'th IO Register.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns>The value of the <paramref name="number"/>'th IO Register.</returns>
        private byte GetIoReg(byte number)
        {
            if (number == 0x00)
            {
                return this.joypad.Pad;
            }

            if (number < 3)
            {
                return 0xFF; // TODO: implement proper serial read/writes.
            }

            if (number < 0xF)
            {
                return this.timer[number];
            }

            if (number == 0xF)
            {
                return (byte)(this.IF | 0xE0);
            }

            if (number <= 0x3F)
            {
                return this.apu[number];
            }

            if (number < 0x50)
            {
                return this.lcd[number];
            }

            return 0xFF;
        }

        /// <summary>
        /// Gets the mapped memory.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="ignoreDmaBlock">if set to <see langword="true"/> ignore dma write restriction.</param>
        /// <returns>the value at <paramref name="address"/></returns>
        private byte GetMappedMemory(GbUInt16 address, bool ignoreDmaBlock)
        {
            GbUInt16 readAddress = address;
            if (!ignoreDmaBlock && this.lcd.DmaMode)
            {
                if (readAddress >= 0xFE00 && readAddress < 0xFEA0)
                {
                    return 0xFF;
                }

                if (HasBusConflict(readAddress, this.lcd.DmaAddress)) // Handle bus conflicts.
                {
                    readAddress = this.lcd.DmaAddress;
                }
            }

            if (readAddress < 0x4000) // 0x0000-3FFF
            {
                return GetRomMemory(this.Mbc1ModeFlag * ((this.RomBanks - 1) & (this.MappedRamBank << 5)), readAddress);
            }

            if (readAddress < 0x8000) // 0x4000-7FFF
            {
                return GetRomMemory(GetRomBank(), readAddress - 0x4000);
            }

            if (readAddress < 0xA000) // 0x8000-9FFF
            {
                return this.lcd.VRamBlocked ? byte.MaxValue : this.lcd.VRam[readAddress - 0x8000];
            }

            if (readAddress < 0xC000) // 0xA000-BFFF
            {
                return GetERamMemory(readAddress - 0xA000);
            }

            if (readAddress < 0xE000) // 0xC000-DFFF
            {
                return this.WRam[readAddress - 0xC000];
            }

            if (readAddress < 0xFE00) // 0xE000-FDFF
            {
                return this.WRam[readAddress - 0xE000];
            }

            if (readAddress < 0xFEA0) // 0xFE00-FE9F
            {
                return this.lcd.OamBlocked ? byte.MaxValue : this.lcd.Oam[readAddress - 0xFE00];
            }

            if (readAddress < 0xFF00) // 0xFEA0-FEFF
            {
                return 0;
            }

            if (readAddress < 0xFF80) // 0xFF00-FF7F
            {
                return GetIoReg((byte)(readAddress - 0xFF00));
            }

            if (readAddress < 0xFFFF) // 0xFF80-FFFE
            {
                return this.HRam[readAddress - 0xFF80];
            }

            return this.IER; // 0xFFFF
        }

        /// <summary>
        /// Gets The value of the rom at <paramref name="bank"/> and <paramref name="address"/>.
        /// </summary>
        /// <param name="bank">The ROM bank.</param>
        /// <param name="address">The address.</param>
        /// <returns>The value of the rom at <paramref name="bank"/> and <paramref name="address"/>.</returns>
        private byte GetRomMemory(int bank, GbUInt16 address)
        {
            if (this.bootMode && bank == 0 && address < 0x100)
            {
                return this.BootRom[address];
            }

            if (this.Rom == null || (address + (bank * MemoryRange.ROMBANKSIZE) >= this.Rom.Length))
            {
                Logger.LogWarning("Out of range read, bank: " + bank.ToString());
                return 0xFF;
            }

            return this.Rom[address + (bank * MemoryRange.ROMBANKSIZE)];
        }

        /// <summary>
        /// Sets <see cref="ERam"/> at <paramref name="address"/> to <paramref name="value"/>.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="value">The value.</param>
        private void SetERam(GbUInt16 address, byte value)
        {
            if (this.ERamEnabled && this.ERam.Length > 0)
            {
                this.ERam[GetERamAddress(address)] = value;
            }
        }

        private int GetERamAddress(GbUInt16 address) =>
            (address + (this.Mbc1ModeFlag * this.MappedRamBank * MemoryRange.ERAMBANKSIZE)) % this.ERam.Length;

        /// <summary>
        /// Sets the io registers.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="value">The value.</param>
        private void SetIoRegisters(byte pointer, byte value)
        {
            if (pointer == 0)
            {
                this.joypad.Status = (byte)(value & 0x30);
                return;
            }

            if (pointer < 3)
            {
                return; // TODO: implement proper serial read/writes.
            }

            if (pointer < 0xF)
            {
                this.timer[pointer] = value;
                return;
            }

            if (pointer == 0xF)
            {
                this.IF = (byte)(value & 0x1F);
                return;
            }

            if (pointer <= 0x3F)
            {
                this.apu[pointer] = value;
                return;
            }

            if (pointer <= 0x4F)
            {
                this.lcd[(byte)(pointer - 0x40)] = value;
                return;
            }

            this.bootMode &= (pointer != 0x50);
        }

        /// <summary>
        /// Sets the memory at <paramref name="pointer"/> to <paramref name="value"/>.
        /// </summary>
        /// <remarks>Common to all MBC modes</remarks>
        /// <param name="pointer">The pointer.</param>
        /// <param name="value">The value.</param>
        private void SetMappedMemoryCommon(GbUInt16 pointer, byte value)
        {
            if (pointer <= 0x9FFF)
            {
                if (!this.lcd.VRamBlocked)
                {
                    this.lcd.VRam[pointer - 0x8000] = value;
                }
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
                if (!this.lcd.OamBlocked)
                {
                    this.lcd.Oam[pointer - 0xFE00] = value;
                }
            }
            else if (pointer <= 0xFEFF)
            {
                SetMappedMemoryCommon(pointer - 0x2000, value);
            }
            else if (pointer <= 0xFF7F)
            {
                SetIoRegisters((byte)(pointer - 0xFF00), value);
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

        /// <summary>
        /// Sets the mapped memory using MBC1 specific code.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="value">The value.</param>
        private void SetMappedMemoryMbc1(GbUInt16 pointer, byte value)
        {
            if (pointer < 0x2000)
            {
                this.ERamEnabled = (value & 0xF) == 0xA;
            }
            else if (pointer < 0x4000)
            {
                this.MappedRomBank = ((value & 0x1F) == 0 ? 1 : value & 0x1F);
            }
            else if (pointer < 0x6000)
            {
                this.MappedRamBank = value & 3;
            }
            else
            {
                this.Mbc1ModeFlag = value & 1;
            }
        }
    }
}
