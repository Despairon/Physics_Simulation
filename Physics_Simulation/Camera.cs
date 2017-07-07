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
       
        private Vector3 position;
        private Vector3 lookAt;
        private Vector3 up;

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
            new MovingStateMapItem( Direction.FORWARD,  Keys.W,          false ),
            new MovingStateMapItem( Direction.BACKWARD, Keys.S,          false ),
            new MovingStateMapItem( Direction.LEFT,     Keys.A,          false ),
            new MovingStateMapItem( Direction.RIGHT,    Keys.D,          false ),
            new MovingStateMapItem( Direction.UP,       Keys.Space,      false ),
            new MovingStateMapItem( Direction.DOWN,     Keys.ControlKey, false ),
            new MovingStateMapItem( Direction.NONE,     Keys.ShiftKey,   false)
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

                    double x_rotation = -(mapCursorCoordsToAngle(cursorPos.Y, borders.Bottom, Math.PI) - (Math.PI/2));
                    double y_rotation = mapCursorCoordsToAngle(cursorPos.X, borders.Right,  Math.PI * 2);

                    lookAt.x = Math.Cos(x_rotation) * Math.Cos(y_rotation);
                    lookAt.y = Math.Sin(x_rotation);
                    lookAt.z = Math.Cos(x_rotation) * Math.Sin(y_rotation);
                    lookAt.normalize();

                    break;
                case MouseEventType.CLICK:
                    Render.test(position.x + lookAt.x, position.y + lookAt.y, position.z + lookAt.z, 0,0,0, 0.3,0.3,0.3); // TODO: 4 fun, delete
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
                double speed = Render.userConfiguration.cameraSpeed / (double)Render.userConfiguration.FPS;

                switch (state.direction)
                {
                    case Direction.FORWARD:
                        position += speed * lookAt;
                        break;
                    case Direction.BACKWARD:
                        position -= speed * lookAt;
                        break;
                    case Direction.LEFT:
                        var left = (lookAt * up);
                        left.normalize();
                        left *= -1;
                        position += left * speed;
                        break;
                    case Direction.RIGHT:
                        var right = (lookAt * up);
                        right.normalize();
                        position += right * speed;
                        break;
                    case Direction.UP:
                        position.y += speed;
                        break;
                    case Direction.DOWN:
                        position.y -= speed;
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

            position = new Vector3(0,0,0);
            lookAt   = new Vector3(0,0,1);
            up       = new Vector3(0,1,0);

            initMovingTimer();
        }

        public void renderCamera()
        {
            Glu.gluLookAt(position.x, position.y, position.z,
                          position.x + lookAt.x, position.y + lookAt.y, position.z + lookAt.z,
                          up.x, up.y, up.z);
        }

        public Vector3 getPosition()
        {
            return position;
        }

        #endregion
    }
}
