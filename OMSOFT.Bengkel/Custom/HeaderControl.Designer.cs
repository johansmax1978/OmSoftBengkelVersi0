namespace OMSOFT.Bengkel.Custom
{
    partial class HeaderControl : DevExpress.XtraEditors.XtraUserControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HeaderControl));
            this.fMainLayout = new DevExpress.XtraLayout.LayoutControl();
            this.tileNavPane2 = new DevExpress.XtraBars.Navigation.TileNavPane();
            this.ButtonRefresh = new DevExpress.XtraBars.Navigation.NavButton();
            this.ButtonEdit = new DevExpress.XtraBars.Navigation.NavButton();
            this.tileNavPane1 = new DevExpress.XtraBars.Navigation.TileNavPane();
            this.ButtonBack = new DevExpress.XtraBars.Navigation.NavButton();
            this.fMainLayoutRoot = new DevExpress.XtraLayout.LayoutControlGroup();
            this.iMainCaption = new DevExpress.XtraLayout.SimpleLabelItem();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.fMainLayout)).BeginInit();
            this.fMainLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tileNavPane2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tileNavPane1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fMainLayoutRoot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iMainCaption)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            this.SuspendLayout();
            // 
            // fMainLayout
            // 
            this.fMainLayout.AllowCustomization = false;
            this.fMainLayout.Controls.Add(this.tileNavPane2);
            this.fMainLayout.Controls.Add(this.tileNavPane1);
            this.fMainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fMainLayout.Location = new System.Drawing.Point(0, 0);
            this.fMainLayout.Name = "fMainLayout";
            this.fMainLayout.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(733, 0, 650, 400);
            this.fMainLayout.OptionsSerialization.StoreAppearance = DevExpress.Utils.DefaultBoolean.True;
            this.fMainLayout.OptionsSerialization.StoreEnabledState = DevExpress.Utils.DefaultBoolean.True;
            this.fMainLayout.OptionsSerialization.StorePrintOptions = DevExpress.Utils.DefaultBoolean.True;
            this.fMainLayout.OptionsSerialization.StoreSpaceOptions = DevExpress.Utils.DefaultBoolean.True;
            this.fMainLayout.OptionsSerialization.StoreText = DevExpress.Utils.DefaultBoolean.True;
            this.fMainLayout.Root = this.fMainLayoutRoot;
            this.fMainLayout.Size = new System.Drawing.Size(612, 40);
            this.fMainLayout.TabIndex = 0;
            this.fMainLayout.Text = "layoutControl1";
            // 
            // tileNavPane2
            // 
            this.tileNavPane2.Buttons.Add(this.ButtonRefresh);
            this.tileNavPane2.Buttons.Add(this.ButtonEdit);
            // 
            // tileNavCategory2
            // 
            this.tileNavPane2.DefaultCategory.Name = "tileNavCategory2";
            // 
            // 
            // 
            this.tileNavPane2.DefaultCategory.Tile.DropDownOptions.BeakColor = System.Drawing.Color.Empty;
            this.tileNavPane2.Location = new System.Drawing.Point(413, 0);
            this.tileNavPane2.Name = "tileNavPane2";
            this.tileNavPane2.Size = new System.Drawing.Size(199, 40);
            this.tileNavPane2.TabIndex = 5;
            this.tileNavPane2.Text = "tileNavPane2";
            // 
            // ButtonRefresh
            // 
            this.ButtonRefresh.Alignment = DevExpress.XtraBars.Navigation.NavButtonAlignment.Right;
            this.ButtonRefresh.Caption = "Refresh";
            this.ButtonRefresh.ImageOptions.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.True;
            this.ButtonRefresh.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("ButtonRefresh.ImageOptions.SvgImage")));
            this.ButtonRefresh.ImageOptions.SvgImageSize = new System.Drawing.Size(22, 22);
            this.ButtonRefresh.Name = "ButtonRefresh";
            // 
            // ButtonEdit
            // 
            this.ButtonEdit.Alignment = DevExpress.XtraBars.Navigation.NavButtonAlignment.Right;
            this.ButtonEdit.Appearance.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.ButtonEdit.Appearance.Options.UseFont = true;
            this.ButtonEdit.AppearanceHovered.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.ButtonEdit.AppearanceHovered.Options.UseFont = true;
            this.ButtonEdit.AppearanceSelected.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.ButtonEdit.AppearanceSelected.Options.UseFont = true;
            this.ButtonEdit.Caption = "Edit";
            this.ButtonEdit.ImageOptions.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.True;
            this.ButtonEdit.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("ButtonEdit.ImageOptions.SvgImage")));
            this.ButtonEdit.ImageOptions.SvgImageSize = new System.Drawing.Size(22, 22);
            this.ButtonEdit.Name = "ButtonEdit";
            this.ButtonEdit.Visible = false;
            // 
            // tileNavPane1
            // 
            this.tileNavPane1.Buttons.Add(this.ButtonBack);
            // 
            // tileNavCategory1
            // 
            this.tileNavPane1.DefaultCategory.Name = "tileNavCategory1";
            // 
            // 
            // 
            this.tileNavPane1.DefaultCategory.Tile.DropDownOptions.BeakColor = System.Drawing.Color.Empty;
            this.tileNavPane1.Location = new System.Drawing.Point(0, 0);
            this.tileNavPane1.Name = "tileNavPane1";
            this.tileNavPane1.Size = new System.Drawing.Size(100, 40);
            this.tileNavPane1.TabIndex = 4;
            this.tileNavPane1.Text = "tileNavPane1";
            // 
            // ButtonBack
            // 
            this.ButtonBack.Alignment = DevExpress.XtraBars.Navigation.NavButtonAlignment.Right;
            this.ButtonBack.Appearance.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.ButtonBack.Appearance.Options.UseFont = true;
            this.ButtonBack.AppearanceHovered.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.ButtonBack.AppearanceHovered.Options.UseFont = true;
            this.ButtonBack.AppearanceSelected.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.ButtonBack.AppearanceSelected.Options.UseFont = true;
            this.ButtonBack.Caption = "Kembali";
            this.ButtonBack.ImageOptions.AllowGlyphSkinning = DevExpress.Utils.DefaultBoolean.True;
            this.ButtonBack.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("ButtonBack.ImageOptions.SvgImage")));
            this.ButtonBack.ImageOptions.SvgImageSize = new System.Drawing.Size(22, 22);
            this.ButtonBack.Name = "ButtonBack";
            // 
            // fMainLayoutRoot
            // 
            this.fMainLayoutRoot.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.fMainLayoutRoot.GroupBordersVisible = false;
            this.fMainLayoutRoot.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.iMainCaption,
            this.layoutControlItem1,
            this.layoutControlItem2});
            this.fMainLayoutRoot.Name = "Root";
            this.fMainLayoutRoot.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.fMainLayoutRoot.Size = new System.Drawing.Size(612, 40);
            this.fMainLayoutRoot.TextVisible = false;
            // 
            // iMainCaption
            // 
            this.iMainCaption.AllowHotTrack = false;
            this.iMainCaption.AllowHtmlStringInCaption = true;
            this.iMainCaption.AppearanceItemCaption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154)))));
            this.iMainCaption.AppearanceItemCaption.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.iMainCaption.AppearanceItemCaption.ForeColor = System.Drawing.Color.White;
            this.iMainCaption.AppearanceItemCaption.Options.UseBackColor = true;
            this.iMainCaption.AppearanceItemCaption.Options.UseFont = true;
            this.iMainCaption.AppearanceItemCaption.Options.UseForeColor = true;
            this.iMainCaption.AppearanceItemCaption.Options.UseTextOptions = true;
            this.iMainCaption.AppearanceItemCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.iMainCaption.Location = new System.Drawing.Point(100, 0);
            this.iMainCaption.Name = "iMainCaption";
            this.iMainCaption.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.iMainCaption.Size = new System.Drawing.Size(313, 40);
            this.iMainCaption.Text = "JUDUL HALAMAN";
            this.iMainCaption.TextSize = new System.Drawing.Size(128, 19);
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.tileNavPane1;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.MaxSize = new System.Drawing.Size(100, 0);
            this.layoutControlItem1.MinSize = new System.Drawing.Size(100, 20);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.layoutControlItem1.Size = new System.Drawing.Size(100, 40);
            this.layoutControlItem1.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.tileNavPane2;
            this.layoutControlItem2.Location = new System.Drawing.Point(413, 0);
            this.layoutControlItem2.MaxSize = new System.Drawing.Size(199, 0);
            this.layoutControlItem2.MinSize = new System.Drawing.Size(199, 20);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.layoutControlItem2.Size = new System.Drawing.Size(199, 40);
            this.layoutControlItem2.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // HeaderControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fMainLayout);
            this.Name = "HeaderControl";
            this.Size = new System.Drawing.Size(612, 40);
            ((System.ComponentModel.ISupportInitialize)(this.fMainLayout)).EndInit();
            this.fMainLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tileNavPane2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tileNavPane1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fMainLayoutRoot)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iMainCaption)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl fMainLayout;
        private DevExpress.XtraLayout.LayoutControlGroup fMainLayoutRoot;
        private DevExpress.XtraLayout.SimpleLabelItem iMainCaption;
        private DevExpress.XtraBars.Navigation.TileNavPane tileNavPane1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraBars.Navigation.NavButton ButtonBack;
        private DevExpress.XtraBars.Navigation.TileNavPane tileNavPane2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraBars.Navigation.NavButton ButtonEdit;
        private DevExpress.XtraBars.Navigation.NavButton ButtonRefresh;
    }
}
