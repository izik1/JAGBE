using System.Drawing.Imaging;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System;

namespace JAGBE.UI
{
    internal static class ContentPipe
    {
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
