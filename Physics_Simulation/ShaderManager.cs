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
    public static class ShaderManager
    {
        #region private_members

        private static List<ShaderProgram> shaders;

        private struct ShaderExtentionTableItem
        {
            public ShaderExtentionTableItem(string extention, int shader_type)
            {
                this.extention   = extention;
                this.shader_type = shader_type;
            }

            public readonly string extention;
            public readonly int    shader_type; 
        };

        private static ShaderExtentionTableItem[] shaderExtentionTable = new ShaderExtentionTableItem[]
        {
            new ShaderExtentionTableItem(".vert", Gl.GL_VERTEX_SHADER       ),
            new ShaderExtentionTableItem(".frag", Gl.GL_FRAGMENT_SHADER     ),
            new ShaderExtentionTableItem(".geom", Gl.GL_GEOMETRY_SHADER_EXT )
        };

        private static Shader create_shader(string shader_file)
        {
            FileStream fs = new FileStream(shader_file, FileMode.Open);

            StreamReader sr = new StreamReader(fs);

            Shader shader = new Shader(-1, "", "");

            foreach (var shaderExtention in shaderExtentionTable)
                if (shader_file.Contains(shaderExtention.extention))
                {
                    shader.id   = Gl.glCreateShader(shaderExtention.shader_type);
                    shader.name = shader_file;
                    shader.source = sr.ReadToEnd();

                    break;
                }
                       
            return shader;
        }

        private static void initialize_shader(Shader shader)
        {
            string[] shaderSource = new string[1];
            shaderSource[0] = shader.source;

            Gl.glShaderSource(shader.id, 1, shaderSource, null);

            Gl.glCompileShader(shader.id);

            int isCompiled;
            Gl.glGetShaderiv(shader.id,Gl.GL_COMPILE_STATUS, out isCompiled);
            if (isCompiled == 0)
            {
                int len;
                StringBuilder log = new StringBuilder(256);
                Gl.glGetShaderInfoLog(shader.id, 256, out len, log);
                MessageBox.Show("Shader " + shader.name + " has not been successfully compiled. Code: \n"+ shaderSource + "\n error log: \n" + log.ToString());
            }
        }

        private static ShaderProgram create_shader_program(string program_directory)
        {
            List<Shader> shaders_in_program = new List<Shader>();

            List<string> shader_files = new List<string>();

            foreach (var shaderExtention in shaderExtentionTable)
                shader_files.AddRange(Directory.GetFiles(program_directory, "*"+shaderExtention.extention));

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

        public static void init()
        {
            if (Directory.Exists("Shaders"))
            {
                shaders = new List<ShaderProgram>();

                foreach (var subDirectory in Directory.GetDirectories("Shaders"))
                {
                    if (Directory.GetFiles(subDirectory).Length != 0)
                        shaders.Add(create_shader_program(subDirectory));
                }
            }
            else
                error = true;
        }

        public struct Shader
        {
            public Shader(int id, string name, string source)
            {
                this.id     = id;
                this.name   = name;
                this.source = source;
            }

            public int          id;
            public string       name;
            public string       source;
        }

        public struct ShaderProgram
        {
            public int    id;
            public string name;
        }

        public static bool error { get; private set; } = false;

        public static ShaderProgram getShader(string shaderName)
        {
            return shaders.Find(shader => shader.name == shaderName);
        }

        public static string[] getShadersList()
        {
            List<string> list = new List<string>();

            foreach (var shader in shaders)
                list.Add(shader.name);

            return list.ToArray();
        }

        #endregion
    }
}
