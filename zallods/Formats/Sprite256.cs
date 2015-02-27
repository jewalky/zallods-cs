using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

namespace zallods.Formats
{
    class Sprite256 : Sprite
    {
        private Palette bOwnPalette = null;
        public Palette Palette
        {
            get
            {
                return bOwnPalette;
            }
        }

        public Sprite256(String filename)
        {
            try
            {
                using (FileStream fs = File.Open(filename, FileMode.Open))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    fs.Seek(-4, SeekOrigin.End);
                    uint spriteCount = br.ReadUInt32() & 0x7FFFFFFF;

                    fs.Seek(0, SeekOrigin.Begin);
                    // read in the palette. 256 entries.
                    uint[] spritePalette = new uint[256];
                    for (int i = 0; i < 256; i++)
                        spritePalette[i] = br.ReadUInt32();
                    bOwnPalette = new Palette(spritePalette);

                    // read in the frames
                    for (int i = 0; i < spriteCount; i++)
                    {
                        int fWidth = br.ReadInt32();
                        int fHeight = br.ReadInt32();
                        int fDataSize = br.ReadInt32();
                        long posAfter = fs.Position + fDataSize;

                        unsafe
                        {
                            ushort* px = (ushort*)Marshal.AllocHGlobal(fWidth * fHeight * 2);
                            for (int k = 0; k < fWidth * fHeight; k++)
                                px[k] = 0;
                            ushort* px1 = px;

                            while (fDataSize > 0)
                            {
                                // read in the data
                                ushort ipx = br.ReadByte();
                                ipx |= (ushort)(ipx << 8);
                                ipx &= 0xC03F;
                                fDataSize -= 1;

                                if ((ipx & 0xC000) > 0)
                                {
                                    if ((ipx & 0xC000) == 0x4000)
                                    {
                                        px1 += fWidth * (ipx & 0x3F);
                                    }
                                    else
                                    {
                                        px1 += (ipx & 0x3F);
                                    }
                                }
                                else
                                {
                                    for (int j = 0; j < (ipx & 0x3F); j++)
                                    {
                                        byte ss = br.ReadByte();
                                        *px1++ = (ushort)((ushort)0xFF00 | ss);
                                    }

                                    fDataSize -= (ipx & 0x3F);
                                }
                            }

                            SpriteFrame sf = new SpriteFrame();
                            sf.Width = fWidth;
                            sf.Height = fHeight;
                            sf.Pixels = px;
                            Frames.Add(sf);
                        }

                        fs.Seek(posAfter, SeekOrigin.Begin);
                    }
                }
            }
            catch (IOException)
            {
                throw;
                //throw new FormatException("Failed to open the image.");
            }
        }

        public override void Render(int index, int x, int y)
        {
            RenderEx(index, x, y, 255, 255, 255, 255, bOwnPalette, false, 0);
        }

        public override void RenderColored(int index, int x, int y, byte r, byte g, byte b, byte a)
        {
            RenderEx(index, x, y, r, g, b, a, bOwnPalette, false, 0);
        }

        private Rendering.VertexBuffer VBO = null;
        public void RenderEx(int index, int x, int y, byte r, byte g, byte b, byte a, Palette pal, bool flip, int xoffs)
        {
            if (index < 0 || index >= Frames.Count)
                throw new FormatException("Frame index out of bounds.");

            if (VBO == null) VBO = new Rendering.VertexBuffer();
            bool op = MainWindow.RenderPaletted;
            MainWindow.RenderPaletted = true;

            VBO.FixedReset();
            VBO.FixedColor4ub(r, g, b, a);
            float left = flip ? 1f : 0f;
            float right = flip ? 0f : 1f;
            VBO.FixedTexCoord2f(left, 0f);
            VBO.FixedVertex2f(x + xoffs, y);
            VBO.FixedTexCoord2f(right, 0f);
            VBO.FixedVertex2f(x + xoffs + Frames[index].Width, y);
            VBO.FixedTexCoord2f(right, 1f);
            VBO.FixedVertex2f(x + Frames[index].Width, y + Frames[index].Height);
            VBO.FixedTexCoord2f(left, 1f);
            VBO.FixedVertex2f(x, y + Frames[index].Height);
            VBO.Update();

            GL.PushAttrib(AttribMask.TextureBit);
            try
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, Frames[index].TextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, pal.TextureID);
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
