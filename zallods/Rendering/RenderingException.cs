using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Graphics.OpenGL;

namespace zallods.Rendering
{
    class RenderingException : Exception
    {
        public RenderingException() : base() { }
        public RenderingException(String message) : base(message) { }
        
        public static void FromGLError()
        {
            ErrorCode ec = GL.GetError();
            if (ec != ErrorCode.NoError)
            {
                while (GL.GetError() != ErrorCode.NoError) ;
                throw new RenderingException("OpenGL Error: " + ec.ToString());
            }
        }
    }
}
