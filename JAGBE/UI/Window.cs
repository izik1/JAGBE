using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using JAGBE.GB;
using JAGBE.GB.Input;
using System.IO;

namespace JAGBE.UI
{
    internal sealed class Window : GameWindow, IInputHandler
    {
        private readonly GameBoy gameBoy;

        private readonly GameboyKeys keys;

        public Window() : this(160, 144)
        {
        }

        public Window(int scale) : this(160 * scale, 144 * scale)
        {
        }

        private Window(int width, int height) : base(width, height, GraphicsMode.Default, "JAGBE Emulator",
            GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible)

        {
            this.keys = new GameboyKeys();
            string romPath;
            string bootRomPath;
            if (File.Exists("config.cfg"))
            {
                string[] strs = File.ReadAllLines("config.cfg");
                if (strs.Length == 2)
                {
                    romPath = strs[0];
                    bootRomPath = strs[1];
                    Console.WriteLine("Loaded from config.");
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                Console.WriteLine("Enter path to rom");
                romPath = Console.ReadLine();
                Console.WriteLine("Enter path to boot rom");
                bootRomPath = Console.ReadLine();
            }
            this.gameBoy = new GameBoy(romPath, bootRomPath, this);
        }

        public event EventHandler<InputEventArgs> OnInput;

        protected override void OnFocusedChanged(EventArgs e)
        {
            // Left intentionally empty
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.Texture2D);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Texture2D tex = ContentPipe.GenerateRgbaTexture(this.gameBoy.cpu.DisplayMemory, 160, 144);
            GL.BindTexture(TextureTarget.Texture2D, tex.Id);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 1);
            GL.Vertex2(-1, 1);
            GL.TexCoord2(0, 0);
            GL.Vertex2(-1, -1);
            GL.TexCoord2(1, 0);
            GL.Vertex2(1, -1);
            GL.TexCoord2(1, 1);
            GL.Vertex2(1, 1);
            GL.End();
            GL.DeleteTexture(tex.Id);
            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (this.gameBoy.cpu.WriteToConsole)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                }
                else
                {
                    return;
                }
            }

            this.gameBoy.Update((int)this.TargetUpdateFrequency);
        }
    }
}
