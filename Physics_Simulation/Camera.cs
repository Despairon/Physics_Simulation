using Tao.OpenGl;
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
            // TODO: implement camera rotation behavior
        }
        
        private void keyboardEvent(KeyEventType keyEventType, KeyEventArgs key_args)
        {
            // TODO: implement camera moving behavior
        }

        private void attachInput()
        {
            Program.getInputManagerFromMainForm().mouseEvent     += mouseEvent;
            Program.getInputManagerFromMainForm().keyDownUpEvent += keyboardEvent;
        }

        #endregion

        #region public_members
        
        public Camera()
        {
            eye_position    = new Point3D(0,0,1);
            center_position = new Point3D(0,0,0);
            up_position     = new Point3D(0,1,0);

            attachInput();
        }

        public void renderCamera()
        {               
            Glu.gluLookAt(eye_position.x,    eye_position.y,    eye_position.z,
                          center_position.x, center_position.y, center_position.z,
                          up_position.x,     up_position.y,     up_position.z);
        }

        #endregion
    }
}
