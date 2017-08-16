using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using JAGBE.GB;
using JAGBE.GB.Input;
using System.IO;
using OpenTK.Input;

namespace JAGBE.UI
{
    internal sealed class Window : GameWindow, IInputHandler
    {
        private readonly GameBoy gameBoy;

        private byte keys = 0xFF;

        public Window() : this(160, 144)
        {
        }

        public Window(int scale) : this(160 * scale, 144 * scale)
        {
        }

        private Window(int width, int height) : base(width, height, GraphicsMode.Default, "JAGBE Emulator",
            GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible)

        {
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
            this.Keyboard.KeyRepeat = false;
        }

        public event EventHandler<InputEventArgs> OnInput;

        protected override void OnFocusedChanged(EventArgs e)
        {
            // Left intentionally empty
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.IsRepeat)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.A:
                    this.keys = this.keys.Res(0);
                    break;

                case Key.S:
                    this.keys = this.keys.Res(1);
                    break;

                case Key.Space:
                    this.keys = this.keys.Res(2);
                    break;

                case Key.Enter:
                    this.keys = this.keys.Res(3);
                    break;

                case Key.Up:
                    this.keys = this.keys.Res(4);
                    break;

                case Key.Right:
                    this.keys = this.keys.Res(5);
                    break;

                case Key.Down:
                    this.keys = this.keys.Res(6);
                    break;

                case Key.Left:
                    this.keys = this.keys.Res(7);
                    break;

                default:
                    return;
            }

            // Try catch hack because OpenTK catches all exceptions that come from this function...
            // so there wouldn't be a way to tell that it was thrown if this wasn't here. Re-throw
            // the exception anyway in case that changes.
            try
            {
                OnInput?.Invoke(this, new InputEventArgs(this.keys));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                    this.keys = this.keys.Set(0);
                    break;

                case Key.S:
                    this.keys = this.keys.Set(1);
                    break;

                case Key.Space:
                    this.keys = this.keys.Set(2);
                    break;

                case Key.Enter:
                    this.keys = this.keys.Set(3);
                    break;

                case Key.Up:
                    this.keys = this.keys.Set(4);
                    break;

                case Key.Right:
                    this.keys = this.keys.Set(5);
                    break;

                case Key.Down:
                    this.keys = this.keys.Set(6);
                    break;

                case Key.Left:
                    this.keys = this.keys.Set(7);
                    break;

                default:
                    return;
            }

            // Try catch hack because OpenTK catches all exceptions that come from this function...
            // so there wouldn't be a way to tell that it was thrown if this wasn't here. Re-throw
            // the exception anyway in case that changes.
            try
            {
                OnInput?.Invoke(this, new InputEventArgs(this.keys));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
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
