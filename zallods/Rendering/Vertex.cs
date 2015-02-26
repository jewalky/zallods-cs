using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;
using OpenTK;

namespace zallods.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    struct Vertex
    {
        public static readonly int StructSize = Marshal.SizeOf(typeof(Vertex)); // ~4*8
        public static readonly int ColorOffset = 0;
        public static readonly int ColorSize = Marshal.SizeOf(typeof(Vector4));
        public static readonly int PositionOffset = ColorSize;
        public static readonly int PositionSize = Marshal.SizeOf(typeof(Vector3));
        public static readonly int TexCoordOffset = ColorSize + PositionSize;
        public static readonly int TexCoordSize = Marshal.SizeOf(typeof(Vector2));

        public Vector4 Color; //rgba
        public Vector3 Position;
        public Vector2 TexCoord;
    }
}
