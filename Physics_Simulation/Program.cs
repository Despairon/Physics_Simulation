using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Physics_Simulation
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm = new MainForm();
            Application.Run(mainForm);
        }
        private static MainForm mainForm;

        public static InputManager getInputManagerFromMainForm()
        {
            return mainForm.inputManager;
        }

        public static InputManager getInputManagerFromCustomForm(MainForm form)
        {
            return form.inputManager;
        }
    }
}
