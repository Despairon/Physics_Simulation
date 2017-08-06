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
            var cubemap_shader = ShaderManager.getShader("Cubemap");
            var default_shader = ShaderManager.getShader("Default");

            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            //cubemap_shader.use();

            //cubemap_shader.unuse();

            renderText();

            //foreach (var obj in objects.FindAll(_obj => _obj is PhysicalObject))
            //    for (uint i = 0; i < userConfiguration.physicsIterations; i++)
            //        (obj as PhysicalObject).calculate_physics();

            int proj_uni = default_shader.getUniform("projection");
            int view_uni = default_shader.getUniform("view");

            foreach (var obj in objects)
            {
                obj.applyShader(default_shader);

                obj.prepare();

                Gl.glUniformMatrix4fv(proj_uni, 1, 1, getProjectionMatrix());

                Gl.glUniformMatrix4fv(view_uni, 1, 1, getModelViewMatrix());

                obj.applyTransformations();

                obj.draw();
                
                obj.complete();
            }
            
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

                    camera = new Camera(new Rectangle(graphics.PointToScreen(Point.Empty), graphics.Size), (Math.PI / 4) * 1.25);

                    userConfiguration = new UserConfiguration();

                    Glut.glutInit();
                    Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
                    Il.ilInit();
                    Il.ilEnable(Il.IL_ORIGIN_SET);
                    Gl.glViewport(0, 0, graphics.Width, graphics.Height);
                    Gl.glEnable(Gl.GL_DEPTH_TEST);
                    Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

                    userConfiguration.readCfgFromFile();

                    drawingTimer.Interval = 1000 / userConfiguration.FPS;
                    drawingTimer.Tick += new EventHandler(drawAll);
                    drawingTimer.Start();

                    if (!initializeShaders())
                        throw new Exception();

                    RenderObject.preloadObjects();

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
            graphics.ParentForm.WindowState     = graphics.ParentForm.WindowState == FormWindowState.Normal ? FormWindowState.Maximized : FormWindowState.Normal;
            graphics.ParentForm.FormBorderStyle = graphics.ParentForm.FormBorderStyle == FormBorderStyle.FixedDialog ? FormBorderStyle.None : FormBorderStyle.FixedDialog;
            Gl.glViewport(0, 0, graphics.Width, graphics.Height);
            camera.updateViewRatio(new Rectangle(graphics.PointToScreen(Point.Empty), graphics.Size), (Math.PI / 4) * 1.25);
        }

        public static void test(double x, double y, double z, double rx, double ry, double rz, double sx, double sy, double sz)
        {
            /*************************TODO: START TEST*******************************/
            System.Collections.Generic.List<Vector3> vertices = new System.Collections.Generic.List<Vector3>();

            Random rnd = new Random(DateTime.Now.Millisecond);
            int r = (int)(rnd.NextDouble() * 0xFF);
            int g = (int)(rnd.NextDouble() * 0xFF);
            int b = (int)(rnd.NextDouble() * 0xFF);

            Color c = Color.FromArgb(r,g,b);

            var obj = RenderObject.getPreloadedObject("Objects\\sphere.obj");

            obj.scale(sx, sy, sz);
            obj.translate(x,y,z);
            obj.rotate(rx, ry, rz);

            instantiateObject(obj);

            /**********************END TEST***********************/
        }

        #region engine_public_methods

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

        public static float[] getModelViewMatrix()
        {
            return camera.getView().toFloat();
        }

        public static float[] getProjectionMatrix()
        {
            return camera.getProjection().toFloat();
        }

        public static Vector3 getMainCameraLocation()
        {
            return camera.getPosition();
        }

        public static Vector3 getMainCameraDirection()
        {
            return camera.getDirection();
        }

        #endregion

        #endregion
    }
}