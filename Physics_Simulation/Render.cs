using System.Collections.Generic;
using Tao.OpenGl;
using Tao.FreeGlut;
using Tao.Platform.Windows;
using Tao.DevIl;
using System;
using System.Windows.Forms;

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

        private static SimpleOpenGlControl graphics      = null;
        private static Timer               drawingTimer  = new Timer();
        private static bool                initialized   = false;
        private static Camera              camera        = new Camera();

        private static int i = 0; // FIXME: delete

        private static void drawAll(object sender, EventArgs e)
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            Gl.glLoadIdentity();

            /* TEST */
            camera.renderCamera();

            Gl.glColor3d(0.1f, 0.7f, 0.1f);

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

            private int _FPS;
 
            #endregion

            #region public_members

            public void setDefaultConfiguration()
            {
                _FPS = 60;
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

            #endregion
        }

        public static void init(ref SimpleOpenGlControl canvas)
        {
            if (!initialized)
            {
                graphics = canvas;
                graphics.InitializeContexts();
                Glut.glutInit();
                Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
                Gl.glClearColor(0.5f, 0.5f, 0.5f, 1);
                Gl.glViewport(0, 0, graphics.Width, graphics.Height);
                Gl.glMatrixMode(Gl.GL_PROJECTION);
                Gl.glLoadIdentity();
                Glu.gluPerspective(45, (float)graphics.Width / (float)graphics.Height, 0.1, 200);
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
        }

        #endregion
    }
}