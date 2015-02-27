using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using OpenTK.Graphics.OpenGL;

namespace zallods.Formats
{
    class Palette
    {
        public static readonly int BMPOffset = 0x36; // offset for reading palette from .pal files

        private void StaticInit(uint[] palette)
        {
            for (int i = 0; i < 256; i++)
            {
                if (i < palette.Length)
                    bColors[i] = palette[i];
                else bColors[i] = 0;
            }
        }

        public Palette(String filename, int offset)
        {
            using (FileStream fs = File.Open(filename, FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
            {
                fs.Seek(offset, SeekOrigin.Begin);
                uint[] palette = new uint[256];
                for (int i = 0; i < 256; i++)
                    palette[i] = br.ReadUInt32();
                StaticInit(palette);
            }
        }

        public Palette(uint[] palette)
        {
            StaticInit(palette);   
        }

        private readonly uint[] bColors = new uint[256];
        public uint[] Colors
        {
            get
            {
                return bColors;
            }
        }

        private int bTextureID = 0;
        public int TextureID
        {
            get
            {
                if (bTextureID > 0) return bTextureID;
                bTextureID = GL.GenTexture();
                if (bTextureID <= 0)
                    throw new Rendering.RenderingException("Unable to generate Texture Object.");
                GL.PushAttrib(AttribMask.TextureBit);
                GL.BindTexture(TextureTarget.Texture2D, bTextureID);

                try
                {
                    unsafe
                    {
                        fixed (uint* colors = bColors)
                        {
                            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Four, 256, 1, 0, PixelFormat.Bgra, PixelType.UnsignedByte, (IntPtr)colors);
                        }
                    }

                    // does TextureWrapT do anything here anyway?
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
    }
}
