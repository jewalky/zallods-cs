using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace zallods.Rendering
{
    class VertexBuffer : IDisposable
    {
        public readonly List<Vertex> Vertices = new List<Vertex>();

        private int VBOID = 0;
        private int VBOSize = 0;
        public int BufferID
        {
            get
            {
                return VBOID;
            }
        }

        public VertexBuffer()
        {
            VBOID = GL.GenBuffer();
            if (VBOID <= 0)
                throw new RenderingException("Unable to generate Vertex Buffer Object.");
        }

        public void Dispose()
        {
            if (VBOID > 0)
                GL.DeleteBuffer(VBOID);
            VBOID = 0;
        }

        public void Update()
        {
            if ((VBOSize == Vertices.Count) && (Vertices.Count <= 0))
                return;

            GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);
            try
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOID);

                if (VBOSize != Vertices.Count) // need to create new buffer
                {
                    unsafe
                    {
                        Vertex[] Mem = Vertices.ToArray();
                        fixed (Vertex* MemPtr = Mem)
                        {
                            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Vertices.Count * Vertex.StructSize), (IntPtr)MemPtr, BufferUsageHint.StreamCopy);
                        }
                    }

                    RenderingException.FromGLError();
                    VBOSize = Vertices.Count;
                }
                else // size matches, just update the items (no need to create new buffer)
                {
                    IntPtr VMemIntPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
                    RenderingException.FromGLError();
                    unsafe
                    {
                        Vertex* VMemPtr = (Vertex*)VMemIntPtr.ToPointer();
                        for (int i = 0; i < Vertices.Count; i++)
                            (*(VMemPtr + i)) = Vertices[i];
                    }
                    GL.UnmapBuffer(BufferTarget.ArrayBuffer);
                }
            }
            finally
            {
                GL.PopClientAttrib();
            }
        }

        public void Draw(PrimitiveType primitive)
        {
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOID);
            GL.ColorPointer(4, ColorPointerType.Float, Vertex.StructSize, Vertex.ColorOffset);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex.StructSize, Vertex.TexCoordOffset);
            GL.VertexPointer(3, VertexPointerType.Float, Vertex.StructSize, Vertex.PositionOffset);

            GL.DrawArrays(primitive, 0, VBOSize);
            GL.Finish();
        }

        private int FixedVertex = 0;
        private Vertex FixedVertexD = new Vertex();
        public void FixedReset()
        {
            FixedVertex = 0;
            Vertices.Clear();
        }

        public void FixedColor4f(float r, float g, float b, float a)
        {
            FixedVertexD.Color = new Vector4(r, g, b, a);
        }

        public void FixedColor4ub(byte r, byte g, byte b, byte a)
        {
            FixedVertexD.Color = new Vector4((float)r / 255f, (float)g / 255f, (float)b / 255f, (float)a / 255f);
        }

        public void FixedTexCoord2f(float x, float y)
        {
            FixedVertexD.TexCoord = new Vector2(x, y);
        }

        public void FixedTexCoord2i(int x, int y)
        {
            FixedVertexD.TexCoord = new Vector2((float)x, (float)y);
        }

        private void FixedPutVertex()
        {
            if (FixedVertex == Vertices.Count)
                Vertices.Add(FixedVertexD);
            else Vertices[FixedVertex] = FixedVertexD;
            FixedVertex++;
        }

        public void FixedVertex2f(float x, float y)
        {
            FixedVertexD.Position = new Vector3(x, y, 0f);
            FixedPutVertex();
        }

        public void FixedVertex2i(int x, int y)
        {
            FixedVertexD.Position = new Vector3((float)x, (float)y, 0f);
            FixedPutVertex();
        }

        public void FixedVertex3f(float x, float y, float z)
        {
            FixedVertexD.Position = new Vector3(x, y, z);
            FixedPutVertex();
        }

        public void FixedVertex3i(int x, int y, int z)
        {
            FixedVertexD.Position = new Vector3((float)x, (float)y, (float)z);
            FixedPutVertex();
        }
    }
}
