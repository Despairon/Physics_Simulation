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

                    const double HALF_CIRCLE = 180;
                    const double FULL_CIRCLE = HALF_CIRCLE * 2;

                    double alpha = mapCursorCoordsToAngle(cursorPos.X, borders.Right, FULL_CIRCLE);
                    double beta  = mapCursorCoordsToAngle(cursorPos.Y, borders.Bottom, HALF_CIRCLE);

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
            Vector3 direction = ExtendedMath.translated_vector(eye_position, lookAt_position, Render.userConfiguration.cameraSpeed + 1);

            foreach (var state in movingStateMap.FindAll(state => state.isMoving))
            {

                // TODO: implementation is INCOMPLETE and nearly WRONG!!! COMPLETE IT!!!
                switch (state.direction)
                {
                    case Direction.FORWARD:
                        break;
                    case Direction.BACKWARD:
                        direction *= ExtendedMath.rotated_vector(direction, eye_position, 180, 0);
                        break;
                    case Direction.LEFT:
                        direction *= ExtendedMath.rotated_vector(direction, eye_position, -90, 0);
                        break;
                    case Direction.RIGHT:
                        direction *= ExtendedMath.rotated_vector(direction, eye_position, 90, 0);
                        break;

                    default:
                        break;
                }

                double speed    = Render.userConfiguration.cameraSpeed / (double)Render.userConfiguration.FPS;
                eye_position    = ExtendedMath.translated_vector(eye_position,    direction, speed);
                lookAt_position = ExtendedMath.translated_vector(lookAt_position, direction, speed);
            }

            Render.userConfiguration.message = "eye: x:" + eye_position.x.ToString() + " y:" + eye_position.y.ToString() + " z:" + eye_position.z.ToString() + "\n";
            Render.userConfiguration.message += "lookAt: x:" + lookAt_position.x.ToString() + " y:" + lookAt_position.y.ToString() + " z:" + lookAt_position.z.ToString() + "\n";
            Render.userConfiguration.message += "Direction: x:" + direction.x.ToString() + " y:" + direction.y.ToString() + " z:" + direction.z.ToString();
        }

        #endregion

        #region public_members

        public Camera(Rectangle cameraWindow)
        {
            this.cameraWindow = cameraWindow;

            eye_position    = new Vector3(0,0,5);
            lookAt_position = new Vector3(0,0,0);
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

        #endregion
    }
}
