using System;
using System.Collections.Generic;
using JAGBE.GB.Assembly;
using JAGBE.GB.Input;
using JAGBE.Logging;

namespace JAGBE.GB.Emulation
{
    /// <summary>
    /// This class manages the GameBoy's cpu.
    /// </summary>
    internal sealed class Cpu
    {
        /// <summary>
        /// The clock speed in hz
        /// </summary>
        /// <value>4194304</value>
        internal const int ClockSpeedHz = 4194304;

        /// <summary>
        /// The multiplier for the number of clocks an instruction takes.
        /// </summary>
        internal const int DelayStep = 4;

        /// <summary>
        /// Determines weather the cpu is in break mode or not.
        /// </summary>
        private bool breakMode;

        /// <summary>
        /// The list of addresses this instance will enter <see cref="breakMode"/> at.
        /// </summary>
        private readonly HashSet<ushort> breakPoints = new HashSet<ushort>();

        /// <summary>
        /// The delay until the next cycle.
        /// </summary>
        private int delay;

        /// <summary>
        /// Gets the status of this instance.
        /// </summary>
        /// <value>The status.</value>
        public CpuState Status => this.memory.Status;

        private bool hung;

        /// <summary>
        /// The memory of this cpu
        /// </summary>
        private GbMemory memory;

        /// <summary>
        /// Gets the LCD's display memory a <see cref="byte"/>[].
        /// </summary>
        /// <returns>The LCD's display memory as a <see cref="byte"/>[].</returns>
        public byte[] DisplayMemoryAsBytes() => this.memory.Lcd.DisplayToBytes();

        /// <summary>
        /// Initializes a new instance of the <see cref="Cpu"/> class.
        /// </summary>
        /// <param name="bootRom">The boot rom.</param>
        /// <param name="rom">The rom.</param>
        /// <param name="inputHandler">The input handler.</param>
        public Cpu(byte[] bootRom, byte[] rom, IInputHandler inputHandler) => Reset(rom, bootRom, inputHandler);

        /// <summary>
        /// Initializes a new instance of the <see cref="Cpu"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor should be used when the ability to freely check the memory is needed, IE tests.
        /// </remarks>
        /// <param name="memory">The memory.</param>
        public Cpu(GbMemory memory) => this.memory = memory;

        /// <summary>
        /// Gets the display memory.
        /// </summary>
        /// <value>The display memory.</value>
        public int[] DisplayMemory => this.memory.Lcd.displayMemory;

        /// <summary>
        /// Gets a value indicating whether the cpu is in breakmode or not.
        /// </summary>
        /// <value><see langword="true"/> if the cpu is in breakmode; otherwise, <see langword="false"/>.</value>
        public bool BreakMode => this.breakMode;

        /// <summary>
        /// Gets the pc.
        /// </summary>
        /// <value>The pc.</value>
        internal GbUInt16 Pc => this.memory.R.Pc;

        /// <summary>
        /// This re-dumps the memory each time it's called.
        /// </summary>
        /// <value>The ram dump.</value>
        internal byte[] RamDump => this.memory.DumpRam();

        /// <summary>
        /// Adds a break point at <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The address.</param>
        public void AddBreakPoint(ushort address) => this.breakPoints.Add(address);

        /// <summary>
        /// Removes the break point at <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The address.</param>
        public void RemoveBreakPoint(ushort address) => this.breakPoints.Remove(address);

        /// <summary>
        /// Resets the memory of this instance.
        /// </summary>
        /// <param name="rom">The rom.</param>
        /// <param name="bootRom">The boot rom.</param>
        /// <param name="inputHandler">The input handler.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Reset(byte[] rom, byte[] bootRom, IInputHandler inputHandler)
        {
            int ramSize;
            switch (rom[0x149])
            {
                case 1:
                    ramSize = 0x800; // 2KB
                    break;

                case 2:
                    ramSize = MemoryRange.ERAMBANKSIZE;
                    break;

                case 3:
                    ramSize = MemoryRange.ERAMBANKSIZE * 4;
                    break;

                case 4:
                    ramSize = MemoryRange.ERAMBANKSIZE * 16;
                    break;

                case 5:
                    ramSize = MemoryRange.ERAMBANKSIZE * 8;
                    break;

                default:
                    ramSize = 0;
                    break;
            }

            byte mbcMode = rom[0x147];
            if (mbcMode > 3)
            {
                throw new InvalidOperationException("Unimplemented MBC mode " + mbcMode.ToString("X2"));
            }

            this.memory = new GbMemory(inputHandler)
            {
                Rom = new byte[(MemoryRange.ROMBANKSIZE * 2) << rom[0x148]], // set the rom size to what the cartrage says.
                ERam = new GbUInt8[ramSize],
                MBCMode = mbcMode == 0 ? MemoryBankController.None : MemoryBankController.MBC1
            }; // Override the memory.

            Array.Copy(bootRom, this.memory.BootRom, 256);

            Buffer.BlockCopy(rom, 0, this.memory.Rom, 0, rom.Length); // Buffer copy because I guess it might be faster?
            for (int i = rom.Length; i < this.memory.Rom.Length; i++)
            {
                this.memory.Rom[i] = 0xFF;
            }
        }

        /// <summary>
        /// Runs <paramref name="cycles"/> number of clock ticks
        /// </summary>
        /// <param name="cycles">The number of clock ticks (NOT INSTRUCTIONS) to run</param>
        public void Tick(int cycles)
        {
            this.delay -= cycles;

            // This is to sync the cpu (with variable clock length instructions) to the GPU and APU
            // which always take the same amount of time. In the previous implementation (gpu/apu
            // runs every cpu tick) they would fall behind whenever the cpu runs an instruction that
            // takes > DelayStep cycles.
            int syncDelay = this.delay;
            while (this.delay < 0)
            {
                if (this.memory.Status == CpuState.STOP)
                {
                    this.memory.UpdateKeys();
                    this.delay += DelayStep;
                    syncDelay += DelayStep;
                    if ((this.memory.IF & this.memory.IER & 0x1F) > 0)
                    {
                        this.memory.Status = CpuState.OKAY;
                    }

                    continue;
                }

                if (this.memory.Status == CpuState.HUNG)
                {
                    if (!this.hung)
                    {
                        Logger.LogWarning("Cpu has hung.");
                        this.hung = true;
                    }

                    this.delay += DelayStep;
                    continue;
                }

                TickIoDevices(ref syncDelay);
                if (this.memory.Status == CpuState.HALT)
                {
                    this.delay += DelayStep;
                    if ((this.memory.IF & this.memory.IER & 0x1F) > 0)
                    {
                        this.memory.Status = CpuState.OKAY;
                    }

                    continue;
                }

                if (this.memory.IME)
                {
                    int step = 0;
                    while (!HandleInterupts(step))
                    {
                        step++;
                        this.delay += DelayStep;
                        TickIoDevices(ref syncDelay);
                        this.memory.IME = this.memory.NextIMEValue;
                    }
                }

                this.memory.IME = this.memory.NextIMEValue;

                if (this.breakPoints.Contains((ushort)this.Pc) && !this.breakMode)
                {
                    this.breakMode = true;
                    Logger.LogInfo("hit breakpoint $" + this.Pc.ToString("X4"));
                    Logger.Instance.SetMinLogLevel(0);
                }

                if (this.breakMode)
                {
                    Logger.LogVerbose(this.memory.R.Pc.ToString("X4") + ": " + Disassembler.DisassembleInstruction(this.memory));
                }

                byte opcode = (byte)this.memory.LdI8();
                if (this.memory.HaltBugged)
                {
                    this.memory.HaltBugged = false;
                    this.memory.R.Pc--;
                }

                int ticks = 0;
                while (!Instruction.Run(this.memory, opcode, ticks))
                {
                    ticks++;
                    TickIoDevices(ref syncDelay);
                    this.delay += DelayStep;
                }

                this.delay += DelayStep;
            }

            if (syncDelay != this.delay)
            {
                Logger.LogWarning("Left sync delay unsynced sync: " + syncDelay.ToString() + ", this.delay: " + this.delay.ToString());
            }
        }

        private void TickIoDevices(ref int syncDelay)
        {
            this.memory.Lcd.Tick(this.memory);
            this.memory.Update();
            TickDma();
            syncDelay += DelayStep;
        }

        /// <summary>
        /// Disables the LCD renderer.
        /// </summary>
        internal void DisableLcdRenderer() => this.memory.Lcd.ForceNullRender = true;

        /// <summary>
        /// Enables the LCD renderer.
        /// </summary>
        internal void EnableLcdRenderer() => this.memory.Lcd.ForceNullRender = false;

        /// <summary>
        /// Handles the interupts.
        /// </summary>
        /// <param name="step"></param>
        private bool HandleInterupts(int step)
        {
            if (step == 0)
            {
                return (this.memory.IER & this.memory.IF & 0x1F) == 0;
            }

            if (step == 1)
            {
                return false;
            }

            if (step == 2)
            {
                this.memory.Push(this.memory.R.Pc.HighByte);
                return false;
            }

            if (step == 3)
            {
                this.memory.Push(this.memory.R.Pc.LowByte);
                return false;
            }

            if (step == 4)
            {
                byte b = (byte)(this.memory.IER & this.memory.IF & 0x1F);
                for (int i = 0; i < 5; i++)
                {
                    if (b.GetBit((byte)i))
                    {
                        this.memory.R.Pc = new GbUInt16(0, (byte)((i * 8) + 0x40));
                        this.memory.IF &= (byte)(~(1 << i));
                        this.memory.IME = false;
                        this.memory.NextIMEValue = false;
                        return true;
                    }
                }

                throw new InvalidOperationException();
            }

            throw new ArgumentOutOfRangeException(nameof(step));
        }

        /// <summary>
        /// Ticks the dma.
        /// </summary>
        private void TickDma()
        {
            Lcd lcd = this.memory.Lcd;
            if (lcd.DMA < DelayStep * 162)
            {
                if (lcd.DMA != 0)
                {
                    if (lcd.DMA > DelayStep)
                    {
                        this.memory.Oam[(lcd.DMA / DelayStep) - 2] = lcd.DMAValue;
                    }

                    lcd.DMAValue = this.memory.GetMappedMemoryDma(lcd.DMAAddress);
                    lcd.DMAAddress++;
                }

                lcd.DMA += DelayStep;
            }
        }
    }
}
