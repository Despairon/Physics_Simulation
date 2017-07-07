using System.Collections.Generic;
using Tao.OpenGl;
using Tao.FreeGlut;
using Tao.Platform.Windows;
using Tao.DevIl;
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

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
        private static ShaderManager       shaderManager;
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
            shaderManager = new ShaderManager();
            if (shaderManager.error)
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

            var shader = shaderManager.getShader("Cubemap");
            if (shader.id != -1)
            {
                Gl.glUseProgram(shader.id);

                int coords_attribute = Gl.glGetAttribLocation(shader.id, "coords");
                int color_attribute  = Gl.glGetAttribLocation(shader.id, "color_seed_in");
                int matrix_uniform   = Gl.glGetUniformLocation(shader.id,  "view");

                int coords_buffer;
                int color_buffer;
                int index_buffer;

                float color_r = 0.3f;//(float)Math.Sin((double)DateTime.Now.Millisecond)   * 0.367f; // TODO: uncomment when shaders are working
                float color_g = 0.5f;//((float)Math.Sin((double)DateTime.Now.Millisecond)) * 0.760f;
                float color_b = 0.2f;//((float)Math.Sin((double)DateTime.Now.Millisecond)) * 1.500f;

                float[,] vertices = new float[,]
                {
                    { -1.0f, -1.0f, -1.0f },
                    {  1.0f, -1.0f, -1.0f },
                    {  1.0f,  1.0f, -1.0f },
                    { -1.0f,  1.0f, -1.0f }
                };

                float[,] colors = new float[,]
                {
                    { color_r, color_g, color_b },
                    { color_g, color_b, color_r },
                    { color_b, color_g, color_r },
                    { color_b, color_g, color_r }
                };

                int[] indices = new int[]
                {
                    0, 1, 3,
                    2, 3, 1
                };

                Gl.glGenBuffers(1, out coords_buffer);
                Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, coords_buffer);
                Gl.glBufferData(Gl.GL_ARRAY_BUFFER, new IntPtr(vertices.Length), vertices,Gl.GL_STATIC_DRAW);

                Gl.glGenBuffers(1, out color_buffer);
                Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, color_buffer);
                Gl.glBufferData(Gl.GL_ARRAY_BUFFER, new IntPtr(vertices.Length * sizeof(float) * 3), colors, Gl.GL_STATIC_DRAW);

                Gl.glGenBuffers(1, out index_buffer);
                Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, index_buffer);
                Gl.glBufferData(Gl.GL_ELEMENT_ARRAY_BUFFER, new IntPtr(indices.Length * sizeof(int)), indices , Gl.GL_STATIC_DRAW);
                
                Gl.glPushMatrix();

                float angle = (float)Stopwatch.GetTimestamp() / 100000.0f;
                Gl.glTranslated(0, 0, -3);
                Gl.glRotated(angle, 0, 1, 0);
                
                float[] view_matrix = new float[16];
                Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX,view_matrix);
                Gl.glUniformMatrix4fv(matrix_uniform,1,Gl.GL_FALSE,view_matrix);

                Gl.glPopMatrix();

                Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, index_buffer);
                
                Gl.glEnableVertexAttribArray(coords_attribute);
                Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, coords_buffer);
                Gl.glVertexAttribPointer(coords_attribute, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 0, IntPtr.Zero);

                Gl.glEnableVertexAttribArray(color_attribute);
                Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, color_buffer);
                Gl.glVertexAttribPointer(color_attribute, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 0, IntPtr.Zero);

                Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, index_buffer);
                
                // TODO: delete and uncomment below
                // Gl.glDrawElements(Gl.GL_TRIANGLES, indices.Length, Gl.GL_UNSIGNED_INT, IntPtr.Zero);

                Gl.glDisableVertexAttribArray(coords_attribute);

                Gl.glDisableVertexAttribArray(color_attribute);

                Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, 0);
                Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, 0);

                Gl.glDeleteBuffers(1, ref coords_buffer);
                Gl.glDeleteBuffers(1, ref color_buffer);
                Gl.glDeleteBuffers(1, ref index_buffer);

                if (shader.id != -1)
                    Gl.glUseProgram(0);

            }

            /****************************END TEST*************************************/

            //foreach (var obj in objects.FindAll(_obj => _obj is PhysicalObject))
            //    (obj as PhysicalObject).calculate_physics();

            foreach (var obj in objects)
                obj.draw();
            
            Gl.glFlush();

            graphics.Invalidate();
        }

        #endregion


        #region public_members

        public static UserConfiguration userConfiguration = new UserConfiguration();

        public struct UserConfiguration
        {
            #region private_members

            private int    _FPS;
            private Color  _backgroundColor;
            private string _backgroundCubemapImage;
            private string _backGroundImageFormat;
            private string _message;
            private double _cameraSpeed;

            #endregion

            #region public_members

            public void setDefaultConfiguration()
            {
                _FPS = 100;

                _cameraSpeed = 4;

                _backgroundColor = Color.Gray;
                float r = (float)_backgroundColor.R / 256;
                float g = (float)_backgroundColor.G / 256;
                float b = (float)_backgroundColor.B / 256;
                Gl.glClearColor(r, g, b, 1);

                _backgroundCubemapImage = "Textures/Cubemap/earth";
                _backGroundImageFormat  = ".jpg";

                _message = "FPS: " + FPS.ToString();
            }

            public Color backgroundColor
            {
                get { return _backgroundColor; }
                set
                {
                    float r = (float)value.R / 256;
                    float g = (float)value.G / 256;
                    float b = (float)value.B / 256;
                    Gl.glClearColor(r, g, b, 1);
                    _backgroundColor = value;
                }
            }

            public int FPS
            {
                get { return _FPS; }
                set
                {
                    const int MILLISEC_IN_SEC = 1000;
                    drawingTimer.Interval = MILLISEC_IN_SEC / value;
                    _FPS = value;
                }
            }

            public string backgroundCubemapImage
            {
                get { return _backgroundCubemapImage; }
                set
                {
                    try
                    {
                        _backgroundCubemapImage = value;
                        cubemap = new Cubemap(-1, -1, -1, -1, -1, -1);
                    }
                    catch (Exception)
                    {
                        
                    }
                }
            }

            public string backGroundImageFormat
            {
                get { return _backGroundImageFormat;  }
                set { _backGroundImageFormat = value; }
            }

            public string message
            {
                get { return _message;  }
                set { _message = value; }
            }

            public double cameraSpeed
            {
                get { return _cameraSpeed;  }
                set { _cameraSpeed = value; }
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

                    

                    userConfiguration.setDefaultConfiguration();

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
            // reset viewport and perspective
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
            System.Collections.Generic.List<Triangle> triangles = new System.Collections.Generic.List<Triangle>();

            triangles.Add
            (
                new Triangle
                (
                    new Vector3(0, 0, 0),
                    new Vector3(-1, -1, 1),
                    new Vector3(-1, -1, -1)
                )
            );

            triangles.Add
            (
                new Triangle
                (
                    new Vector3(0, 0, 0),
                    new Vector3(-1, -1, -1),
                    new Vector3(1, -1, -1)
                )
            );

            triangles.Add
            (
                new Triangle
                (
                    new Vector3(0, 0, 0),
                    new Vector3(1, -1, -1),
                    new Vector3(1, -1, 1)
                )
            );

            triangles.Add
            (
                new Triangle
                (
                    new Vector3(0, 0, 0),
                    new Vector3(-1, -1, 1),
                    new Vector3(1, -1, 1)
                )
            );

            triangles.Add
            (
                new Triangle
                (
                    new Vector3(-1, -1, 1),
                    new Vector3(1, -1, -1),
                    new Vector3(1, -1, 1)
                )
            );

            triangles.Add
            (
                new Triangle
                (
                    new Vector3(1, -1, -1),
                    new Vector3(-1, -1, -1),
                    new Vector3(-1, -1, 1)
                )
            );

            Random rnd = new Random(DateTime.Now.Millisecond);
            int r = (int)(rnd.NextDouble() * 0xFF);
            int g = (int)(rnd.NextDouble() * 0xFF);
            int b = (int)(rnd.NextDouble() * 0xFF);

            Color c = Color.FromArgb(r,g,b);

            RenderObject obj = new RenderObject("triangle", triangles, c);
            obj.scale(sx, sy, sz);
            obj.translate(x,y,z);
            obj.rotate(rx, ry, rz);
            instantiateObject(obj);

            /**********************END TEST***********************/
        }

        #endregion
    }
}