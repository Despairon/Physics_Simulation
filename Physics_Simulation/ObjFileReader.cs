using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physics_Simulation
{
    public static class ObjFileReader
    {
        #region private_members

        private enum Global_State
        {
            IDLE,
            IN_PROCESS
        }

        private enum Local_State
        {
            IDLE = 0,
            READING_COMMENT,
            READING_VERTEX,
            READING_TEXCOORD,
            READING_NORMAL,
            READING_MESH,
            READING_FACE,
            READING_SMOOTHING,
            READING_MTLLIB,
            READING_USEMTL
        }

        private struct FSM_Table_item
        {
            public FSM_Table_item(Local_State nextState, string cmd, func cmd_func)
            {
                this.nextState = nextState;
                this.cmd       = cmd;
                this.cmd_func  = cmd_func;
            }
            
            public Local_State nextState;
            public string cmd;
            public func   cmd_func;

            public delegate void func(ObjFile objFile);
        }

        private static List<FSM_Table_item> fsm_table = new List<FSM_Table_item>()
        {
            new FSM_Table_item(Local_State.READING_COMMENT,   "#",      FSM_Callbacks.read_commend),
            new FSM_Table_item(Local_State.READING_VERTEX,    "v",      FSM_Callbacks.read_vertex),
            new FSM_Table_item(Local_State.READING_TEXCOORD,  "vt",     FSM_Callbacks.read_texcoord),
            new FSM_Table_item(Local_State.READING_NORMAL,    "vn",     FSM_Callbacks.read_normal),
            new FSM_Table_item(Local_State.READING_MESH,      "g",      FSM_Callbacks.read_mesh),
            new FSM_Table_item(Local_State.READING_MESH,      "o",      FSM_Callbacks.read_mesh),
            new FSM_Table_item(Local_State.READING_FACE,      "f",      FSM_Callbacks.read_face),
            new FSM_Table_item(Local_State.READING_SMOOTHING, "s",      FSM_Callbacks.read_smoothing),
            new FSM_Table_item(Local_State.READING_MTLLIB,    "mtllib", FSM_Callbacks.read_mtllib),
            new FSM_Table_item(Local_State.READING_USEMTL,    "usemtl", FSM_Callbacks.read_usemtl)
        };

        private static Global_State   _gState = Global_State.IDLE;
        private static Local_State    _lState = Local_State.IDLE;

        private static void changeGlobalState(Global_State gState)
        {
            _gState = gState;
        }

        private static void changeLocalState(Local_State lState)
        {
            _lState = lState;
        }

        private static void execute_fsm(ObjFile objFile)
        {
            string cmd = objFile.nextCmd();

            var fsm_table_item = fsm_table.Find(item => item.cmd == cmd);

            if (fsm_table_item.cmd_func != null)
            {
                changeLocalState(fsm_table_item.nextState);
                fsm_table_item.cmd_func(objFile);
            }
        }

        private struct FSM_Callbacks
        {
            public static void read_commend(ObjFile objFile)
            {
                objFile.skipLine();
            }

            public static void read_vertex(ObjFile objFile)
            {
                var vertex_str = new List<string>(objFile.getLine().Split(' '));

                vertex_str.RemoveAll(str => (str == "") || (str == " "));

                if ((vertex_str.Count == 3) || (vertex_str.Count == 4 && vertex_str[3] != ""))
                {
                    Vector3 vertex = new Vector3();
                    vertex.x = double.Parse(vertex_str[0], CultureInfo.InvariantCulture);
                    vertex.y = double.Parse(vertex_str[1], CultureInfo.InvariantCulture);
                    vertex.z = double.Parse(vertex_str[2], CultureInfo.InvariantCulture);

                    objFile.vertices.Add(vertex);
                }
            }

            public static void read_texcoord(ObjFile objFile)
            {
                var texcoord_str = new List<string>(objFile.getLine().Split(' '));

                texcoord_str.RemoveAll(str => (str == "") || (str == " "));

                if (texcoord_str.Count == 2 || (texcoord_str.Count == 3 && texcoord_str[2] != ""))
                {
                    Vector2 texcoord = new Vector2();
                    texcoord.x = double.Parse(texcoord_str[0], CultureInfo.InvariantCulture);
                    texcoord.y = double.Parse(texcoord_str[1], CultureInfo.InvariantCulture);

                    objFile.texcoords.Add(texcoord);
                }
            }

            public static void read_normal(ObjFile objFile)
            {
                var normal_str = new List<string>(objFile.getLine().Split(' '));

                normal_str.RemoveAll(str => (str == "") || (str == " "));

                if (normal_str.Count == 3 || (normal_str.Count == 4 && normal_str[3] != ""))
                {
                    Vector3 normal = new Vector3();
                    normal.x = double.Parse(normal_str[0], CultureInfo.InvariantCulture);
                    normal.y = double.Parse(normal_str[1], CultureInfo.InvariantCulture);
                    normal.z = double.Parse(normal_str[2], CultureInfo.InvariantCulture);

                    objFile.normals.Add(normal);
                }
            }

            public static void read_mesh(ObjFile objFile)
            {
                var group_str = objFile.getLine();
                if (group_str != "")
                {
                    var mesh = new ObjFile.Mesh(group_str);

                    objFile.addMesh(mesh);
                }
            }

            public static void read_face(ObjFile objFile)
            {
                var face_str = objFile.getLine();

                var mesh = objFile.currMesh;

                var face_str_splitted = new List<string>(face_str.Split(' '));

                face_str_splitted.RemoveAll(str => (str == "") || (str == " "));

                List<int> vertex_indices   = new List<int>();
                List<int> texcoord_indices = new List<int>();
                List<int> normal_indices   = new List<int>();

                foreach (var str in face_str_splitted)
                {
                    var str_splitted = str.Split('/');

                    int vertex_index   = 0;
                    int texcoord_index = 0;
                    int normal_index   = 0;

                    switch (str_splitted.Length)
                    {
                        case 1:
                            {
                                vertex_index = Convert.ToInt32(str_splitted[0]);
                                if (vertex_index < 0)
                                    vertex_index = (objFile.vertices.Count - (vertex_index * -1) + 1);
                            }
                            break;
                        case 2:
                            {
                                vertex_index = Convert.ToInt32(str_splitted[0]);
                                if (vertex_index < 0)
                                    vertex_index = (objFile.vertices.Count - (vertex_index * -1) + 1);

                                texcoord_index = Convert.ToInt32(str_splitted[1]);
                                if (texcoord_index < 0)
                                    texcoord_index = (objFile.texcoords.Count - (texcoord_index * -1) + 1);
                            }
                            break;
                        case 3:
                            {
                                vertex_index = Convert.ToInt32(str_splitted[0]);
                                if (vertex_index < 0)
                                    vertex_index = (objFile.vertices.Count - (vertex_index * -1) + 1);

                                if (str_splitted[1] != "")
                                {
                                    texcoord_index = Convert.ToInt32(str_splitted[1]);
                                    if (texcoord_index < 0)
                                        texcoord_index = (objFile.texcoords.Count - (texcoord_index * -1) + 1);
                                }

                                normal_index = Convert.ToInt32(str_splitted[2]);
                                if (normal_index < 0)
                                    normal_index = (objFile.normals.Count - (normal_index * -1) + 1);
                            }
                            break;

                        default: break;
                    }

                    if (vertex_index != 0)
                        vertex_indices.Add(vertex_index);

                    if (texcoord_index != 0)
                        texcoord_indices.Add(texcoord_index);

                    if (normal_index != 0)
                        normal_indices.Add(normal_index);
                }

                var face = new ObjFile.Mesh.Face(vertex_indices.ToArray(), texcoord_indices.ToArray(), normal_indices.ToArray());

                mesh.faces.Add(face); 
            }

            public static void read_smoothing(ObjFile objFile)
            {
                var smoothing_str = objFile.getLine();
                // TODO: implement
            }

            public static void read_mtllib(ObjFile objFile)
            {
                var matlib_str = objFile.getLine();

                objFile.setMatLib(matlib_str);
            }

            public static void read_usemtl(ObjFile objFile)
            {
                var usemtl_str = objFile.getLine();
                // TODO: implement
            }
        }

        #endregion

        #region public_members 

        public static ObjFile read(string filename)
        {
            ObjFile objFile = null;

            if (_gState == Global_State.IDLE)
            {
                changeGlobalState(Global_State.IN_PROCESS);

                FileStream fs = new FileStream(filename, FileMode.Open);

                if (fs != null)
                {
                    if (fs.CanRead)
                    {
                        StreamReader sr = new StreamReader(fs);
                        if (sr != null)
                        {
                            string source = sr.ReadToEnd();
                            objFile = new ObjFile(filename, source);

                            while (!objFile.eof)
                                execute_fsm(objFile);

                            sr.Close();
                        }
                    }

                    fs.Close();
                }

                changeGlobalState(Global_State.IDLE);
            }

            return objFile;
        }

        public class ObjFile
        {
            #region private_members

            private int index;

            #endregion

            #region public_members

            public ObjFile(string name, string source)
            {
                this.name   = name;
                this.source = source;

                vertices  = new List<Vector3>();
                texcoords = new List<Vector2>();
                normals   = new List<Vector3>();

                meshes    = new List<Mesh>();

                index = 0;
            }

            public string name   { get; private set; }
            public string source { get; private set; }

            public bool eof
            {
                get { return index == source.Length; }
            }

            public string nextCmd()
            {
                string cmd = "";

                while (true)
                {
                    cmd += source[index];
                    index++;

                    if (index == source.Length) break;
                    if (source[index] == '\n')  break;
                    if (source[index] == '\0')  break;
                    if (source[index] == ' ')   break;
                }

                return cmd.Trim();
            }

            public void skipLine()
            {
                while ((source[index] != '\n') && (source[index] != '\0'))
                    index++;
            }

            public string getLine()
            {
                string line = "";

                while ((source[index] != '\n') && (source[index] != '\0'))
                {
                    line += source[index];
                    index++;
                }

                var trimmedLine = line.TrimStart().TrimEnd();
                return trimmedLine;
            }

            public List<Vector3> vertices  { get; private set; }
            public List<Vector2> texcoords { get; private set; }
            public List<Vector3> normals   { get; private set; }
                  
            public string matlib { get; private set; }

            private bool matlibSet = false;
            public void setMatLib(string matlib)
            {
                if (!matlibSet)
                {
                    this.matlib = matlib;
                    matlibSet = true;
                }
            }

            public class Mesh
            {
                private string name;

                public Mesh(string name)
                {
                    this.name  = name;

                    faces = new List<Face>();
                }

                public struct Face
                {
                    public Face(int[] vertex_indices, int[] texcoord_indices, int[] normal_indices)
                    {
                        this.vertex_indices   = vertex_indices;
                        this.texcoord_indices = texcoord_indices;
                        this.normal_indices   = normal_indices;
                    }

                    public int[] vertex_indices;
                    public int[] texcoord_indices;
                    public int[] normal_indices;
                }

                public List<Face> faces { get; private set; }
            }

            public List<Mesh> meshes    { get; private set; }

            public Mesh currMesh        { get; private set; }

            public void addMesh(Mesh mesh)
            {
                meshes.Add(mesh);
                currMesh = mesh;
            }
        }

        #endregion

        #endregion
    }
}
