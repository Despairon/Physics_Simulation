using System;
using System.Drawing;
using System.Windows.Forms;

namespace Physics_Simulation
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        public InputManager inputManager { get; private set; }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            inputManager = new InputManager(renderWindow as Control);

            if (!Render.init(ref renderWindow))
                MessageBox.Show("already initialized or error occured");

            var obj = RenderObject.getPreloadedObject("Objects\\NewTieFighter.obj");
            Render.instantiateObject(obj);
        }
    }
}
