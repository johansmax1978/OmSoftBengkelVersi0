namespace OMSOFT.Bengkel.Views.Master
{
    partial class PerusahaanMasterEditDialog
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
            this.perusahaan1 = new OMSOFT.Bengkel.Views.Master.PerusahaanMasterEdit();
            this.SuspendLayout();
            // 
            // perusahaan1
            // 
            this.perusahaan1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.perusahaan1.Location = new System.Drawing.Point(0, 0);
            this.perusahaan1.Margin = new System.Windows.Forms.Padding(4);
            this.perusahaan1.Name = "perusahaan1";
            this.perusahaan1.Size = new System.Drawing.Size(603, 476);
            this.perusahaan1.TabIndex = 0;
            // 
            // PerusahaanDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(603, 476);
            this.Controls.Add(this.perusahaan1);
            this.DoubleBuffered = true;
            this.FormBorderEffect = DevExpress.XtraEditors.FormBorderEffect.Shadow;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(605, 508);
            this.MinimumSize = new System.Drawing.Size(605, 508);
            this.Name = "PerusahaanDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OMSOFT - Perusahan Dialog";
            this.ResumeLayout(false);

        }

        #endregion

        private PerusahaanMasterEdit perusahaan1;
    }
}