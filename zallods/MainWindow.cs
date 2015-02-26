﻿using System;
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
            MainShader = new Rendering.Shader();
            MainShader.AddShader(ShaderType.VertexShader, System.IO.File.ReadAllText("data/shaders/main/main.vert"));
            MainShader.AddShader(ShaderType.FragmentShader, System.IO.File.ReadAllText("data/shaders/main/main.frag"));
            MainShader.Compile();
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

        static Rendering.VertexBuffer vbo = null;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (vbo == null)
                vbo = new Rendering.VertexBuffer();

            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            MainShader.Activate();

            // simulate fixed pipeline style vertex pushing
            vbo.FixedReset();
            vbo.FixedColor4f(1f, 1f, 1f, 1f);
            vbo.FixedVertex2f(0f, 0f);
            vbo.FixedVertex2f(32f, 0f);
            vbo.FixedVertex2f(32f, 32f);
            vbo.FixedVertex2f(0f, 32f);
            vbo.Update();
            vbo.Draw(PrimitiveType.Quads);

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
    }
}