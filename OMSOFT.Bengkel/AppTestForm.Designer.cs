namespace OMSOFT.Bengkel
{
    partial class AppTestForm
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
            this.components = new System.ComponentModel.Container();
            this.alamatControl1 = new OMSOFT.Bengkel.Custom.AlamatControl(this.components);
            this.SuspendLayout();
            // 
            // alamatControl1
            // 
            this.alamatControl1.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.alamatControl1.Appearance.Options.UseBackColor = true;
            this.alamatControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.alamatControl1.Location = new System.Drawing.Point(0, 0);
            this.alamatControl1.Name = "alamatControl1";
            this.alamatControl1.Size = new System.Drawing.Size(387, 219);
            this.alamatControl1.TabIndex = 0;
            // 
            // AppTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(387, 219);
            this.Controls.Add(this.alamatControl1);
            this.DoubleBuffered = true;
            this.FormBorderEffect = DevExpress.XtraEditors.FormBorderEffect.Shadow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AppTestForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Omsoft Bengkel - Control Testing Form";
            this.ResumeLayout(false);

        }

        #endregion

        private Custom.AlamatControl alamatControl1;
    }
}