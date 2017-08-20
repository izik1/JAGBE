using System.Drawing.Imaging;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System;

namespace JAGBE.UI
{
    /// <summary>
    /// This class contains functions to generate textures.
    /// </summary>
    internal static class ContentPipe
    {
        /// <summary>
        /// Generates a texture with the given <paramref name="bitmap"/><paramref name="width"/> and
        /// <paramref name="height"/>.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>
        /// A new <see cref="Texture2D"/> with width <paramref name="width"/>, height <paramref
        /// name="height"/>, and <see cref="Bitmap"/><paramref name="bitmap"/>
        /// </returns>
        public static Texture2D GenerateTexture(Bitmap bitmap, int width, int height)
        {
            int id = GL.GenTexture();
            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, id);

            GL.TexImage2D(TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgba,
                width, height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte,
                bmpData.Scan0);

            bitmap.UnlockBits(bmpData);

            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            return new Texture2D(id, width, height);
        }

        /// <summary>
        /// Generates A rgba texture with <paramref name="src"/><paramref name="width"/> and
        /// <paramref name="height"/>.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>
        /// A new <see cref="Texture2D"/> with A bitmap made from <paramref name="src"/>, <paramref
        /// name="width"/> and <paramref name="height"/>
        /// </returns>
        public static Texture2D GenerateRgbaTexture(int[] src, int width, int height)
        {
            using (DirectBitmap bitmap = new DirectBitmap(width, height))
            {
                Buffer.BlockCopy(src, 0, bitmap.Bits, 0, width * height * 4);
                return GenerateTexture(bitmap.Bitmap, width, height);
            }
        }
    }
}
