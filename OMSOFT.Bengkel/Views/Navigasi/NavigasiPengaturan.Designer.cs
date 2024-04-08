namespace OMSOFT.Bengkel.Views.Navigasi
{
    partial class NavigasiPengaturan
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DevExpress.XtraBars.Ribbon.GalleryItemGroup galleryItemGroup1 = new DevExpress.XtraBars.Ribbon.GalleryItemGroup();
            DevExpress.XtraBars.Ribbon.GalleryItem galleryItem1 = new DevExpress.XtraBars.Ribbon.GalleryItem();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NavigasiPengaturan));
            DevExpress.XtraBars.Ribbon.GalleryItem galleryItem2 = new DevExpress.XtraBars.Ribbon.GalleryItem();
            DevExpress.XtraBars.Ribbon.GalleryItem galleryItem3 = new DevExpress.XtraBars.Ribbon.GalleryItem();
            DevExpress.XtraBars.Ribbon.GalleryItemGroup galleryItemGroup2 = new DevExpress.XtraBars.Ribbon.GalleryItemGroup();
            DevExpress.XtraBars.Ribbon.GalleryItem galleryItem4 = new DevExpress.XtraBars.Ribbon.GalleryItem();
            DevExpress.XtraBars.Ribbon.GalleryItem galleryItem5 = new DevExpress.XtraBars.Ribbon.GalleryItem();
            this.galleryControl1 = new DevExpress.XtraBars.Ribbon.GalleryControl();
            this.galleryControlClient1 = new DevExpress.XtraBars.Ribbon.GalleryControlClient();
            this.iCaption = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.galleryControl1)).BeginInit();
            this.galleryControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // galleryControl1
            // 
            this.galleryControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.galleryControl1.Controls.Add(this.galleryControlClient1);
            this.galleryControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // 
            // 
            this.galleryControl1.Gallery.AllowFilter = false;
            this.galleryControl1.Gallery.AllowHtmlText = true;
            this.galleryControl1.Gallery.ContentHorzAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.galleryControl1.Gallery.DistanceBetweenItems = 25;
            galleryItemGroup1.Caption = "Perusahaan dan Pengaturan";
            galleryItem1.Caption = "Info Perusahaan";
            galleryItem1.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("resource.SvgImage")));
            galleryItem1.Value = "PERUSAHAAN";
            galleryItem2.Caption = "Cabang";
            galleryItem2.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("resource.SvgImage1")));
            galleryItem2.Value = "CABANG";
            galleryItem3.Caption = "Pengaturan Umum";
            galleryItem3.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("resource.SvgImage2")));
            galleryItemGroup1.Items.AddRange(new DevExpress.XtraBars.Ribbon.GalleryItem[] {
            galleryItem1,
            galleryItem2,
            galleryItem3});
            galleryItemGroup2.Caption = "Pengguna Program";
            galleryItem4.Caption = "Data User";
            galleryItem4.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("resource.SvgImage3")));
            galleryItem5.Caption = "Group User/\r\nHak Akses User";
            galleryItem5.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("resource.SvgImage4")));
            galleryItemGroup2.Items.AddRange(new DevExpress.XtraBars.Ribbon.GalleryItem[] {
            galleryItem4,
            galleryItem5});
            this.galleryControl1.Gallery.Groups.AddRange(new DevExpress.XtraBars.Ribbon.GalleryItemGroup[] {
            galleryItemGroup1,
            galleryItemGroup2});
            this.galleryControl1.Gallery.ImageSize = new System.Drawing.Size(64, 64);
            this.galleryControl1.Gallery.RowCount = 1;
            this.galleryControl1.Gallery.ScrollMode = DevExpress.XtraBars.Ribbon.Gallery.GalleryScrollMode.Smooth;
            this.galleryControl1.Gallery.ShowItemText = true;
            this.galleryControl1.Gallery.ShowScrollBar = DevExpress.XtraBars.Ribbon.Gallery.ShowScrollBar.Auto;
            this.galleryControl1.Location = new System.Drawing.Point(0, 40);
            this.galleryControl1.Name = "galleryControl1";
            this.galleryControl1.Padding = new System.Windows.Forms.Padding(75);
            this.galleryControl1.Size = new System.Drawing.Size(746, 525);
            this.galleryControl1.TabIndex = 7;
            this.galleryControl1.Text = "galleryControl1";
            // 
            // galleryControlClient1
            // 
            this.galleryControlClient1.GalleryControl = this.galleryControl1;
            this.galleryControlClient1.Location = new System.Drawing.Point(75, 75);
            this.galleryControlClient1.Size = new System.Drawing.Size(596, 375);
            // 
            // iCaption
            // 
            this.iCaption.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(158)))));
            this.iCaption.Appearance.Font = new System.Drawing.Font("Tahoma", 19F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.iCaption.Appearance.ForeColor = System.Drawing.Color.White;
            this.iCaption.Appearance.Options.UseBackColor = true;
            this.iCaption.Appearance.Options.UseFont = true;
            this.iCaption.Appearance.Options.UseForeColor = true;
            this.iCaption.Appearance.Options.UseTextOptions = true;
            this.iCaption.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.iCaption.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.iCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.iCaption.Location = new System.Drawing.Point(0, 0);
            this.iCaption.Name = "iCaption";
            this.iCaption.Size = new System.Drawing.Size(746, 40);
            this.iCaption.TabIndex = 8;
            this.iCaption.Text = "Pengaturan";
            // 
            // NavigasiPengaturan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.galleryControl1);
            this.Controls.Add(this.iCaption);
            this.Name = "NavigasiPengaturan";
            this.Size = new System.Drawing.Size(746, 565);
            ((System.ComponentModel.ISupportInitialize)(this.galleryControl1)).EndInit();
            this.galleryControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.GalleryControl galleryControl1;
        private DevExpress.XtraBars.Ribbon.GalleryControlClient galleryControlClient1;
        private DevExpress.XtraEditors.LabelControl iCaption;
    }
}
