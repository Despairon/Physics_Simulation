namespace Physics_Simulation
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.renderWindow = new Tao.Platform.Windows.SimpleOpenGlControl();
            this.SuspendLayout();
            // 
            // renderWindow
            // 
            this.renderWindow.AccumBits = ((byte)(0));
            this.renderWindow.AutoCheckErrors = false;
            this.renderWindow.AutoFinish = false;
            this.renderWindow.AutoMakeCurrent = true;
            this.renderWindow.AutoSwapBuffers = true;
            this.renderWindow.BackColor = System.Drawing.Color.Black;
            this.renderWindow.ColorBits = ((byte)(32));
            this.renderWindow.DepthBits = ((byte)(16));
            this.renderWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderWindow.Location = new System.Drawing.Point(0, 0);
            this.renderWindow.Margin = new System.Windows.Forms.Padding(0);
            this.renderWindow.Name = "renderWindow";
            this.renderWindow.Size = new System.Drawing.Size(784, 562);
            this.renderWindow.StencilBits = ((byte)(0));
            this.renderWindow.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.renderWindow);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private Tao.Platform.Windows.SimpleOpenGlControl renderWindow;
    }
}

