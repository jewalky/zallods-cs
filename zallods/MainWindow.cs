using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace zallods
{
    class MainWindow : GameWindow
    {
        private static Rendering.Shader MainShader = null;
        public static Rendering.Shader Shader
        {
            get
            {
                return MainShader;
            }
        }

        public MainWindow() : base(640, 480)
        {
            if (Instance != null)
                throw new Exception("Tried to create more than one instance of zallods::MainWindow!");
            Instance = this;

            WindowBorder = WindowBorder.Fixed;
            Cursor = MouseCursor.Empty;
            Title = "ZAllods";
        }

        protected override void OnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if ((e.Key == Key.F4) && ((e.Modifiers & KeyModifiers.Alt) == KeyModifiers.Alt))
            {
                Exit();
                return;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            // init alpha blending
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            MainShader = new Rendering.Shader();
            MainShader.AddShader(ShaderType.VertexShader, System.IO.File.ReadAllText("data/shaders/main/main.vert"));
            MainShader.AddShader(ShaderType.FragmentShader, System.IO.File.ReadAllText("data/shaders/main/main.frag"));
            MainShader.Compile();
            RenderStencil = RenderStencil;
            RenderPaletted = RenderPaletted;
            MainShader.SetUniform("tex1", (int)0);
            MainShader.SetUniform("tex2", (int)1);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            MainShader.SetUniform("main_width", (float)Width);
            MainShader.SetUniform("main_height", (float)Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            
        }

        int FPS_Current = 0;
        int FPS_LastTicks = 0;
        int FPS_Counter = 0;
        public int FPS
        {
            get
            {
                return FPS_Current;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            FPS_Counter++;
            if (GetTickCount() - FPS_LastTicks > 1000)
            {
                FPS_Current = FPS_Counter - 1;
                FPS_Counter = 1;
                FPS_LastTicks = GetTickCount();
                //Console.WriteLine("FPS = {0}", FPS_Current);
            }

            GL.ClearColor(0f, 0f, 0.5f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            MainShader.Activate();
            // start actual rendering

            //RenderStencil = true;
            //img.Render(8, 0, 0);
            //RenderStencil = false;

            // finish actual rendering
            MainShader.Deactivate();
            SwapBuffers();
        }

        protected override void OnClosed(EventArgs e)
        {
            Instance = null;
        }

        private static MainWindow Instance = null;
        private static System.Diagnostics.Stopwatch Stopwatch = new System.Diagnostics.Stopwatch();

        public static int GetTickCount()
        {
            return (int)Stopwatch.ElapsedMilliseconds;
        }

        public static void Sleep(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        // this is also called from toplevel widgets to implement "pause" of all widgets except the active one.
        public static bool DoMainLoop()
        {
            if (Instance == null) return false;
            
            Instance.ProcessEvents(false);

            bool exv = (Instance != null && Instance.Exists && !Instance.IsExiting);
            if (!exv)
            {
                if (Instance != null)
                {
                    Instance.Exit();
                    Instance.ProcessEvents(true);
                    Instance.Dispose();
                }

                Instance = null;
            }
            else
            {
                FrameEventArgs fea = new FrameEventArgs();
                Instance.OnUpdateFrame(fea);
                Instance.OnRenderFrame(fea);
            }

            return exv;
        }

        static void MainExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            if (Instance != null)
            {
                Instance.Exit();
                Instance.ProcessEvents(true);
                Instance.Dispose();
            }

            Instance = null;
            Console.WriteLine("[!!] Uncaught exception has occurred."+Environment.NewLine);
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine(e.ToString()+Environment.NewLine);

            Console.Write("Press a key to exit.");
            ConsoleKeyInfo ki = Console.ReadKey();
            Console.Write(((ki.KeyChar >= 0x20)?"\b \b":"")+Environment.NewLine);

            Environment.Exit(3);
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MainExceptionHandler);

            Instance = new MainWindow();
            Instance.Visible = true;
            Instance.OnLoad(EventArgs.Empty);
            Instance.OnResize(EventArgs.Empty);

            bool ShadersSupported = (new Version(GL.GetString(StringName.Version).Substring(0, 3)) >= new Version(2, 0));
            if (!ShadersSupported)
            {
                Console.WriteLine("[!!] ZAllods couldn't start. OpenGL 2.0 is required in order to play ZAllods.");
                return;
            }

            Console.WriteLine("[==] OpenGL 2.0 is supported. Proceeding with startup.");

            Stopwatch.Start(); // initialize tick counter
            while (DoMainLoop())
                Sleep(1);
        }

        private static bool bRenderPaletted = false;
        private static bool bRenderStencil = false;

        public static bool RenderPaletted
        {
            get
            {
                return bRenderPaletted;
            }
            set
            {
                bRenderPaletted = value;
                if (MainShader != null)
                    MainShader.SetUniform("main_paletted", bRenderPaletted ? 1f : 0f);
            }
        }

        public static bool RenderStencil
        {
            get
            {
                return bRenderStencil;
            }
            set
            {
                bRenderStencil = value;
                if (MainShader != null)
                    MainShader.SetUniform("main_stencil", bRenderStencil ? 1f : 0f);
            }
        }
    }
}
