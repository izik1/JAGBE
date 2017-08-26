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

        private readonly Key[] keybinds = { Key.A, Key.S, Key.Space, Key.Enter, Key.Right, Key.Left, Key.Up, Key.Down };

        private byte keys = 0xFF;

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        public Window() : this(160, 144)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Window"/> class.
        /// </summary>
        /// <param name="scale">The scale.</param>
        public Window(int scale) : this(160 * scale, 144 * scale)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Window"/> with the specified attributes.
        /// </summary>
        /// <param name="width">The width of the Window in pixels.</param>
        /// <param name="height">The height of the Window in pixels.</param>
        private Window(int width, int height) : base(width, height, GraphicsMode.Default, "JAGBE Emulator",
            GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible)
        {
            string[] roms = GetRom("config.cfg");
            this.gameBoy = new GameBoy(roms[0], roms[1], this);
            Console.WriteLine("Now Playing: " + Path.GetFileName(roms[0]));
            this.Keyboard.KeyRepeat = false;
        }

        public event EventHandler<InputEventArgs> OnInput;

        /// <summary>
        /// Called when the <see cref="P:OpenTK.INativeWindow.Focused"/> property of the NativeWindow
        /// has changed.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnFocusedChanged(EventArgs e)
        {
            // Left intentionally empty
        }

        /// <summary>
        /// Occurs whenever a keyboard key is pressed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.IsRepeat)
            {
                return;
            }

            int i = Array.IndexOf(this.keybinds, e.Key);

            if (i < 0)
            {
                return;
            }

            this.keys &= ((byte)(~(1 << i)));

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

        /// <summary>
        /// Called when a keyboard key is released.
        /// </summary>
        /// <param name="e">The <see cref="T:OpenTK.Input.KeyboardKeyEventArgs"/> for this event.</param>
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            int i = Array.IndexOf(this.keybinds, e.Key);
            if (i < 0)
            {
                return;
            }

            this.keys |= ((byte)(1 << i));

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

        /// <summary>
        /// Called after an OpenGL context has been established, but before entering the main loop.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.Texture2D);
        }

        /// <summary>
        /// Called when the frame is rendered.
        /// </summary>
        /// <param name="e">Contains information necessary for frame rendering.</param>
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

        /// <summary>
        /// Called when the frame is updated.
        /// </summary>
        /// <param name="e">Contains information necessary for frame updating.</param>
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

        /// <summary>
        /// Gets the rom and boot rom.
        /// </summary>
        /// <param name="configLocation">The expected location of the config file.</param>
        /// <returns>The path of the rom and boot rom</returns>
        private static string[] GetRom(string configLocation)
        {
            string[] strs;
            try
            {
                strs = File.ReadAllLines(configLocation);
            }
            catch (Exception)
            {
                strs = null;
            }

            if (strs == null || strs.Length != 2)
            {
                strs = new string[2];
                Console.WriteLine("Enter path to rom");
                strs[0] = Console.ReadLine();
                Console.WriteLine("Enter path to boot rom");
                strs[1] = Console.ReadLine();
            }

            return strs;
        }
    }
}
