using System.Collections.Generic;
using Tao.OpenGl;
using Tao.FreeGlut;
using Tao.Platform.Windows;
using Tao.DevIl;
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace Physics_Simulation
{
    public enum Direction
    {
        NONE,
        UP,
        DOWN,
        LEFT,
        RIGHT,
        FORWARD,
        BACKWARD
    }
    
    public static class Render
    {
        #region private_members

        private static SimpleOpenGlControl graphics;
        private static Timer               drawingTimer;
        private static bool                initialized;
        private static Camera              camera;
        private static Cubemap             cubemap;
        private static List<RenderObject>  objects;

        private static void initializeComponents()
        {
            graphics      = null;
            drawingTimer  = new Timer();
            initialized   = false;
            objects       = new List<RenderObject>();
        }

        private static bool initializeShaders()
        {
            ShaderManager.init();
            if (ShaderManager.error)
                return false;
            return true;
        }

        public static void setColor(Color color)
        {
            float r = (float)color.R / 256;
            float g = (float)color.G / 256;
            float b = (float)color.B / 256;
            Gl.glColor3d(r, g, b);
        }

        private struct Cubemap
        {
            public Cubemap (int right, int left, int top, int bottom, int back, int front)
            {
                this.right  = loadCubemapFace("right");
                this.left   = loadCubemapFace("left");
                this.top    = loadCubemapFace("top");
                this.bottom = loadCubemapFace("bottom");
                this.back   = loadCubemapFace("back");
                this.front  = loadCubemapFace("front");
            }
            public int right;
            public int left;
            public int top;
            public int bottom;
            public int back;
            public int front;
        }

        private static int loadCubemapFace(string cubemap_face_name)
        {
            string textures_directory  = userConfiguration.backgroundCubemapImage + "/";

            string cubemap_face = (textures_directory + cubemap_face_name + userConfiguration.backGroundImageFormat);

            return loadTexture(cubemap_face);
        }

        private static void drawCubemapFace(int cubemap_face, Direction direction)
        {
            Gl.glEnable(Gl.GL_TEXTURE_2D);

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, cubemap_face);

            Gl.glPushMatrix();

            var cameraPos = camera.getPosition();

            Gl.glTranslated(cameraPos.x, cameraPos.y, cameraPos.z);

            Gl.glScaled(3000, 3000, 3000);

            switch (direction)
            {
                case Direction.RIGHT:
                    Gl.glTranslated(1,0,0);
                    Gl.glRotated(180, 0, 1, 0);
                    Gl.glRotated(90, 0, 1, 0);
                    break;
                case Direction.LEFT:
                    Gl.glTranslated(-1, 0, 0);
                    Gl.glRotated(180, 0, 1, 0);
                    Gl.glRotated(-90, 0, 1, 0);
                    break;
                case Direction.UP:
                    Gl.glTranslated(0, 1, 0);
                    Gl.glRotated(90, 1, 0, 0);
                    break;
                case Direction.DOWN:
                    Gl.glTranslated(0, -1, 0);
                    Gl.glRotated(90, 1, 0, 0);
                    Gl.glRotated(180, 1,0, 0);
                    break;
                case Direction.FORWARD:
                    Gl.glTranslated(0, 0, -1);
                    break;
                case Direction.BACKWARD:
                    Gl.glTranslated(0, 0, 1);
                    Gl.glRotated(180, 0, 1, 0);
                    break;
                default : break;
            }

            Gl.glBegin(Gl.GL_QUADS);

            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(-1.0f, -1.0f, 0.0f);
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f( 1.0f, -1.0f, 0.0f);
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f( 1.0f,  1.0f, 0.0f);
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(-1.0f,  1.0f, 0.0f);

            Gl.glEnd();

            Gl.glPopMatrix();

            Gl.glDisable(Gl.GL_TEXTURE_2D);
        }

        private static void drawCubemap()
        {
            if (cubemap.right  != -1 && cubemap.left != -1 && cubemap.top   != -1 &&
                cubemap.bottom != -1 && cubemap.back != -1 && cubemap.front != -1)
            {
                drawCubemapFace(cubemap.right,  Direction.RIGHT);
                drawCubemapFace(cubemap.left,   Direction.LEFT);
                drawCubemapFace(cubemap.top,    Direction.UP);
                drawCubemapFace(cubemap.bottom, Direction.DOWN);
                drawCubemapFace(cubemap.back,   Direction.BACKWARD);
                drawCubemapFace(cubemap.front,  Direction.FORWARD);
            }
        }

        private static int makeTexture(int format, IntPtr data, int width, int height)
        {
            int texture_ID;

            Gl.glGenTextures(1, out texture_ID);

            Gl.glPixelStorei(Gl.GL_UNPACK_ALIGNMENT, 1);

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texture_ID);
            
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_REPLACE);

            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, format, width, height, 0, format, Gl.GL_UNSIGNED_BYTE, data);

            return texture_ID;
        }

        private static int loadTexture(string filepath)
        {
            int texture_ID;
            int image_ID;

            image_ID = Il.ilGenImage();

            Il.ilBindImage(image_ID);

            if (Il.ilLoadImage(filepath))
            {
                int width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
                int height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);

                int bitsPerPixel = Il.ilGetInteger(Il.IL_IMAGE_BITS_PER_PIXEL);
                
                switch (bitsPerPixel)
                {
                    case 24:
                        texture_ID = makeTexture(Gl.GL_RGB, Il.ilGetData(), width, height);
                        break;
                    case 32:
                        texture_ID = makeTexture(Gl.GL_RGBA, Il.ilGetData(), width, height);
                        break;
                    default:
                        texture_ID = -1;
                        break;
                }

                Il.ilDeleteImage(image_ID);

                return texture_ID;
            }
            else return -1;
        }

        private static void renderText()
        {
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Glu.gluOrtho2D(0, graphics.Width, 0, graphics.Height);

            Gl.glRasterPos2i(0, 0);
            setColor(Color.Red);
            Gl.glTranslated(0,graphics.Height - 25,0);
            Gl.glScaled(0.2, 0.2, 0.2);
            Glut.glutStrokeString(Glut.GLUT_STROKE_ROMAN, userConfiguration.message);
            
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPopMatrix();
        }

        private static void drawAll(object sender, EventArgs e)
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            Gl.glLoadIdentity();

            camera.renderCamera();

            drawCubemap();

            renderText();

            /*****************************TODO: debug, delete*************************************/

            var shader = ShaderManager.getShader("Cubemap");

            if (shader.id != -1)
            {
                Gl.glUseProgram(shader.id);

                if (shader.id != -1)
                    Gl.glUseProgram(0);

            }

            /****************************END TEST*************************************/

            //foreach (var obj in objects.FindAll(_obj => _obj is PhysicalObject))
            //    for (uint i = 0; i < userConfiguration.physicsIterations; i++)
            //        (obj as PhysicalObject).calculate_physics();

            foreach (var obj in objects)
                obj.draw();
            
            Gl.glFlush();

            graphics.Invalidate();
        }

        #endregion


        #region public_members

        public static UserConfiguration userConfiguration;

        public class UserConfiguration
        {
            #region private_members

            private class Cfg_param_table_item
            {
                public Cfg_param_table_item(string name, object value, Type type, Configuration_parameter param)
                {
                    this.name  = name;
                    this.value = value;
                    this.type  = type;
                    this.param = param;
                }

                public string                  name;
                public object                  value;
                public Type                    type;
                public Configuration_parameter param;
            }

            private List<Cfg_param_table_item> cfg_param_table;

            private object getParam(Configuration_parameter param)
            {
                return cfg_param_table.Find(p => p.param == param).value;
            }

            private void setParam(object value, Configuration_parameter param)
            {
                var p = cfg_param_table.Find(_p => _p.param == param);

                if (p != null)
                    p.value = value;
            }

            private void readParam(string line)
            {
                for (int i = 0; i < cfg_param_table.Count; i++)
                {
                    if (line.Contains(cfg_param_table[i].name))
                    {
                        string varStr = line.Substring(line.IndexOf('=')).Trim('=').TrimStart().TrimEnd();

                        object convertedVar = Convert.ChangeType(varStr, cfg_param_table[i].type);

                        cfg_param_table[i].value = convertedVar;

                        break;
                    }
                }
            }

            private void setDefaultConfiguration()
            {
                FPS = 100;

                cameraSpeed = 4;

                backgroundColor = Color.Black;

                backGroundImageFormat = ".jpg";
                backgroundCubemapImage = "Textures/Cubemap/earth";


                // TODO: switch _message and set to appropriate string
                message = "FPS: " + FPS.ToString();

                physicsIterations = 1;
            }

            #endregion

            #region public_members

            public enum Configuration_parameter
            {
                CFG_FPS,
                CFG_BACKGROUND_COLOR,
                CFG_BACKGROUND_CUBEMAP_IMAGE,
                CFG_BACKGROUND_IMAGE_FORMAT,
                CFG_MESSAGE,
                CFG_CAMERA_SPEED,
                CFG_PHYSICS_ITERATIONS
            }

            public UserConfiguration()
            {
                cfg_param_table = new List<Cfg_param_table_item>()
                {
                    new Cfg_param_table_item( "FPS"                   , new object() , typeof(int)    , Configuration_parameter.CFG_FPS                      ),
                    new Cfg_param_table_item( "backgroundColor"       , new object() , typeof(int)    , Configuration_parameter.CFG_BACKGROUND_COLOR         ),
                    new Cfg_param_table_item( "backgroundCubemapImage", new object() , typeof(string) , Configuration_parameter.CFG_BACKGROUND_CUBEMAP_IMAGE ),
                    new Cfg_param_table_item( "backGroundImageFormat" , new object() , typeof(string) , Configuration_parameter.CFG_BACKGROUND_IMAGE_FORMAT  ),
                    new Cfg_param_table_item( "message"               , new object() , typeof(string) , Configuration_parameter.CFG_MESSAGE                  ),
                    new Cfg_param_table_item( "cameraSpeed"           , new object() , typeof(double) , Configuration_parameter.CFG_CAMERA_SPEED             ),
                    new Cfg_param_table_item( "physicsIterations"     , new object() , typeof(uint)   , Configuration_parameter.CFG_PHYSICS_ITERATIONS       )
                };
            }

            public void readCfgFromFile()
            {
                const string CFG_FILENAME = "config.cfg";
                const string CFG_VARIABLE_PREFIX = "var ";

                var fs = new FileStream(CFG_FILENAME, FileMode.Open);
                var sr = new StreamReader(fs);

                try
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();

                        if (line.Contains(CFG_VARIABLE_PREFIX))
                            readParam(line);
                    }
                }
                catch (Exception)
                {
                    setDefaultConfiguration();
                }

                sr.Close();
                fs.Close();

                FPS                    = (int)                getParam(Configuration_parameter.CFG_FPS);
                backgroundColor        = Color.FromArgb((int) getParam(Configuration_parameter.CFG_BACKGROUND_COLOR));
                backgroundCubemapImage = (string)             getParam(Configuration_parameter.CFG_BACKGROUND_CUBEMAP_IMAGE);
                backGroundImageFormat  = (string)             getParam(Configuration_parameter.CFG_BACKGROUND_IMAGE_FORMAT);
                message                = (string)             getParam(Configuration_parameter.CFG_MESSAGE);
                cameraSpeed            = (double)             getParam(Configuration_parameter.CFG_CAMERA_SPEED);
                physicsIterations      = (uint)               getParam(Configuration_parameter.CFG_PHYSICS_ITERATIONS);

                // add setters here to apply configuration
            }

            public Color backgroundColor
            {
                get
                {
                    var bgColor = (int)getParam(Configuration_parameter.CFG_BACKGROUND_COLOR);
                    return Color.FromArgb(bgColor);
                }
                set
                {
                    float r = (float)value.R / 256;
                    float g = (float)value.G / 256;
                    float b = (float)value.B / 256;
                    Gl.glClearColor(r, g, b, 1);
                    setParam(value.ToArgb(),Configuration_parameter.CFG_BACKGROUND_COLOR);
                }
            }

            public int FPS
            {
                get { return (int)getParam(Configuration_parameter.CFG_FPS); }
                set
                {
                    const int MILLISEC_IN_SEC = 1000;
                    drawingTimer.Interval = MILLISEC_IN_SEC / value;
                    setParam(value, Configuration_parameter.CFG_FPS);
                }
            }

            public string backgroundCubemapImage
            {
                get { return (string)getParam(Configuration_parameter.CFG_BACKGROUND_CUBEMAP_IMAGE); }
                set
                {
                    try
                    {
                        setParam(value, Configuration_parameter.CFG_BACKGROUND_CUBEMAP_IMAGE);
                        cubemap = new Cubemap(-1, -1, -1, -1, -1, -1);
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            public string backGroundImageFormat
            {
                get { return (string)getParam(Configuration_parameter.CFG_BACKGROUND_IMAGE_FORMAT); }
                set { setParam(value, Configuration_parameter.CFG_BACKGROUND_IMAGE_FORMAT); }
            }

            public string message
            {
                get { return (string)getParam(Configuration_parameter.CFG_MESSAGE); }
                set { setParam(value, Configuration_parameter.CFG_MESSAGE); }
            }

            public double cameraSpeed
            {
                get { return (double)getParam(Configuration_parameter.CFG_CAMERA_SPEED); }
                set { setParam(value, Configuration_parameter.CFG_CAMERA_SPEED); }
            }

            public uint physicsIterations
            {
                get { return (uint)getParam(Configuration_parameter.CFG_PHYSICS_ITERATIONS); }
                set { setParam(value, Configuration_parameter.CFG_PHYSICS_ITERATIONS); }
            }

            #endregion
        }

        public static bool init(ref SimpleOpenGlControl canvas)
        {
            try
            {
                if (!initialized)
                {
                    initializeComponents();

                    graphics = canvas;
                    graphics.InitializeContexts();

                    camera = new Camera(new Rectangle(graphics.PointToScreen(Point.Empty), graphics.Size));

                    userConfiguration = new UserConfiguration();

                    Glut.glutInit();
                    Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
                    Il.ilInit();
                    Il.ilEnable(Il.IL_ORIGIN_SET);
                    Gl.glViewport(0, 0, graphics.Width, graphics.Height);
                    Gl.glMatrixMode(Gl.GL_PROJECTION);
                    Gl.glLoadIdentity();
                    Glu.gluPerspective(60, (float)graphics.Width / (float)graphics.Height, 0.1, 10000);
                    Gl.glMatrixMode(Gl.GL_MODELVIEW);
                    Gl.glLoadIdentity();
                    Gl.glEnable(Gl.GL_DEPTH_TEST);
                    Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

                    userConfiguration.readCfgFromFile();
                    
                    drawingTimer.Interval = 1000 / userConfiguration.FPS;
                    drawingTimer.Tick += new EventHandler(drawAll);
                    drawingTimer.Start();

                    if (!initializeShaders())
                        throw new Exception();

                    initialized = true;
                }
                else return false;

                return true;
            }
            catch (Exception)
            {
                drawingTimer.Stop();
                return false;
            }
        }

        public static void changeWindowState()
        {
            graphics.ParentForm.WindowState = graphics.ParentForm.WindowState == FormWindowState.Normal ? FormWindowState.Maximized : FormWindowState.Normal;
            graphics.ParentForm.FormBorderStyle = graphics.ParentForm.FormBorderStyle == FormBorderStyle.FixedDialog ? FormBorderStyle.None : FormBorderStyle.FixedDialog;
            Gl.glViewport(0, 0, graphics.Width, graphics.Height);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluPerspective(60, (float)graphics.Width / (float)graphics.Height, 0.1, 20000);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
        }

        public static void instantiateObject(RenderObject obj)
        {
            objects.Add(obj);
        }

        public static RenderObject getObjectByName(string name)
        {
            return objects.Find(obj => obj.name == name);
        }

        public static void deleteObject(RenderObject obj)
        {
            if (objects.Contains(obj))
                objects.Remove(obj);
        }

        public static void test(double x, double y, double z, double rx, double ry, double rz, double sx, double sy, double sz)
        {
            /*************************TODO: START TEST*******************************/
            System.Collections.Generic.List<Vector3> vertices = new System.Collections.Generic.List<Vector3>();

            vertices.Add(new Vector3(0, 0, 0));
            vertices.Add(new Vector3(-1, -1, 1));
            vertices.Add(new Vector3(-1, -1, -1));

            vertices.Add(new Vector3(0, 0, 0));
            vertices.Add(new Vector3(-1, -1, -1));
            vertices.Add(new Vector3(1, -1, -1));
                    
            vertices.Add(new Vector3(0, 0, 0));
            vertices.Add(new Vector3(1, -1, -1));
            vertices.Add(new Vector3(1, -1, 1));

            vertices.Add(new Vector3(0, 0, 0));
            vertices.Add(new Vector3(-1, -1, 1));
            vertices.Add(new Vector3(1, -1, 1));

            vertices.Add(new Vector3(-1, -1, 1));
            vertices.Add(new Vector3(1, -1, -1));
            vertices.Add(new Vector3(1, -1, 1));

            vertices.Add(new Vector3(1, -1, -1));
            vertices.Add(new Vector3(-1, -1, -1));
            vertices.Add(new Vector3(-1, -1, 1));

            Random rnd = new Random(DateTime.Now.Millisecond);
            int r = (int)(rnd.NextDouble() * 0xFF);
            int g = (int)(rnd.NextDouble() * 0xFF);
            int b = (int)(rnd.NextDouble() * 0xFF);

            Color c = Color.FromArgb(r,g,b);

            RenderObject obj = new RenderObject("triangle", vertices, null, null, c, RenderObject.Primitives_type.TRIANGLES);
            obj.scale(sx, sy, sz);
            obj.translate(x,y,z);
            obj.rotate(rx, ry, rz);
            instantiateObject(obj);

            /**********************END TEST***********************/
        }

        #endregion
    }
}