using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace zallods.Rendering
{
    class Shader : IDisposable
    {
        // basically, this is an interface to a Shader Program.
        // 
        private int ShaderID = 0;
        private bool ShaderCompiled = false;
        public int ProgramID
        {
            get
            {
                return ShaderID;
            }
        }

        public Shader()
        {
            ShaderID = GL.CreateProgram();
            if (ShaderID <= 0)
                throw new RenderingException("Unable to generate Shader Program Object.");
        }

        public void Dispose()
        {
            if (ShaderID > 0)
                GL.DeleteProgram(ShaderID);
            ShaderID = 0;
        }

        public void AddShader(ShaderType type, String code)
        {
            if (ShaderID <= 0)
                throw new RenderingException("Can't add shaders to an invalid program!");
            if (ShaderCompiled)
                throw new RenderingException("Can't add shaders to an already compiled program!");

            int shader = GL.CreateShader(type);
            try
            {
                if (shader <= 0)
                    throw new RenderingException("Unable to generate Shader Object.");

                GL.ShaderSource(shader, code);
                GL.CompileShader(shader);

                int shader_status;
                GL.GetShader(shader, ShaderParameter.CompileStatus, out shader_status);
                if (shader_status != 1)
                {
                    String shader_infolog = GL.GetShaderInfoLog(shader);
                    String msg = "Unable to compile shader:" + Environment.NewLine + shader_infolog;
                    throw new RenderingException(msg);
                }

                GL.AttachShader(ShaderID, shader);
            }
            finally
            {
                if (shader > 0)
                    GL.DeleteShader(shader);
            }

            RenderingException.FromGLError();
        }

        public void Compile()
        {
            if (ShaderID <= 0)
                throw new RenderingException("Can't compile an invalid program!");
            GL.LinkProgram(ShaderID);
            int program_status;
            GL.GetProgram(ShaderID, GetProgramParameterName.LinkStatus, out program_status);
            if (program_status != 1)
            {
                String program_infolog = GL.GetProgramInfoLog(ShaderID);
                String msg = "Unable to link shader program:" + Environment.NewLine + program_infolog;
                Dispose();
                throw new RenderingException(msg);
            }

            ShaderCompiled = true;
        }

        public void SetUniform(String name, params float[] values)
        {
            if (ShaderID <= 0)
                throw new RenderingException("Can't set uniforms in an invalid program!");
            if (!ShaderCompiled)
                throw new RenderingException("Can't set uniforms in a not-yet-compiled program!");
            if (values.Length <= 0)
                return;
            if (values.Length > 4)
                throw new RenderingException("Uniform size larger than 4 is not supported.");

            int prog = GL.GetInteger(GetPName.CurrentProgram);
            GL.UseProgram(ShaderID);

            try
            {
                int uloc = GL.GetUniformLocation(ShaderID, name);
                if (uloc < 0)
                    throw new RenderingException("Nonexistent uniform name.");
                switch (values.Length)
                {
                    case 1:
                        GL.Uniform1(uloc, values[0]);
                        break;
                    case 2:
                        GL.Uniform2(uloc, values[0], values[1]);
                        break;
                    case 3:
                        GL.Uniform3(uloc, values[0], values[1], values[2]);
                        break;
                    case 4:
                        GL.Uniform4(uloc, values[0], values[1], values[2], values[3]);
                        break;
                }

                RenderingException.FromGLError();
            }
            finally
            {
                GL.UseProgram(prog);
            }
        }

        public void Activate()
        {
            if (ShaderID <= 0)
                throw new RenderingException("Can't activate an invalid program!");
            if (!ShaderCompiled)
                throw new RenderingException("Can't activate a not-yet-compiled program!");
            GL.UseProgram(ShaderID);
        }

        public void Deactivate()
        {
            GL.UseProgram(0);
        }
    }
}
