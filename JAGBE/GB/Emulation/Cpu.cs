using System;
using JAGBE.GB.DataTypes;
using JAGBE.GB.Emulation;
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
        /// The delay until the next cycle.
        /// </summary>
        private int delay;

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
        /// The memory of this cpu
        /// </summary>
        private GbMemory memory;

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
        public int[] DisplayMemory => this.memory.lcdMemory.displayMemory;

        /// <summary>
        /// Gets or sets a value indicating whether debug information is active.
        /// </summary>
        /// <value><see langword="true"/> if debug; otherwise, <see langword="false"/>.</value>
        public bool WriteToConsole { get; set; }

        /// <summary>
        /// Resets the memory of this instance.
        /// </summary>
        /// <param name="rom">The rom.</param>
        /// <param name="bootRom">The boot rom.</param>
        /// <param name="inputHandler">The input handler.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Reset(byte[] rom, byte[] bootRom, IInputHandler inputHandler)
        {
            byte ramBanks = rom[0x149];
            if (ramBanks > 2)
            {
                throw new InvalidOperationException();
            }

            if (ramBanks == 2)
            {
                ramBanks *= 4;
            }

            byte mbcMode = rom[0x147];
            if (mbcMode > 3)
            {
                throw new InvalidOperationException();
            }

            this.memory = new GbMemory(inputHandler)
            {
                Rom = new byte[(MemoryRange.ROMBANKSIZE * 2) << rom[0x148]], // set the rom size to what the cartrage says.
                ERam = new GbUInt8[MemoryRange.ERAMBANKSIZE * ramBanks],
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
        /// <exception cref="InvalidOperationException"></exception>
        public void Tick(int cycles)
        {
            this.delay -= cycles;

            // This is to sync the cpu (with variable clock length instructions) to the GPU and APU
            // which always take the same amount of time. In the previous implementation (gpu/apu
            // runs every cpu tick) they would fall behind whenever the cpu runs an instruction that
            // takes > DelayStep cycles.
            int syncDelay = this.delay;
            void TickIoDevices()
            {
                Lcd.Tick(this.memory);
                this.memory.Update();
                syncDelay += DelayStep;
            }
            void TickDMA()
            {
                LcdMemory lcdMem = this.memory.lcdMemory;
                if (lcdMem.DMA < DelayStep * 162)
                {
                    if (lcdMem.DMA != 0)
                    {
                        if (lcdMem.DMA > DelayStep)
                        {
                            this.memory.Oam[(lcdMem.DMA / DelayStep) - 2] = lcdMem.DMAValue;
                        }

                        lcdMem.DMAValue = this.memory.GetMappedMemoryDma(this.memory.lcdMemory.DMAAddress);
                        lcdMem.DMAAddress++;
                    }

                    lcdMem.DMA += DelayStep;
                }
            }

            GbUInt16 prevPc = 0;

            while (this.delay < 0)
            {
                if (this.memory.IME)
                {
                    int step = 0;
                    while (!HandleInterupts(step))
                    {
                        step++;
                        this.delay += DelayStep;
                    }
                }

                this.memory.IME = this.memory.NextIMEValue;

                if (this.memory.Status == CpuState.STOP)
                {
                    this.memory.UpdateKeys();
                    this.delay += DelayStep;
                    if ((this.memory.IF & this.memory.IER & 0x1F) > 0)
                    {
                        this.memory.Status = CpuState.OKAY;
                    }

                    continue;
                }

                TickIoDevices();

                if (this.memory.Status == CpuState.ERROR)
                {
                    this.memory.R.Pc = prevPc; // Don't need to save a temp to be able to restore the pc to...
                    Logger.LogError((prevPc).ToString("X4") + ": " + this.memory.GetMappedMemory(prevPc).ToString("X2") +
                        " (" + Disassembler.DisassembleInstruction(this.memory) + ") --ERR");
                    throw new InvalidOperationException(); // ..Because an exception just gets thrown
                }

                if (this.memory.Status == CpuState.HUNG)
                {
                    Logger.LogVerbose("Cpu has hung.");
                    this.delay += DelayStep;
                    continue;
                }

                if (this.memory.Status == CpuState.HALT)
                {
                    this.delay += DelayStep;
                    if ((this.memory.IF & this.memory.IER & 0x1F) > 0)
                    {
                        this.memory.Status = CpuState.OKAY;
                    }
                    else
                    {
                        continue;
                    }
                }

                TickDMA();

                //TODO: Write breakpoint handler here.

                if (this.WriteToConsole)
                {
                    Logger.LogVerbose(this.memory.R.Pc.ToString("X4") + ": " + Disassembler.DisassembleInstruction(this.memory));
                }

                prevPc = this.Pc;

                Instruction inst = new Instruction(this.memory.LdI8());

                int ticks = 0;
                while (!inst.Run(this.memory, ticks))
                {
                    ticks++;
                    TickIoDevices();
                    TickDMA();
                    this.delay += DelayStep;
                }

                this.delay += DelayStep;
            }

            while (syncDelay < 0)
            {
                TickIoDevices();
                TickDMA();
            }
        }

        /// <summary>
        /// Disables the LCD renderer.
        /// </summary>
        internal void DisableLcdRenderer() => this.memory.lcdMemory.ForceNullRender = true;

        /// <summary>
        /// Enables the LCD renderer.
        /// </summary>
        internal void EnableLcdRenderer() => this.memory.lcdMemory.ForceNullRender = false;

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
    }
}
