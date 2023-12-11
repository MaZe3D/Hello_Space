using System.Reflection;
using System.Diagnostics;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using NAudio.Wave;
using NAudio.Dsp;
using System.Runtime.InteropServices;
using NAudio.Utils;


namespace Hello_Space
{
    internal class Game : GameWindow
    {
        Features features = new Features()
        {
            Audio = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        };
        Audio? audio;

        float lowPassSample = 1;
        float midPassSample = 1;
        float highPassSample = 1;
        float AudioBasedTime = 0;

        float timestamp = 0;

        // === Graphics ===

        // set of vertices to draw the triangle with (x,y,z) for each vertex
        List<Vector2> vertices = new()
        {
            // front face
            new Vector2(-1f, 1f),  // topleft vert
            new Vector2(1f,  1f),  // topright vert
            new Vector2(-1f, -1f), // bottomleft vert
            new Vector2(1f,  -1f)  // bottomright vert
        };

        // Render Pipeline vars
        int vao;
        int shaderProgram;
        int vbo;
        Stopwatch playTime = new Stopwatch();
        Stopwatch frameTime = new Stopwatch();
        // width and height of screen
        Vector2i resolution;

        // Constructor that sets the width, height, and calls the base constructor (GameWindow's Constructor) with default args
        public Game(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.resolution.X = width;
            this.resolution.Y = height;

            // center window
            CenterWindow(new Vector2i(width, height));
        }
        // called whenever window is resized
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
            resolution.X = e.Width;
            resolution.Y = e.Height;
        }

        // called once when game is started
        protected override void OnLoad()
        {
            // Graphics
            base.OnLoad();

            // generate the vao
            vao = GL.GenVertexArray();

            // bind the vao
            GL.BindVertexArray(vao);

            // generate a buffer
            vbo = GL.GenBuffer();
            // bind the buffer as an array buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            // Store data in the vbo
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Vector2.SizeInBytes, vertices.ToArray(), BufferUsageHint.StaticDraw);

            // put the vertex VBO in slot 0 of our VAO
            // point slot (0) of the VAO to the currently bound VBO (vbo)
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
            // enable the slot
            GL.EnableVertexArrayAttrib(vao, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // create the shader program
            shaderProgram = GL.CreateProgram();

            // create the vertex shader
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            // add the source code from "Default.vert" in the Shaders file
            GL.ShaderSource(vertexShader, LoadShaderSource("res/shaders/Default.vert"));
            // Compile the Shader
            GL.CompileShader(vertexShader);

            // Same as vertex shader
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, LoadShaderSource("res/shaders/Default.frag"));
            GL.CompileShader(fragmentShader);

            // Attach the shaders to the shader program
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);

            // Link the program to OpenGL
            GL.LinkProgram(shaderProgram);

            // delete the shaders
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // GL.Enable(EnableCap.DepthTest);

            // Set the color to fill the screen with
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1f);

            // draw our triangle
            GL.UseProgram(shaderProgram); // bind vao

            // GL.GetUniformLocation(shaderProgram, "timestamp");
            if (features.Audio)
            {
                try
                {
                    audio = new Audio(GetResourcePath("res/audio/audio.flac"));
#pragma warning disable CS8622 // Die NULL-Zulässigkeit von Verweistypen im Typ des Parameters entspricht (möglicherweise aufgrund von Attributen für die NULL-Zulässigkeit) nicht dem Zieldelegaten.
                    audio.waveOut.PlaybackStopped += OnPlaybackStopped;
#pragma warning restore CS8622 // Die NULL-Zulässigkeit von Verweistypen im Typ des Parameters entspricht (möglicherweise aufgrund von Attributen für die NULL-Zulässigkeit) nicht dem Zieldelegaten.
                    audio.StartFromBeginning();

                }
                catch (FileNotFoundException e)
                {
                    Debug.WriteLine($"Error while loading audio: {e.Message}");
                    Debug.WriteLine($"Feature {nameof(features.Audio)} is disabled.");

                }
            }
        }
        // called once when game is closed
        protected override void OnUnload()
        {
            base.OnUnload();

            if (audio != null)
            {
#pragma warning disable CS8622 // Die NULL-Zulässigkeit von Verweistypen im Typ des Parameters entspricht (möglicherweise aufgrund von Attributen für die NULL-Zulässigkeit) nicht dem Zieldelegaten.                audio.waveOut.PlaybackStopped -= OnPlaybackStopped;
                audio.waveOut.PlaybackStopped -= OnPlaybackStopped;
#pragma warning restore CS8622 // Die NULL-Zulässigkeit von Verweistypen im Typ des Parameters entspricht (möglicherweise aufgrund von Attributen für die NULL-Zulässigkeit) nicht dem Zieldelegaten.
                audio.waveOut.Stop();
                audio.Dispose();
            }

            // Stop Stopwatch
            playTime.Stop();

            // Delete, VAO, VBO, Shader Program
            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);
            GL.DeleteProgram(shaderProgram);
        }
        // called every frame. All rendering happens here
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            // Fill the screen with the color
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(vao); // use shader program

            GL.Uniform1((int)Locations.TimeStamp, timestamp);
            GL.Uniform2((int)Locations.Resolution, resolution);
            GL.Uniform1((int)Locations.LowSample, lowPassSample);
            GL.Uniform1((int)Locations.MidSample, midPassSample);
            GL.Uniform1((int)Locations.HighSample, highPassSample);

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            // swap the buffers
            Context.SwapBuffers();

            base.OnRenderFrame(args);
            frameTime.Restart();
        }
        // called every frame. All updating happens here
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            var timeLastFrame = 0.1f;//(float)frameTime.Elapsed.TotalSeconds;
            frameTime.Restart();
            base.OnUpdateFrame(args);
            KeyboardHandler();

            try
            {
                timestamp = (float)(audio?.waveOut.GetPositionTimeSpan().TotalSeconds ?? playTime.Elapsed.TotalSeconds);
            }
            catch { }

            var timeOffset = timestamp - timeLastFrame / 2;

            /* lowPassSample = audio?.GetSampleAtTime(timestamp, AudioFrequencyBand.Bass).Left ?? 1f;
            midPassSample = audio?.GetSampleAtTime(timestamp, AudioFrequencyBand.Mid).Left ?? 1f;
            highPassSample = audio?.GetSampleAtTime(timestamp, AudioFrequencyBand.High).Left ?? 1f; */

            lowPassSample = audio?.GetSampleAtTimeSpan(timeOffset, timeLastFrame, AudioFrequencyBand.Bass).Left ?? 1f;
            midPassSample = audio?.GetSampleAtTimeSpan(timeOffset, timeLastFrame, AudioFrequencyBand.Mid).Left ?? 1f;
            highPassSample = audio?.GetSampleAtTimeSpan(timeOffset, timeLastFrame, AudioFrequencyBand.High).Left ?? 1f;
        }

        float GetPositiveOrZero(float value)
        {
            if (value < 0)
            {
                return 0;
            }
            return 0.05f;
        }

        // Handle Playback Stopped Event
        void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            Debug.WriteLine("Playback Stopped");
            // Restart Audio if it has finished
            audio?.StartFromBeginning();
            audio?.waveOut.Play();
        }



        public static string LoadShaderSource(string resourcePath)
        {
            string shaderSource = "";
            string filePath = GetResourcePath(resourcePath);
            try
            {
                Debug.Write($"Try to load resource \"{filePath}\"... ");
                using (StreamReader reader = new StreamReader(filePath))
                {
                    shaderSource = reader.ReadToEnd();
                }
                Debug.WriteLine($"Done!");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error while loading resource: {e.Message}");
            }

            return shaderSource;
        }

        public static string GetResourcePath(string resourcePath)
        {
            string resourceDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/";
            string filePath = resourceDirectory + resourcePath;
            return filePath;
        }

        void KeyboardHandler()
        {
            if (KeyboardState.IsKeyPressed(Keys.Escape)) // Escape -> Close
            {
                Debug.WriteLine("Closing...");
                Close();
            }

            if (KeyboardState.IsKeyPressed(Keys.R)) // R -> Reload
            {
                Debug.WriteLine("Reloading...");
                OnUnload();
                OnLoad();
            }

            if (KeyboardState.IsKeyPressed(Keys.Space)) // Space -> Play/Pause
            {
                if (audio != null)
                {
                    switch (audio.waveOut.PlaybackState)
                    {
                        case PlaybackState.Playing:
                            Debug.WriteLine("Pausing Playback");
                            audio.waveOut.Pause();
                            break;
                        case PlaybackState.Paused:
                            Debug.WriteLine("Resuming Playback");
                            audio.waveOut.Play();
                            break;
                        default:
                            Debug.WriteLine("Starting Playback");
                            audio.waveOut.Play();
                            break;
                    }
                    ToggleStopwatch();
                }
            }

            if (KeyboardState.IsKeyPressed(Keys.S)) // toggle enableSampleDebugOutput
            {
                if (audio != null)
                {
                    audio.EnableSampleOutput = !audio.EnableSampleOutput;
                }
            }
        }

        //Toggle Stopwatch
        void ToggleStopwatch()
        {
            if (playTime.IsRunning)
            {
                playTime.Stop();
            }
            else
            {
                playTime.Start();
            }
        }
    }

    enum Locations
    {
        TimeStamp = 0,
        Resolution = 1,
        LowSample = 2,
        MidSample = 3,
        HighSample = 4

    }

    struct Features
    {
        public bool Audio;
    }
}