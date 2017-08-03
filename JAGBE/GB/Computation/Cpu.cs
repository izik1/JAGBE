using System;
using System.Diagnostics;
using JAGBE.GB.DataTypes;
using JAGBE.GB.Computation.Execution;

namespace JAGBE.GB.Computation
{
    internal sealed class Cpu
    {
        /// <summary>
        /// The clock speed in hz
        /// </summary>
        /// <value>4194304</value>
        internal const int ClockSpeedHz = 4194304;

        internal const int DelayStep = 4;

        /// <summary>
        /// The delay until the next cycle.
        /// </summary>
        private int delay;

        internal GbUInt16 Pc => this.memory.R.Pc;

        /// <summary>
        /// This re-dumps the memory each time it's called.
        /// </summary>
        /// <value>The ram dump.</value>
        internal byte[] RamDump
        {
            get => memory.DumpRam();
        }

        private GbMemory memory = new GbMemory();

        public Cpu(byte[] bootRom, byte[] rom) => Reset(rom, bootRom);

        public int[] DisplayMemory => this.memory.lcdMemory.displayMemory;

        public bool WriteToConsole { get; set; } = true;

        public void Reset(byte[] rom, byte[] bootRom)
        {
            this.memory = new GbMemory
            {
                Rom = new byte[(1024 * 32) << rom[0x148]] // set the rom size to what the cartrage says.
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
            void TickIoDevices()
            {
                Lcd.Tick(this.memory);
                syncDelay += DelayStep;
            }
            void TickDMA()
            {
                if (this.memory.lcdMemory.DMA < DelayStep * 162)
                {
                    if (this.memory.lcdMemory.DMA != 0)
                    {
                        if (this.memory.lcdMemory.DMA > DelayStep)
                        {
                            this.memory.Oam[(this.memory.lcdMemory.DMA / DelayStep) - 2] = this.memory.lcdMemory.DMAValue;
                        }

                        this.memory.lcdMemory.DMAValue = this.memory.GetMappedMemory(this.memory.lcdMemory.DMAAddress);
                        this.memory.lcdMemory.DMAAddress++;
                    }

                    this.memory.lcdMemory.DMA += DelayStep;
                }
            }

            while (this.delay < 0)
            {
                if (this.memory.IME)
                {
                    HandleInterupts();
                }

                this.memory.IME = this.memory.NextIMEValue;

                if (this.memory.Status == CpuState.STOP)
                {
                    continue;
                }

                TickIoDevices();

                if (this.memory.Status == CpuState.ERROR)
                {
                    //TODO: Hang correctly. Since when a gameboy actually hangs gfx, sounds, DMA, etc still work.
                    Console.WriteLine("(" + this.memory.GetMappedMemory((ushort)(this.memory.R.Pc - 1)).ToString("X2") +
                        ") {" + (this.memory.R.Pc - 1).ToString("X4") + "} ERR");
                    throw new InvalidOperationException();
                }

                if (this.memory.Status == CpuState.HUNG)
                {
                    Console.WriteLine("Hung...");
                    this.delay += DelayStep;
                    continue;
                }

                if (this.memory.Status == CpuState.HALT)
                {
                    this.delay += DelayStep;
                    if ((this.memory.GetMappedMemory(0xFF0F) & this.memory.GetMappedMemory(0xFFFF) & 0x1F) > 0)
                    {
                        this.memory.Status = CpuState.OKAY;
                    }
                    else
                    {
                        continue;
                    }
                }

                TickDMA();
                if (this.WriteToConsole)
                {
                    Console.WriteLine(
                        "(" + this.memory.GetMappedMemory(this.memory.R.Pc).ToString("X2") + ") {" + this.memory.R.Pc.ToString("X4") + "}");
                }

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
        private void HandleInterupts()
        {
            byte I = (byte)(this.memory.GetMappedMemory(IoReg.IF) & this.memory.GetMappedMemory(0xFFFF));
            int x = 0;
            int i;
            for (i = 0; i < 5 && x == 0; i++)
            {
                if (I.GetBit((byte)i))
                {
                    x = 0x40 + (8 * i);
                }
            }

            if (x > 0)
            {
                this.memory.SetMappedMemory(IoReg.IF, this.memory.GetMappedMemory(IoReg.IF).Res((byte)(i - 1)));
                this.memory.Push(new GbUInt16((ushort)(this.memory.R.Pc - 1)));
                this.memory.R.Pc = new GbUInt16(0, (byte)x);
                this.memory.IME = false;
            }
        }
    }
}
