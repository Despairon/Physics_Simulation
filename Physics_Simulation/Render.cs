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

        private static void initializeComponents()
        {
            graphics     = null;
            drawingTimer = new Timer();
            initialized  = false;
            camera       = new Camera();
        }

        private static void drawBackground()
        {
            const int cubeSize = 15000;

            setColor(Color.DarkSlateGray);

            Glut.glutSolidCube(cubeSize);
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

        private static int i = 0; // FIXME: delete

        private static void drawAll(object sender, EventArgs e)
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            Gl.glLoadIdentity();

            drawBackground();

            camera.renderCamera();

            renderText();

            /* TEST */

            setColor(Color.DarkSeaGreen);

            Gl.glPushMatrix();

            Gl.glTranslated(0, 0, -2);
            Gl.glRotated(i++, 0, 1, 0);
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
                _FPS = 60;

                _backgroundColor = Color.Gray;
                float r = (float)_backgroundColor.R / 256;
                float g = (float)_backgroundColor.G / 256;
                float b = (float)_backgroundColor.B / 256;
                Gl.glClearColor(r, g, b, 1);

                _backgroundCubemapImage = "Textures/Cubemap_stars.jpg";

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
                        // TODO: implement background set
                    }
                    catch (Exception)
                    {
                        // TODO: implement default behavior
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
                Gl.glViewport(0, 0, graphics.Width, graphics.Height);
                Gl.glMatrixMode(Gl.GL_PROJECTION);
                Gl.glLoadIdentity();
                Glu.gluPerspective(45, (float)graphics.Width / (float)graphics.Height, 0.1, 20000);
                Gl.glMatrixMode(Gl.GL_MODELVIEW);
                Gl.glLoadIdentity();
                Gl.glEnable(Gl.GL_DEPTH_TEST);
                Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

                userConfiguration.setDefaultConfiguration();

                drawingTimer.Interval = 1000 / userConfiguration.FPS;
                drawingTimer.Tick += new EventHandler(drawAll);
                drawingTimer.Start();

                initialized = true;
            }
            else return false;

            return true;
        }

        public static void setColor(Color color)
        {
            float r = (float)color.R / 256;
            float g = (float)color.G / 256;
            float b = (float)color.B / 256;
            Gl.glColor3d(r, g, b);
        }

        #endregion
    }
}