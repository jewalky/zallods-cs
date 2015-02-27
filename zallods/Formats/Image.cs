using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using System.Drawing;
using System.Drawing.Imaging;

using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

namespace zallods.Formats
{
    class Image : IDisposable
    {
        private void StaticInit(String filename, bool haveckey, uint colorkey)
        {
            try
            {
                using (Bitmap bm = new Bitmap(filename))
                {
                    bWidth = bm.Width;
                    bHeight = bm.Height;

                    unsafe
                    {
                        Pixels = (uint*)Marshal.AllocHGlobal(bWidth * bHeight * 4); // new uint[Width*Height]
                        BitmapData bmd = bm.LockBits(new Rectangle(0, 0, bWidth, bHeight), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        uint* px1 = (uint*)bmd.Scan0;
                        uint* px2 = Pixels;
                        if (haveckey)
                        {
                            colorkey &= 0xF0F0F0;
                            for (int i = 0; i < bWidth * bHeight; i++)
                            {
                                if ((*px1 & 0xF0F0F0) == colorkey)
                                    *px2 = *px1;
                                else *px2 = 0;
                                px2++;
                                px1++;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < bWidth * bHeight; i++)
                                *px2++ = *px1++;
                        }

                        bm.UnlockBits(bmd);
                    }
                }
            }
            catch (Exception)
            {
                throw new FormatException("Failed to open the image.");
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

            bWidth = 0;
            bHeight = 0;

            if (bTextureID != 0)
                GL.DeleteTexture(bTextureID);
            bTextureID = 0;
        }

        public Image(String filename, uint colorkey)
        {
            StaticInit(filename, true, colorkey);
        }

        public Image(String filename)
        {
            StaticInit(filename, false, 0);
        }

        // fields
        private int bTextureID = 0;
        private int bWidth = 0;
        private int bHeight = 0;
        private unsafe uint* Pixels = null;

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
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Four, bWidth, bHeight,
                            0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, (IntPtr)Pixels);
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

        public int Width
        {
            get
            {
                return bWidth;
            }
        }

        public int Height
        {
            get
            {
                return bHeight;
            }
        }

        public unsafe uint GetPixelAt(int x, int y)
        {
            if (x < 0 || x >= bWidth ||
                y < 0 || y >= bHeight) return 0;
            return Pixels[y * Width + x];
        }

        public void Render(int x, int y)
        {
            RenderPart(x, y, 0, 0, bWidth, bHeight);
        }

        public void RenderPart(int x, int y, int inx, int iny, int inw, int inh)
        {
            RenderEx(x, y, inx, iny, inw, inh, 255, 255, 255, 255);
        }

        private Rendering.VertexBuffer VBO = null;
        public void RenderEx(int x, int y, int inx, int iny, int inw, int inh, byte r, byte g, byte b, byte a)
        {
            if (VBO == null) VBO = new Rendering.VertexBuffer();
            bool op = MainWindow.RenderPaletted;
            MainWindow.RenderPaletted = false;

            float fStepX = 1f / Width;
            float fStepY = 1f / Height;
            VBO.FixedReset();
            VBO.FixedColor4ub(r, g, b, a);
            VBO.FixedTexCoord2f(fStepX * inx, fStepY * iny);
            VBO.FixedVertex2i(x, y);
            VBO.FixedTexCoord2f(fStepX * (inx + inw), fStepY * iny);
            VBO.FixedVertex2i(x + inw, y);
            VBO.FixedTexCoord2f(fStepX * (inx + inw), fStepY * (iny + inh));
            VBO.FixedVertex2i(x + inw, y + inh);
            VBO.FixedTexCoord2f(fStepX * inx, fStepY * (iny + inh));
            VBO.FixedVertex2i(x, y + inh);
            VBO.Update();

            GL.PushAttrib(AttribMask.TextureBit);
            try
            {
                GL.BindTexture(TextureTarget.Texture2D, TextureID);
                VBO.Draw(PrimitiveType.Quads);
            }
            finally
            {
                GL.PopAttrib();
            }

            MainWindow.RenderPaletted = op;
        }
    }
}
