using Tao.OpenGl;
using System.Windows.Forms;
using System.Drawing;
using System;

namespace Physics_Simulation
{
    public class Camera
    {
        #region private_members

        private Point3D   eye_position;
        private Point3D   center_position;
        private Point3D   up_position;
        private Rectangle cameraWindow;

        private void mouseEvent(MouseEventType mouseEvent, MouseEventArgs mouseArgs)
        {
            Point     cursorPos = screenCursorPosToWindow();
            Rectangle borders   = screenCameraWindowToLocal();
            
            switch (mouseEvent)
            {
                case MouseEventType.ENTER:
                    Cursor.Hide();
                    Cursor.Clip = cameraWindow;
                    break;
                case MouseEventType.LEAVE:
                    Cursor.Show();
                    break;
                case MouseEventType.MOVE:
                    mouseReverse(cursorPos, borders);

                    double alpha = mapCursorCoordsToAngle(cursorPos.X, borders.Right, Math.PI * 2);
                    double beta = mapCursorCoordsToAngle(cursorPos.Y, borders.Bottom, Math.PI);

                    double x = eye_position.x + Math.Cos(alpha);
                    double y = eye_position.y + Math.Cos(beta);
                    double z = eye_position.z + Math.Sin(alpha);

                    center_position = new Point3D(x, y, z);
             
                    break;
                default : break;
            }
            
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

        private Point screenCursorPosToWindow()
        {
            int x = Cursor.Position.X - cameraWindow.X;
            int y = Cursor.Position.Y - cameraWindow.Y;

            return new Point(x,y);
        }

        private Rectangle screenCameraWindowToLocal()
        {
            return new Rectangle(Point.Empty, new Size(cameraWindow.Size.Width-1, cameraWindow.Size.Height-1));
        }

        private void mouseReverse(Point cursorPos, Rectangle borders)
        {
            if (cursorPos.X == borders.Right)
                Cursor.Position = new Point(cameraWindow.Left + borders.Left, Cursor.Position.Y);
            else if (cursorPos.X == borders.Left)
                Cursor.Position = new Point(cameraWindow.Left + borders.Right, Cursor.Position.Y);
        }

        private double mapCursorCoordsToAngle(int coord, int coord_max, double maxAngle)
        {
            double percentage = (double)coord / (double)coord_max;

            return maxAngle * percentage;
        }

        #endregion

        #region public_members

        public Camera(Rectangle cameraWindow)
        {
            this.cameraWindow = cameraWindow;

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
