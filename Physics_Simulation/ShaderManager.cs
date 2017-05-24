using System.Collections.Generic;
using Tao.OpenGl;
using Tao.FreeGlut;
using Tao.Platform.Windows;
using Tao.DevIl;
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Physics_Simulation
{
    public class ShaderManager
    {
        #region private_members

        private Shader create_shader(string shader_file)
        {
            FileStream fs = new FileStream(shader_file, FileMode.Open);

            StreamReader sr = new StreamReader(fs);

            Shader shader = new Shader(-1, "", new List<string>());

            if (shader_file.Contains("vertex") || shader_file.Contains("Vertex"))
            {
                shader.id   = Gl.glCreateShader(Gl.GL_VERTEX_SHADER);
                shader.name = shader_file;
                while (!sr.EndOfStream)
                    shader.source.Add(sr.ReadLine() + "\n");
            }
            else if (shader_file.Contains("fragment") || shader_file.Contains("Fragment"))
            {
                shader.id   = Gl.glCreateShader(Gl.GL_FRAGMENT_SHADER);
                shader.name = shader_file;
                while (!sr.EndOfStream)
                    shader.source.Add(sr.ReadLine() + "\n");
            }

            return shader;
        }

        private void initialize_shader(Shader shader)
        {
            int length = shader.source.Count;
            Gl.glShaderSource(shader.id, 1, shader.source.ToArray(), ref length);
            Gl.glCompileShader(shader.id);

            /* FIXME: debug, delete*/
            int isCompiled;
            Gl.glGetShaderiv(shader.id,Gl.GL_COMPILE_STATUS, out isCompiled);
            if (isCompiled == 0)
            {
                int len;
                StringBuilder log = new StringBuilder(256);
                Gl.glGetShaderInfoLog(shader.id, 256, out len, log);
                string source = "";
                foreach (var line in shader.source.ToArray())
                    source += line;
                MessageBox.Show("Shader " + shader.name + " has not been successfully compiled. Code: \n"+ source + "\n error log: \n" + log.ToString());
            }
            /******/
        }

        private ShaderProgram create_shader_program(string program_directory)
        {
            List<Shader> shaders_in_program = new List<Shader>();

            List<string> shader_files = new List<string>();

            shader_files.AddRange(Directory.GetFiles(program_directory, "*.glsl"));

            foreach (var shader_file in shader_files)
                shaders_in_program.Add(create_shader(shader_file));

            ShaderProgram shaderProgram = new ShaderProgram();

            shaderProgram.id   = Gl.glCreateProgram();
            shaderProgram.name = program_directory;

            foreach (var shader in shaders_in_program)
            {
                initialize_shader(shader);
                Gl.glAttachShader(shaderProgram.id, shader.id);
            }

            Gl.glLinkProgram(shaderProgram.id);
            
            int link_ok;
            Gl.glGetProgramiv(shaderProgram.id, Gl.GL_LINK_STATUS, out link_ok);

            if (link_ok == 0)
            {
                int maxLength;
                int length;
                Gl.glGetProgramiv(shaderProgram.id, Gl.GL_INFO_LOG_LENGTH, out maxLength);
                StringBuilder log = new StringBuilder(256);
                Gl.glGetProgramInfoLog(shaderProgram.id, maxLength, out length, log);
                MessageBox.Show("Shader build error: " + log.ToString(), "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                error = true;
            }

            return shaderProgram;
        }

        #endregion

        #region public_members

        public ShaderManager()
        {
            if (Directory.Exists("Shaders"))
            {
                shaders = new List<ShaderProgram>();

                foreach (var directory in Directory.GetDirectories("Shaders"))
                    shaders.Add(create_shader_program(directory));
            }
            else
                error = true;
        }

        public List<ShaderProgram> shaders { get; private set; }

        public struct Shader
        {
            public Shader(int id, string name, List<string> source)
            {
                this.id     = id;
                this.name   = name;
                this.source = source;
            }

            public int          id;
            public string       name;
            public List<string> source;
        }

        public struct ShaderProgram
        {
            public int    id;
            public string name;
        }

        public bool error { get; private set; } = false;

        #endregion
    }
}
