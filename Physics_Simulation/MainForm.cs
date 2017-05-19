using System;
using System.Windows.Forms;

namespace Physics_Simulation
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Render.init(ref renderWindow);

            Render.userConfiguration.FPS = 60;
        }
    }
}
