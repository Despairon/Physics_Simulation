using System.Windows.Forms;

namespace Physics_Simulation
{
    public enum MouseEventType
    {
        CLICK,
        DOUBLE_CLICK,
        DOWN,
        MOVE,
        UP,
        WHEEL
    }

    public enum KeyEventType
    {
        DOWN,
        UP,
        PRESSED
    }

    public class InputManager
    {
        #region private_members

        /***********************default "do nothing" handle section****************************/
        private static void defaultInputEvent(MouseEventType mouseEvent, MouseEventArgs mouseArgs)  { }
        private static void defaultInputEvent(KeyEventType mouseEvent, KeyEventArgs mouseArgs)      { }
        private static void defaultInputEvent(KeyEventType mouseEvent, KeyPressEventArgs mouseArgs) { }

        /*******************************mouse handle section***********************************/
        private void mouseClickHandler(object sender, MouseEventArgs mouse_args)
        {
            mouseEvent.Invoke(MouseEventType.CLICK, mouse_args);
        }

        private void mouseDoubleClickHandler(object sender, MouseEventArgs mouse_args)
        {
            mouseEvent.Invoke(MouseEventType.DOUBLE_CLICK, mouse_args);
        }

        private void mouseDownHandler(object sender, MouseEventArgs mouse_args)
        {
            mouseEvent.Invoke(MouseEventType.DOWN, mouse_args);
        }

        private void mouseMoveHandler(object sender, MouseEventArgs mouse_args)
        {
            mouseEvent.Invoke(MouseEventType.MOVE, mouse_args);
        }

        private void mouseUpHandler(object sender, MouseEventArgs mouse_args)
        {
            mouseEvent.Invoke(MouseEventType.UP, mouse_args);
        }

        private void mouseWheelHandler(object sender, MouseEventArgs mouse_args)
        {
            mouseEvent.Invoke(MouseEventType.WHEEL, mouse_args);
        }
        /**************************************************************************************/


        /******************************keyboard handle section*********************************/
        private void keyDownEventHandler(object sender, KeyEventArgs keyb_args)
        {
            keyDownUpEvent.Invoke(KeyEventType.DOWN, keyb_args);
        }

        private void keyUpEventHandler(object sender, KeyEventArgs keyb_args)
        {
            keyDownUpEvent.Invoke(KeyEventType.UP, keyb_args);
        }

        private void keyPressEventHandler(object sender, KeyPressEventArgs keyb_args)
        {
            keyPressEvent.Invoke(KeyEventType.PRESSED, keyb_args);
        }
        /**************************************************************************************/

        #endregion

        #region public_members

        public InputManager(Control ctrl)
        {
            if (ctrl != null)
            { 
                // mouse handle section
                ctrl.MouseClick       += mouseClickHandler;
                ctrl.MouseDoubleClick += mouseDoubleClickHandler;
                ctrl.MouseDown        += mouseDownHandler;
                ctrl.MouseMove        += mouseMoveHandler;
                ctrl.MouseUp          += mouseUpHandler;
                ctrl.MouseWheel       += mouseWheelHandler;

                // keyboard handle section
                ctrl.KeyDown  += keyDownEventHandler;
                ctrl.KeyPress += keyPressEventHandler;
                ctrl.KeyUp    += keyUpEventHandler;
            }
        }

        public delegate void MouseEvent(MouseEventType mouseEvent, MouseEventArgs mouseArgs);
        public delegate void KeyDownUpEvent(KeyEventType keyEvent, KeyEventArgs keyEventArgs);
        public delegate void KeyPressEvent(KeyEventType keyEvent, KeyPressEventArgs keyEventArgs);

        public MouseEvent     mouseEvent      = new MouseEvent(defaultInputEvent);
        public KeyDownUpEvent keyDownUpEvent  = new KeyDownUpEvent(defaultInputEvent);
        public KeyPressEvent  keyPressEvent   = new KeyPressEvent(defaultInputEvent);

        #endregion
    }
}
