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
        internal const int MCycle = 4;

        /// <summary>
        /// Determines weather the cpu is in break mode or not.
        /// </summary>
        private bool breakMode;

        /// <summary>
        /// The list of addresses this instance will enter <see cref="breakMode"/> at.
        /// </summary>
        private readonly HashSet<ushort> breakPoints = new HashSet<ushort>();

        /// <summary>
        /// The list of addresses this instance will exit <see cref="breakMode"/> at.
        /// </summary>
        private readonly HashSet<ushort> unbreakPoints = new HashSet<ushort>();

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
        public int[] DisplayMemory => this.memory.lcd.displayMemory;

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
        /// Adds a break point at <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The address.</param>
        public void AddBreakPoint(ushort address) => this.breakPoints.Add(address);

        public void AddUnbreakPoint(ushort address) => this.unbreakPoints.Add(address);

        /// <summary>
        /// Gets the LCD's display memory a <see cref="byte"/>[].
        /// </summary>
        /// <returns>The LCD's display memory as a <see cref="byte"/>[].</returns>
        public byte[] DisplayMemoryAsBytes() => this.memory.lcd.DisplayToBytes();

        /// <summary>
        /// Gets the LCD's display memory and stores in into the given buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>A refrence to <paramref name="buffer"/>.</returns>
        public byte[] DisplayMemoryAsBytes(byte[] buffer) => this.memory.lcd.DisplayToBytes(buffer);

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
            byte[] iRom;
            if (rom.Length < 0x150)
            {
                iRom = new byte[0x150];
                for (int i = 0; i < 0x150; i++)
                {
                    iRom[i] = 0xFF;
                }

                Array.Copy(rom, iRom, rom.Length);
            }
            else
            {
                iRom = rom;
            }

            byte mbcMode = iRom[0x147];
            if (mbcMode > 3)
            {
                throw new InvalidOperationException("Unimplemented MBC mode " + mbcMode.ToString("X2"));
            }

            Logger.LogInfo("MBCTYPE:" + iRom[0x147].ToString("X2"));
            Logger.LogInfo("ROMSIZE:" + iRom[0x148].ToString("X2"));
            Logger.LogInfo("RAMSIZE:" + iRom[0x149].ToString("X2"));
            this.memory = new GbMemory(inputHandler)
            {
                RomBanks = 2 << iRom[0x148],
                Rom = new byte[(MemoryRange.ROMBANKSIZE * 2) << iRom[0x148]], // Set the rom size to what the cartrage says.
                ERam = new byte[Cart.GetRamSize(iRom[0x149])],
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
            this.memory.joypad.Update(this.memory); // This is only okay because I'm assuming a single threaded application.
            while (this.delay < 0)
            {
                switch (this.memory.Status)
                {
                    case CpuState.OKAY:
                        HandleOkayMode();
                        continue;

                    case CpuState.HALT:
                        HandleHaltMode();
                        continue;

                    case CpuState.STOP:
                        HandleStopMode();
                        continue;

                    case CpuState.HUNG:
                        HandleHungMode();
                        continue;

                    default:
                        throw new InvalidOperationException("Cpu is in an invalid state.");
                }
            }
        }

        private void HandleBreakPoints()
        {
            if (this.breakPoints.Contains((ushort)this.Pc) && !this.breakMode)
            {
                this.breakMode = true;
                Logger.LogInfo("hit breakpoint $" + this.Pc.ToString("X4"));
                Logger.Instance.SetMinLogLevel(0);
            }

            if (this.unbreakPoints.Contains((ushort)this.Pc) && this.breakMode)
            {
                this.breakMode = false;
                Logger.LogInfo("hit unbreakpoint $" + this.Pc.ToString("X4"));
                Logger.Instance.SetMinLogLevel(1);
            }

            if (this.breakMode)
            {
                Logger.LogVerbose(this.memory.R.Pc.ToString("X4") + ": " + Disassembler.DisassembleInstruction(this.memory));
            }
        }

        private void HandleHaltMode()
        {
            this.memory.Update();
            this.delay += MCycle;
            if ((this.memory.IF & this.memory.IER & 0x1F) > 0)
            {
                this.memory.Status = CpuState.OKAY;
            }
        }

        private void HandleHungMode()
        {
            this.memory.Update();
            this.delay += MCycle;
            if (!this.hung)
            {
                Logger.LogWarning("Cpu has hung.");
                this.hung = true;
            }
        }

        /// <summary>
        /// Handles the interupts.
        /// </summary>
        private void HandleInterupts()
        {
            if (!this.memory.IME || (this.memory.IER & this.memory.IF & 0x1F) == 0)
            {
                this.memory.IME = this.memory.NextIMEValue;
                return;
            }

            this.memory.IME = false;
            this.memory.NextIMEValue = false;

            // Interrupt to service might be decided on the falling edge of second push's M-clock. So
            // timing might be: delay, delay, push, push, discover interrupt/handle pc, delay delay

            this.memory.Update(6);
            this.memory.WriteCyclePush(this.memory.R.Pc.HighByte);
            GbUInt16 newPc = 0;
            int b = this.memory.IER & this.memory.IF & 0x1F;
            for (int i = 0; i < 5; i++)
            {
                if (((b >> i) & 0x1) == 1)
                {
                    newPc = new GbUInt16(0, (byte)((i * 8) + 0x40));
                    this.memory.IF = (byte)(this.memory.IF & ~(1 << i));
                    break;
                }
            }

            this.memory.WriteCyclePush(this.memory.R.Pc.LowByte);
            this.memory.R.Pc = newPc;
            this.memory.Update(2);
            this.delay += MCycle * 4;
        }

        private void HandleOkayMode()
        {
            this.memory.Update(2);
            HandleInterupts();
            HandleBreakPoints();
            this.delay += Instruction.Run(this.memory) * MCycle;
        }

        private void HandleStopMode()
        {
            this.memory.joypad.Update(this.memory);
            this.delay += MCycle;
            if ((this.memory.IF & this.memory.IER & 0x1F) > 0)
            {
                this.memory.Status = CpuState.OKAY;
            }
        }
    }
}
