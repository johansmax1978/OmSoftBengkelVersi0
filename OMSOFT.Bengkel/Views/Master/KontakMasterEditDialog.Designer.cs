namespace OMSOFT.Bengkel.Views.Master
{
    partial class KontakMasterEditDialog
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
            this.kontak1 = new OMSOFT.Bengkel.Views.Master.KontakMasterEdit();
            this.SuspendLayout();
            // 
            // kontak1
            // 
            this.kontak1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kontak1.Location = new System.Drawing.Point(0, 0);
            this.kontak1.Name = "kontak1";
            this.kontak1.Size = new System.Drawing.Size(630, 515);
            this.kontak1.TabIndex = 0;
            // 
            // KontakMasterEditDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 515);
            this.Controls.Add(this.kontak1);
            this.MaximizeBox = false;
            this.Name = "KontakMasterEditDialog";
            this.Text = "KontakDialog";
            this.ResumeLayout(false);

        }

        #endregion

        private KontakMasterEdit kontak1;
    }
}