using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System;

namespace JAGBE.UI
{
    /// <summary>
    /// Creates fast access to individual bytes in a bitmap by GC pinning them
    /// </summary>
    /// <seealso cref="IDisposable"/>
    internal sealed class DirectBitmap : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectBitmap"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public DirectBitmap(int width, int height) : this(width, height, new int[width * height])
        {
        }

        public DirectBitmap(int width, int height, int[] bits)
        {
            if (bits == null)
            {
                throw new ArgumentNullException(nameof(bits));
            }

            if (bits.Length != width * height)
            {
                throw new ArgumentException(nameof(bits) + ".length must equal " + nameof(width) + " * " + nameof(height));
            }

            this.Width = width;
            this.Height = height;
            this.Bits = bits;
            this.BitsHandle = GCHandle.Alloc(this.Bits, GCHandleType.Pinned);
            this.Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, this.BitsHandle.AddrOfPinnedObject());
        }

        /// <summary>
        /// Gets the bitmap.
        /// </summary>
        /// <value>The bitmap.</value>
        public Bitmap Bitmap { get; }

        /// <summary>
        /// Gets the bits.
        /// </summary>
        /// <value>The bits.</value>
        public int[] Bits { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="DirectBitmap"/> is disposed.
        /// </summary>
        /// <value><see langword="true"/> if disposed; otherwise, <see langword="false"/>.</value>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>The height.</value>
        public int Height { get; }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>The width.</value>
        public int Width { get; }

        /// <summary>
        /// Gets the bits handle.
        /// </summary>
        /// <value>The bits handle.</value>
        private GCHandle BitsHandle { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.Disposed)
            {
                this.Disposed = true;
                this.Bitmap.Dispose();
                this.BitsHandle.Free();
            }
        }
    }
}
