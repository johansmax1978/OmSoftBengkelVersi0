namespace OMSOFT.Bengkel.Views.Master
{
    partial class CabangMasterEditDialog
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
            this.cabang1 = new OMSOFT.Bengkel.Views.Master.CabangMasterEdit();
            this.SuspendLayout();
            // 
            // cabang1
            // 
            this.cabang1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cabang1.Location = new System.Drawing.Point(0, 0);
            this.cabang1.Name = "cabang1";
            this.cabang1.Size = new System.Drawing.Size(673, 503);
            this.cabang1.TabIndex = 0;
            // 
            // CabangDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(673, 503);
            this.Controls.Add(this.cabang1);
            this.MaximizeBox = false;
            this.Name = "CabangDialog";
            this.Text = "CabangDialog";
            this.ResumeLayout(false);

        }

        #endregion

        private CabangMasterEdit cabang1;
    }
}