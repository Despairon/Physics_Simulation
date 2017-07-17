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
            READONG_MESH,
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
            new FSM_Table_item(Local_State.READONG_MESH,      "g",      FSM_Callbacks.read_mesh),
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
                var vertex_str = objFile.getLine();
                // TODO: implement
            }

            public static void read_texcoord(ObjFile objFile)
            {
                var texcoord_str = objFile.getLine();
                // TODO: implement
            }

            public static void read_normal(ObjFile objFile)
            {
                var normal_str = objFile.getLine();
                // TODO: implement
            }

            public static void read_mesh(ObjFile objFile)
            {
                var group_str = objFile.getLine();
                if (group_str != "")
                { }
                // TODO: implement
            }

            public static void read_face(ObjFile objFile)
            {
                var face_str = objFile.getLine();
                // TODO: implement
            }

            public static void read_smoothing(ObjFile objFile)
            {
                var smoothing_str = objFile.getLine();
                // TODO: implement
            }

            public static void read_mtllib(ObjFile objFile)
            {
                var matlib_str = objFile.getLine();
                // TODO: implement
            }

            public static void read_usemtl(ObjFile objFile)
            {
                var usemtl_str = objFile.getLine();
                // TODO: implement
            }
        }
       
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
                        }
                    }
                }

                changeGlobalState(Global_State.IDLE);
            }

            return objFile;
        }

        public class ObjFile
        {
            public ObjFile(string name, string source)
            {
                this.name   = name;
                this.source = source;
                index = 0;
            }

            private int index;

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
                  
            // TODO: implement
        }
    }
}
