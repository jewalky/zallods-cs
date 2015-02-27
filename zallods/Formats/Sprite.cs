using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

namespace zallods.Formats
{
    class SpriteFrame : IDisposable // not interface anyway
    {
        public int Width;
        public int Height;
        public unsafe ushort* Pixels;
        private int bTextureID;

        public int TextureID
        {
            get
            {
                unsafe
                {
                    if (Pixels == null)
                        return 0;
                }

                if (bTextureID > 0) return bTextureID;
                bTextureID = GL.GenTexture();
                if (bTextureID <= 0)
                    throw new Rendering.RenderingException("Unable to generate Texture Object.");
                GL.PushAttrib(AttribMask.TextureBit);
                
                try
                {
                    GL.BindTexture(TextureTarget.Texture2D, bTextureID);
                    unsafe
                    {
                        // apparently, using LuminanceAlpha (and having only two channels, one used for index, another for alpha) is deprecated.
                        // welp. opengl board is gay. have to enlarge the buffer so GL eats it.
                        uint* tmpArray = (uint*)Marshal.AllocHGlobal(Width * Height * 4);
                        ushort* tmpArray0 = Pixels;
                        uint* tmpArray1 = tmpArray;
                        for (int i = 0; i < Width * Height; i++)
                        {
                            uint cext = (uint)(*tmpArray0++) << 16;
                            cext |= (cext & 0x00FF0000) >> 8;
                            cext |= (cext & 0x0000FF00) >> 8;
                            *tmpArray1++ = cext; // A = alpha, R = G = B = index (or 255 for .16)
                        }
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Four, Width, Height,
                            0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, (IntPtr)tmpArray);
                        Marshal.FreeHGlobal((IntPtr)tmpArray);
                    }
                    
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                }
                finally
                {
                    GL.PopAttrib();
                }

                return bTextureID;
            }
        }

        public void Dispose()
        {
            unsafe
            {
                if (Pixels != null)
                    Marshal.FreeHGlobal((IntPtr)Pixels);
                Pixels = null;
            }

            Width = 0;
            Height = 0;

            if (bTextureID != 0)
                GL.DeleteTexture(bTextureID);
            bTextureID = 0;
        }
    }

    abstract class Sprite
    {
        public int GetCount()
        {
            return Frames.Count;
        }

        public int GetWidth(int index)
        {
            if (index < 0 || index >= Frames.Count)
                throw new FormatException("Frame index out of bounds.");
            return Frames[index].Width;
        }

        public int GetHeight(int index)
        {
            if (index < 0 || index >= Frames.Count)
                throw new FormatException("Frame index out of bounds.");
            return Frames[index].Height;
        }

        public unsafe ushort GetPixelAt(int index, int x, int y)
        {
            if (index < 0 || index >= Frames.Count)
                throw new FormatException("Frame index out of bounds.");
            if (x < 0 || x >= Frames[index].Width ||
                y < 0 || y >= Frames[index].Height)
                return 0;
            return Frames[index].Pixels[y * Frames[index].Width + x];
        }

        public abstract void Render(int index, int x, int y);
        public abstract void RenderColored(int index, int x, int y, byte r, byte g, byte b, byte a);

        protected List<SpriteFrame> Frames = new List<SpriteFrame>();
    }
}
