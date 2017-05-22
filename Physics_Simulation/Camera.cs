using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using Tao.FreeGlut;
using Tao.Platform.Windows;
using Tao.DevIl;
using System.Windows.Forms;

namespace Physics_Simulation
{
    public class Camera
    {
        #region private_members

        private Point3D eye_position;
        private Point3D center_position;
        private Point3D up_position;

        private void mouseEvent(MouseEventType mouseEvent, MouseEventArgs mouseArgs)
        {

        }

        private void attachMouseInput()
        {
            Program.getInputManagerFromMainForm().mouseEvent += mouseEvent;
        }

        #endregion

        #region public_members
        
        public Camera()
        {
            eye_position    = new Point3D(0,0,1);
            center_position = new Point3D(0,0,0);
            up_position     = new Point3D(0,1,0);

            attachMouseInput();
        }

        public void renderCamera()
        {     
            // TODO: implement camera rotate
            Glu.gluLookAt(eye_position.x,    eye_position.y,    eye_position.z,
                          center_position.x, center_position.y, center_position.z,
                          up_position.x,     up_position.y,     up_position.z);
        }

        #endregion
    }
}
