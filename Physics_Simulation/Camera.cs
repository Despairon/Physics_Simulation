using Tao.OpenGl;
using System.Windows.Forms;
using System.Drawing;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Physics_Simulation
{
    public class Camera
    {
        #region private_members

        private Vector3   eye_position;
        private Vector3   lookAt_position;
        private Vector3   up_position;

        private Rectangle cameraWindow;

        private Timer     movingTimer;

        private class MovingStateMapItem
        {
            #region public_members

            public readonly Direction direction;
            public readonly Keys      key;
            public bool     isMoving  { get; private set; }

            public MovingStateMapItem(Direction direction, Keys key, bool isMoving)
            {
                this.direction = direction;
                this.key       = key;
                this.isMoving  = isMoving;
            }

            public void changeState(bool newState)
            {
                isMoving = newState;
            }

            #endregion
        }

        private readonly List<MovingStateMapItem> movingStateMap = new List<MovingStateMapItem>()
        {
            new MovingStateMapItem( Direction.FORWARD,  Keys.W, false ),
            new MovingStateMapItem( Direction.BACKWARD, Keys.S, false ),
            new MovingStateMapItem( Direction.LEFT,     Keys.A, false ),
            new MovingStateMapItem( Direction.RIGHT,    Keys.D, false )
        };

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
                    double beta  = mapCursorCoordsToAngle(cursorPos.Y, borders.Bottom, Math.PI);
                    // TODO: do something with this
                    lookAt_position = ExtendedMath.rotated_vector(lookAt_position, eye_position, alpha, beta);

                    break;
                default : break;
            }          
        }

        private void keyDownUpEvent(KeyEventType keyEventType, KeyEventArgs key_args)
        {
            if (keyEventType == KeyEventType.DOWN)
            {
                MovingStateMapItem state = movingStateMap.Find(s => s.key == key_args.KeyCode);

                if (state != null)
                    state.changeState(true);
                
            }

            if (keyEventType == KeyEventType.UP)
            {
                MovingStateMapItem state = movingStateMap.Find(s => s.key == key_args.KeyCode);

                if (state != null)
                    state.changeState(false);

            }       
        }

        private void keyPressEvent(KeyEventType keyEventType, KeyPressEventArgs key_args)
        {
            switch (keyEventType)
            {
                case KeyEventType.PRESSED:
                    if (key_args.KeyChar == (char)Keys.Enter)
                        Render.changeWindowState();
                    break;
                default:
                    break;
            }
        }

        private void attachInput()
        {
            Program.getInputManagerFromMainForm().mouseEvent     += mouseEvent;
            Program.getInputManagerFromMainForm().keyDownUpEvent += keyDownUpEvent;
            Program.getInputManagerFromMainForm().keyPressEvent  += keyPressEvent;
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

        private void initMovingTimer()
        {
            movingTimer = new Timer();
            movingTimer.Enabled  = true;
            movingTimer.Interval = 1;
            movingTimer.Tick    += new EventHandler(cameraMove);
        }

        private void cameraMove(object sender, EventArgs args)
        {
            var direction = eye_position;
            direction.translateByDirection(lookAt_position, Render.userConfiguration.cameraSpeed+(lookAt_position-eye_position).getLength());

            foreach (var state in movingStateMap.FindAll(state => state.isMoving))
            {
                // TODO: implementation is INCOMPLETE and nearly WRONG!!! COMPLETE IT!!!
                switch (state.direction)
                {
                    case Direction.FORWARD:
                        break;
                    case Direction.BACKWARD:
                        direction.rotate(0, Math.PI, 0);
                        break;
                    case Direction.LEFT:
                        direction.rotate(0, Math.PI / 2.0f, 0);
                        break;
                    case Direction.RIGHT:
                        direction.rotate(0, Math.PI / -2.0f, 0);
                        break;

                    default:
                        break;
                }

                double speed = Render.userConfiguration.cameraSpeed / (double)Render.userConfiguration.FPS;
                eye_position.translateByDirection(direction, speed);
                lookAt_position.translateByDirection(direction, speed);
            }

            Render.userConfiguration.message =  "eye: x:"       + eye_position.x.ToString()    + " y:" + eye_position.y.ToString()    + " z:" + eye_position.z.ToString()    + "\n";
            Render.userConfiguration.message += "lookAt: x:"    + lookAt_position.x.ToString() + " y:" + lookAt_position.y.ToString() + " z:" + lookAt_position.z.ToString() + "\n";
            Render.userConfiguration.message += "Direction: x:" + direction.x.ToString()       + " y:" + direction.y.ToString()       + " z:" + direction.z.ToString();
            Render.userConfiguration.message += "\nLength: " + (lookAt_position - eye_position).getLength().ToString();
        }

        #endregion

        #region public_members

        public Camera()
        {
            eye_position    = new Vector3(0,0,5);
            lookAt_position = new Vector3(0,0,4);
            up_position     = new Vector3(0,1,0);

            attachInput();

            initMovingTimer();
        }

        public void renderCamera()
        {               
            Glu.gluLookAt(eye_position.x,    eye_position.y,    eye_position.z,
                          lookAt_position.x, lookAt_position.y, lookAt_position.z,
                          up_position.x,     up_position.y,     up_position.z);
        }

        public Vector3 getPosition()
        {
            return eye_position;
        }

        public void changeWindowPosition(Rectangle newPosition)
        {
            cameraWindow = newPosition;
        }

        #endregion
    }
}
