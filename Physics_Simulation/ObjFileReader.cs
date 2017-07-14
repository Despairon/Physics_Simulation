using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Simulation
{
    public static class ObjFileReader
    {
        private struct ObjFileCommandTableItem
        {
            public ObjFileCommandTableItem(string command, command_func cmd_func)
            {
                this.command = command;
                this.cmd_func = cmd_func;
            }

            public delegate void   command_func(ObjFile objFile);

            public readonly string       command;
            public readonly command_func cmd_func;
        }

        private static ObjFileCommandTableItem[] cmd_table = new ObjFileCommandTableItem[]
        {
            new ObjFileCommandTableItem("#",      comment_cmd_handler   ),
            new ObjFileCommandTableItem("mtllib", matlib_cmd_handler    ),
            new ObjFileCommandTableItem("usemtl", usemtl_cmd_handler    ),
            new ObjFileCommandTableItem("v",      vertex_cmd_handler    ),
            new ObjFileCommandTableItem("vt",     texcoord_cmd_handler  ),
            new ObjFileCommandTableItem("vn",     normal_cmd_handler    ),
            new ObjFileCommandTableItem("vp",     parameter_cmd_handler ),
            new ObjFileCommandTableItem("f",      face_cmd_handler      ),
            new ObjFileCommandTableItem("o",      object_cmd_handler    ),
            new ObjFileCommandTableItem("g",      group_cmd_handler     ),
            new ObjFileCommandTableItem("s",      smooth_cmd_handler    )
        };

        private static void comment_cmd_handler(ObjFile objFile)
        {
            sReader.ReadLine();
        }

        private static void matlib_cmd_handler(ObjFile objFile)
        {
            string matlib = sReader.ReadLine();
            objFile.setMatLib(matlib);
        }

        private static void usemtl_cmd_handler(ObjFile objFile)
        {
            // TODO: IMPLEMENT!
        }

        private static void vertex_cmd_handler(ObjFile objFile)
        {
            // TODO: IMPLEMENT!
        }

        private static void texcoord_cmd_handler(ObjFile objFile)
        {
            // TODO: IMPLEMENT!
        }

        private static void normal_cmd_handler(ObjFile objFile)
        {
            // TODO: IMPLEMENT!
        }

        private static void parameter_cmd_handler(ObjFile objFile)
        {
            comment_cmd_handler(objFile);
            // TODO: not implemented
        }

        private static void face_cmd_handler(ObjFile objFile)
        {
            // TODO: IMPLEMENT!
        }

        private static void object_cmd_handler(ObjFile objFile)
        {
            // TODO: IMPLEMENT!
        }

        private static void group_cmd_handler(ObjFile objFile)
        {
            // TODO: IMPLEMENT!
        }

        private static void smooth_cmd_handler(ObjFile objFile)
        {
            comment_cmd_handler(objFile);
            // TODO: not implemented
        }

        private static FileStream   fStream;
        private static StreamReader sReader;

        public static ObjFile read(string filename)
        {
            if (!filename.Contains(".obj"))
                return null;
            else
            {
                try
                {
                    ObjFile objFile = new ObjFile(filename);

                    fStream = new FileStream(filename, FileMode.Open);
                    sReader = new StreamReader(fStream);

                    while (!sReader.EndOfStream)
                    {
                        string command = "";
                        char ch = '\0';

                        while (ch != ' ')
                        {
                            ch = (char)sReader.Read();
                            command += ch;                    
                        }

                        command = command.Trim();

                        var ctrl = Array.Find(cmd_table, cmd => cmd.command == command);

                        ctrl.cmd_func(objFile);
                    }

                    fStream.Close();
                    sReader.Close();

                    return objFile;
                }
                catch(Exception)
                {
                    return null;
                }
            }
        }
    }

    public class ObjFile
    {
        public ObjFile(string name)
        {
            this.name = name;

            vertices  = new List<Vector3>();
            texcoords = new List<Vector2>();
            normals   = new List<Vector3>();

            matlib = "";
        }

        public readonly string name;

        public List<Vector3> vertices  { get; private set; }
        public List<Vector2> texcoords { get; private set; }
        public List<Vector3> normals   { get; private set; }

        public string matlib           { get; private set; }

        public void setMatLib(string matlib)
        {
            if (!matLibSet)
            {
                this.matlib = matlib;
                matLibSet = true;
            }
        }

        private bool matLibSet = false;
    }
}
