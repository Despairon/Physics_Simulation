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

        private static readonly Vector3 EYE_ORIGIN = new Vector3(0,0,0);

        private double  x_rotation, y_rotation;
        private Vector3 position;

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

                    x_rotation = mapCursorCoordsToAngle(cursorPos.Y, borders.Bottom, HALF_CIRCLE) - (HALF_CIRCLE / 2);
                    y_rotation = mapCursorCoordsToAngle(cursorPos.X, borders.Right, FULL_CIRCLE);

                    /* TODO: something with this
                    alpha = mapCursorCoordsToAngle(cursorPos.X, borders.Right, Math.PI * 2);
                    beta = mapCursorCoordsToAngle(cursorPos.Y, borders.Bottom, Math.PI);
                    Vector3 lookAt = ExtendedMath.getPointOnCircle(position, 1, alpha, beta);
                     */
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
                    switch (key_args.KeyChar)
                    {
                        case (char)Keys.Enter:
                            Render.changeWindowState();
                            break;
                        default:
                            break;
                    }
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
            foreach (var state in movingStateMap.FindAll(state => state.isMoving))
            {
                // TODO: implement moving here !!!

                double speed = Render.userConfiguration.cameraSpeed / (double)Render.userConfiguration.FPS;

                switch (state.direction)
                {
                    case Direction.FORWARD:
                        position.z += speed;
                        break;
                    case Direction.BACKWARD:
                        position.z -= speed;
                        break;
                    case Direction.LEFT:
                        position.x += speed;
                        break;
                    case Direction.RIGHT:
                        position.x -= speed;
                        break;

                    default:
                        break;
                }

                
            }

            Render.userConfiguration.message = "position: x:" + position.x.ToString() + " y:" + position.y.ToString() + " z:" + position.z.ToString() + "\n";
        }

        #endregion

        #region public_members

        public Camera(Rectangle window)
        {
            // TODO: need to fix this on window move
            cameraWindow = window;

            attachInput();

            x_rotation = 0;
            y_rotation = 0;

            position = EYE_ORIGIN;

            initMovingTimer();
        }

        public void renderCamera()
        {
            // TODO: do something about it

            Gl.glRotated(x_rotation, 1, 0, 0);
            Gl.glRotated(y_rotation, 0, 1, 0);

            Gl.glTranslated(position.x, position.y, position.z);
        }

        #endregion
    }
}
