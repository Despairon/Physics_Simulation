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
                Render.userConfiguration.backgroundCubemapImage = "Textures/Cubemap";
                Render.userConfiguration.cameraSpeed            = 1;
                this.Move                                      += new EventHandler(Render.onWindowMove);
            }
            else
                MessageBox.Show("already initialized");

            /*************************TODO: START TEST*******************************/
            System.Collections.Generic.List<Triangle> triangles = new System.Collections.Generic.List<Triangle>();

            triangles.Add
            (
                new Triangle
                (
                    new Vector3( 0,  0, 0),
                    new Vector3(-1, -1, 1),
                    new Vector3(-1, -1,-1)
                )
            );

            triangles.Add
            (
                new Triangle
                (
                    new Vector3( 0,  0,  0),
                    new Vector3(-1, -1, -1),
                    new Vector3( 1, -1, -1)
                )
            );

            triangles.Add
            (
                new Triangle
                (
                    new Vector3(0,  0,  0),
                    new Vector3(1, -1, -1),
                    new Vector3(1, -1,  1)
                )
            );

            triangles.Add
            (
                new Triangle
                (
                    new Vector3( 0,  0, 0),
                    new Vector3(-1, -1, 1),
                    new Vector3( 1, -1, 1)
                )
            );

            triangles.Add
            (
                new Triangle
                (
                    new Vector3(-1, -1,  1),
                    new Vector3( 1, -1, -1),
                    new Vector3( 1, -1,  1)
                )
            );

            triangles.Add
            (
                new Triangle
                (
                    new Vector3( 1, -1, -1),
                    new Vector3(-1, -1, -1),
                    new Vector3(-1, -1,  1)
                )
            );

            RenderObject obj = new RenderObject(triangles,Color.Aquamarine);
            Render.instantiateObject(obj);
            /**********************END TEST***********************/
        }
    }
}
