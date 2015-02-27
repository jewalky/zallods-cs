using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

namespace zallods.Formats
{
    class Sprite16 : Sprite
    {
        public Sprite16(String filename)
        {
            try
            {
                using (FileStream fs = File.Open(filename, FileMode.Open))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    fs.Seek(-4, SeekOrigin.End);
                    uint spriteCount = br.ReadUInt32() & 0x7FFFFFFF;

                    fs.Seek(0, SeekOrigin.Begin);

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
                                    byte[] bytes = new byte[(ipx & 0x3F)];
                                    br.Read(bytes, 0, bytes.Length);
                                    for (int j = 0; j < bytes.Length; j++)
                                    {
                                        int alpha2;
                                        alpha2 = (bytes[j] & 0x0F); alpha2 |= alpha2 << 4;
                                        *px1++ = (ushort)((ushort)((alpha2 & 0xFF) << 8) | 0xFF);
                                        
                                        if ((j != bytes.Length - 1) || ((bytes[bytes.Length - 1] & 0xF0) > 0))
                                        {
                                            alpha2 = (bytes[j] & 0xF0); alpha2 |= alpha2 >> 4;
                                            *px1++ = (ushort)((ushort)((alpha2 & 0xFF) << 8) | 0xFF);
                                        }
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
            RenderColored(index, x, y, 255, 255, 255, 255);
        }

        private Rendering.VertexBuffer VBO = null;
        public override void RenderColored(int index, int x, int y, byte r, byte g, byte b, byte a)
        {
            if (index < 0 || index >= Frames.Count)
                throw new FormatException("Frame index out of bounds.");

            if (VBO == null) VBO = new Rendering.VertexBuffer();
            bool op = MainWindow.RenderPaletted;
            MainWindow.RenderPaletted = false;

            VBO.FixedReset();
            VBO.FixedColor4ub(r, g, b, a);
            VBO.FixedTexCoord2f(0f, 0f);
            VBO.FixedVertex2f(x, y);
            VBO.FixedTexCoord2f(1f, 0f);
            VBO.FixedVertex2f(x + Frames[index].Width, y);
            VBO.FixedTexCoord2f(1f, 1f);
            VBO.FixedVertex2f(x + Frames[index].Width, y + Frames[index].Height);
            VBO.FixedTexCoord2f(0f, 1f);
            VBO.FixedVertex2f(x, y + Frames[index].Height);
            VBO.Update();

            GL.PushAttrib(AttribMask.TextureBit);
            try
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, Frames[index].TextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, 0);
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
