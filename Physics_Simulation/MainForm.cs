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

            if (Render.init(ref renderWindow))
            {
                // set render configuration
                Render.userConfiguration.FPS                    = 100;
                Render.userConfiguration.backgroundColor        = Color.Gray;
                Render.userConfiguration.backGroundImageFormat  = ".jpg";
                Render.userConfiguration.backgroundCubemapImage = "Textures/Cubemap/earth";
                Render.userConfiguration.cameraSpeed            = 4;
            }
            else
                MessageBox.Show("already initialized or error occured");

            ObjFileReader.read("Objects/cessna.obj");
        }
    }
}
