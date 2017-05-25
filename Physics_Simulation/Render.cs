using System.Collections.Generic;
using Tao.OpenGl;
using Tao.FreeGlut;
using Tao.Platform.Windows;
using Tao.DevIl;
using System;
using System.Windows.Forms;
using System.Drawing;

namespace Physics_Simulation
{
    public enum DIRECTION
    {
        NONE,
        UP,
        DOWN,
        LEFT,
        RIGHT,
        FORWARD,
        BACKWARD
    }

    public struct Point3D
    {
        #region public members

        public Point3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double x;
        public double y;
        public double z;

        #endregion
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

        private static void initializeComponents()
        {
            graphics      = null;
            drawingTimer  = new Timer();
            initialized   = false;
            camera        = new Camera();
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

            string cubemap_face = (textures_directory + cubemap_face_name + ".bmp");

            return loadTexture(cubemap_face);
        }

        private static void drawCubemapFace(int cubemap_face, DIRECTION direction)
        {
            Gl.glEnable(Gl.GL_TEXTURE_2D);

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, cubemap_face);

            Gl.glPushMatrix();

            Gl.glScaled(1000,1000,1000);
          
            switch (direction)
            {
                case DIRECTION.RIGHT:
                    Gl.glRotated(90, 0, 1, 0);
                    break;
                case DIRECTION.LEFT:
                    Gl.glRotated(-90, 0, 1, 0);
                    break;
                case DIRECTION.UP:
                    Gl.glRotated(90, 1, 0, 0);
                    break;
                case DIRECTION.DOWN:
                    Gl.glRotated(-90, 1, 0, 0);
                    break;
                case DIRECTION.FORWARD:
                    Gl.glTranslated(0, 0, -2);
                    break;
                case DIRECTION.BACKWARD:
                    break;
                default : break;
            }

            Gl.glBegin(Gl.GL_QUADS);

            Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(-1.0f, -1.0f, 1.0f);
            Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f( 1.0f, -1.0f, 1.0f);
            Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f( 1.0f,  1.0f, 1.0f);
            Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(-1.0f,  1.0f, 1.0f);

            Gl.glEnd();

            Gl.glPopMatrix();

            Gl.glDisable(Gl.GL_TEXTURE_2D);
        }

        private static void drawCubemap()
        {
            if (cubemap.right  != -1 && cubemap.left != -1 && cubemap.top   != -1 &&
                cubemap.bottom != -1 && cubemap.back != -1 && cubemap.front != -1)
            {
                drawCubemapFace(cubemap.right,  DIRECTION.RIGHT);
                drawCubemapFace(cubemap.left,   DIRECTION.LEFT);
                drawCubemapFace(cubemap.top,    DIRECTION.UP);
                drawCubemapFace(cubemap.bottom, DIRECTION.DOWN);
                drawCubemapFace(cubemap.back,   DIRECTION.BACKWARD);
                drawCubemapFace(cubemap.front,  DIRECTION.FORWARD);
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
            Gl.glScaled(0.2, 0.2, 0.2);
            Gl.glTranslated(0, 2700, 0);
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

            /* TODO: debug, delete */

            Gl.glPushMatrix();

            Gl.glEnable(Gl.GL_LIGHT0);
            Gl.glTranslated(0, 0, -2);
            float angle = ((float)DateTime.Now.Millisecond / (float)1000) * 180;
            Gl.glRotated(angle, 0, 1, 0);
            setColor(Color.DarkSeaGreen);
            Gl.glScaled(0.6, 0.6, 0.6);
            Glut.glutSolidSphere(1, 50, 50);
            Gl.glScaled(1.001, 1.001, 1.001);
            Gl.glDisable(Gl.GL_LIGHT0);
            setColor(Color.Black);
            Glut.glutWireSphere(1, 50, 50);

            Gl.glPopMatrix();

            /* END TEST*/

            //foreach (var obj in physic_objects)
            //    obj.calculate_physics();

            //foreach (var obj in physic_objects)
            //    obj.draw();

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
            private string _message;

            #endregion

            #region public_members

            public void setDefaultConfiguration()
            {
                _FPS = 100;

                _backgroundColor = Color.Gray;
                float r = (float)_backgroundColor.R / 256;
                float g = (float)_backgroundColor.G / 256;
                float b = (float)_backgroundColor.B / 256;
                Gl.glClearColor(r, g, b, 1);

                _backgroundCubemapImage = "Textures/Cubemap";

                _message = "FPS: " + FPS.ToString();
            }

            public Color backgroundColor
            {
                get
                {
                    return _backgroundColor;
                }
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
                get
                {
                    return _FPS;
                }
                set
                {
                    const int MILLISEC_IN_SEC = 1000;
                    drawingTimer.Interval = MILLISEC_IN_SEC / value;
                    _FPS = value;
                }
            }

            public string backgroundCubemapImage
            {
                get
                {
                    return _backgroundCubemapImage;
                }
                set
                {
                    try
                    {
                        cubemap = new Cubemap(-1, -1, -1, -1, -1, -1);
                    }
                    catch (Exception)
                    {
                        
                    }
                    _backgroundCubemapImage = value;
                }
            }

            public string message
            {
                get
                {
                    return _message;
                }
                set
                {
                    _message = value;
                }
            }

            #endregion
        }

        public static bool init(ref SimpleOpenGlControl canvas)
        {
            if (!initialized)
            {
                initializeComponents();

                graphics = canvas;
                graphics.InitializeContexts();

                Glut.glutInit();
                Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
                Il.ilInit();
                Il.ilEnable(Il.IL_ORIGIN_SET);
                Gl.glViewport(0, 0, graphics.Width, graphics.Height);
                Gl.glMatrixMode(Gl.GL_PROJECTION);
                Gl.glLoadIdentity();
                Glu.gluPerspective(45, (float)graphics.Width / (float)graphics.Height, 0.1, 20000);
                Gl.glMatrixMode(Gl.GL_MODELVIEW);
                Gl.glLoadIdentity();
                Gl.glEnable(Gl.GL_DEPTH_TEST);
                Gl.glEnable(Gl.GL_LIGHTING);
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

        #endregion
    }
}