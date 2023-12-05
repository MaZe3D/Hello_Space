using System.Reflection;
using System.Diagnostics;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;

namespace Hello_Space
{
    internal class Game : GameWindow
    {
        // set of vertices to draw the triangle with (x,y,z) for each vertex
        List<Vector2> vertices = new()
        {
            // front face
            new Vector2(-1f, 1f),  // topleft vert
            new Vector2(1f,  1f),  // topright vert
            new Vector2(-1f, -1f), // bottomleft vert
            new Vector2(1f,  -1f)  // bottomright vert
        };

        /* List<Vector2> texCoordinates = new()
        {
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0f),
            new Vector2(0f, 0f)
        }; */

        // Render Pipeline vars
        int vao;
        int shaderProgram;
        int vbo;

        DateTime runTime;
        TimeSpan TimeStamp {
            get
            {
                return DateTime.Now - runTime;
            }
        }

        // width and height of screen
        int width, height;
        // Constructor that sets the width, height, and calls the base constructor (GameWindow's Constructor) with default args
        public Game(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.width = width;
            this.height = height;

            // center window
            CenterWindow(new Vector2i(width, height));
        }
        // called whenever window is resized
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
            this.width = e.Width;
            this.height = e.Height;
        }

        // called once when game is started
        protected override void OnLoad()
        {
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

            // unbind the vbo and vao respectively
            // GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // GL.BindVertexArray(0);

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

            runTime = DateTime.Now;
        }
        // called once when game is closed
        protected override void OnUnload()
        {
            base.OnUnload();

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

            GL.Uniform1((int)Locations.TimeStamp, TimeStamp.Milliseconds);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            // swap the buffers
            Context.SwapBuffers();

            base.OnRenderFrame(args);
        }
        // called every frame. All updating happens here
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
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
    }

    enum Locations
    {
        TimeStamp = 0
    }
}