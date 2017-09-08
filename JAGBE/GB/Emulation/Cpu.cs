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

        private bool hung;

        /// <summary>
        /// The memory of this cpu
        /// </summary>
        private GbMemory memory;

        /// <summary>
        /// The This is to sync the cpu (with variable clock length instructions) to the GPU, APU
        /// DMA, Timer, etc which always take the same amount of time.
        /// </summary>
        private int syncDelay;

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
        /// Gets a value indicating whether the cpu is in breakmode or not.
        /// </summary>
        /// <value><see langword="true"/> if the cpu is in breakmode; otherwise, <see langword="false"/>.</value>
        public bool BreakMode => this.breakMode;

        /// <summary>
        /// Gets the display memory.
        /// </summary>
        /// <value>The display memory.</value>
        public int[] DisplayMemory => this.memory.Lcd.displayMemory;

        /// <summary>
        /// Gets the status of this instance.
        /// </summary>
        /// <value>The status.</value>
        public CpuState Status => this.memory.Status;

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
        /// Gets the LCD's display memory a <see cref="byte"/>[].
        /// </summary>
        /// <returns>The LCD's display memory as a <see cref="byte"/>[].</returns>
        public byte[] DisplayMemoryAsBytes() => this.memory.Lcd.DisplayToBytes();

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
            byte mbcMode = rom[0x147];
            if (mbcMode > 3)
            {
                throw new InvalidOperationException("Unimplemented MBC mode " + mbcMode.ToString("X2"));
            }

            this.memory = new GbMemory(inputHandler)
            {
                Rom = new byte[(MemoryRange.ROMBANKSIZE * 2) << rom[0x148]], // Set the rom size to what the cartrage says.
                ERam = new GbUInt8[Cart.GetRamSize(rom[0x149])],
                MBCMode = mbcMode == 0 ? MemoryBankController.None : MemoryBankController.MBC1
            };

            Cart.CopyRom(bootRom, rom, this.memory);
        }

        /// <summary>
        /// Runs <paramref name="cycles"/> number of clock ticks.
        /// </summary>
        /// <param name="cycles">The number of clock ticks (NOT INSTRUCTIONS) to run.</param>
        public void Tick(int cycles)
        {
            this.delay -= cycles;
            this.syncDelay = this.delay;
            while (this.delay < 0)
            {
                if (this.memory.Status == CpuState.STOP)
                {
                    HandleStopMode();
                    continue;
                }

                if (this.memory.Status == CpuState.HUNG)
                {
                    HandleHungMode();
                    return; // Return right away to avoid busy looping as much as possible in the core of the emulator.
                }

                TickIoDevices();
                if (this.memory.Status == CpuState.HALT)
                {
                    HandleHaltMode();
                    continue;
                }

                HandleInterupts();
                HandleBreakPoints();
                RunInstruction();
            }

            if (this.syncDelay != this.delay)
            {
                Logger.LogWarning("Left sync delay unsynced sync: " + this.syncDelay.ToString() + ", this.delay: " + this.delay.ToString());
            }
        }

        /// <summary>
        /// Disables the LCD renderer.
        /// </summary>
        internal void DisableLcdRenderer() => this.memory.Lcd.ForceNullRender = true;

        /// <summary>
        /// Enables the LCD renderer.
        /// </summary>
        internal void EnableLcdRenderer() => this.memory.Lcd.ForceNullRender = false;

        private void HandleBreakPoints()
        {
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
        }

        private void HandleHaltMode()
        {
            this.delay += DelayStep;
            if ((this.memory.IF & this.memory.IER & 0x1F) > 0)
            {
                this.memory.Status = CpuState.OKAY;
            }
        }

        private void HandleHungMode()
        {
            if (!this.hung)
            {
                Logger.LogWarning("Cpu has hung.");
                this.hung = true;
            }

            this.delay = 0;
            this.syncDelay = 0;
        }

        /// <summary>
        /// Handles the interupts.
        /// </summary>
        private void HandleInterupts()
        {
            if (!this.memory.IME || (this.memory.IER & this.memory.IF & 0x1F) == 0)
            {
                return;
            }

            void subStep()
            {
                this.delay += DelayStep;
                TickIoDevices();
                this.memory.IME = this.memory.NextIMEValue;
            }

            subStep();
            subStep();
            this.memory.Push(this.memory.R.Pc.HighByte);
            subStep();
            this.memory.Push(this.memory.R.Pc.LowByte);
            subStep();
            byte b = (byte)(this.memory.IER & this.memory.IF & 0x1F);
            for (int i = 0; i < 5; i++)
            {
                if (b.GetBit((byte)i))
                {
                    this.memory.R.Pc = new GbUInt16(0, (byte)((i * 8) + 0x40));
                    this.memory.IF &= (byte)(~(1 << i));
                    this.memory.IME = false;
                    this.memory.NextIMEValue = false;
                    return;
                }
            }
        }

        private void HandleStopMode()
        {
            this.memory.joypad.Update(this.memory);
            this.delay += DelayStep;
            this.syncDelay += DelayStep;
            if ((this.memory.IF & this.memory.IER & 0x1F) > 0)
            {
                this.memory.Status = CpuState.OKAY;
            }
        }

        private void RunInstruction()
        {
            byte opcode = (byte)this.memory.LdI8();
            if (this.memory.HaltBugged)
            {
                this.memory.HaltBugged = false;
                this.memory.R.Pc--;
            }

            int ticks = 0;
            while (!Instruction.Run(this.memory, opcode, ticks++))
            {
                TickIoDevices();
            }

            this.delay += (ticks * DelayStep);
        }

        private void TickIoDevices()
        {
            this.memory.Update();
            this.syncDelay += DelayStep;
        }
    }
}
