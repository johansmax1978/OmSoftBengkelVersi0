namespace OMSOFT.Bengkel.Custom
{
    using DevExpress.XtraBars.Navigation;
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    public partial class HeaderControl
    {
        public HeaderControl()
        {
            this.InitializeComponent();
            this.InitializeRuntime();
        }

        public HeaderControl(IContainer container) : this() => container?.Add(this);

        private void InitializeRuntime()
        {
            this.Dock = DockStyle.Top;
            this.OnLookAndFeelChanged(null, null);
            DevExpress.LookAndFeel.UserLookAndFeel.Default.StyleChanged += this.OnLookAndFeelChanged;
        }

        private void OnLookAndFeelChanged(Object sender, EventArgs e)
        {
            this.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Primary;
            this.tileNavPane1.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Primary;
            this.tileNavPane2.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Primary;
            if (this.IsHandleCreated)
            {
                this.Refresh();
            }
        }

        [Category("Elements")]
        [DefaultValue("Back")]
        public String ButtonBackText
        {
            get => this.ButtonBack.Caption;
            set => this.ButtonBack.Caption = value ?? "Back";
        }

        [Category("Elements")]
        [Description("Provides access to settings that allow you to set up raster and vector glyphs for this NavElement rendered as a button in the TileNavPane’s nav bar.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public NavElementImageOptions ButtonBackImage => this.ButtonBack.ImageOptions;

        [Category("Elements")]
        [DefaultValue(true)]
        public Boolean ButtonBackVisible
        {
            get => this.ButtonBack.Visible;
            set => this.ButtonBack.Visible = value;
        }

        [Category("Elements")]
        [Description("Occurs when the ButtonBack is clicked.")]
        public event NavElementClickEventHandler ButtonBackClick { add => this.ButtonBack.ElementClick += value; remove => this.ButtonBack.ElementClick -= value; }

        [Category("Elements")]
        [DefaultValue("Edit")]
        public String ButtonEditText
        {
            get => this.ButtonEdit.Caption;
            set => this.ButtonEdit.Caption = value ?? "Edit";
        }

        [Category("Elements")]
        [Description("Provides access to settings that allow you to set up raster and vector glyphs for this NavElement rendered as a button in the TileNavPane’s nav bar.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public NavElementImageOptions ButtonEditImage => this.ButtonEdit.ImageOptions;

        [Category("Elements")]
        [DefaultValue(false)]
        public Boolean ButtonEditVisible
        {
            get => this.ButtonEdit.Visible;
            set => this.ButtonEdit.Visible = value;
        }

        [Category("Elements")]
        [Description("Occurs when the ButtonEdit is clicked.")]
        public event NavElementClickEventHandler ButtonEditClick { add => this.ButtonEdit.ElementClick += value; remove => this.ButtonEdit.ElementClick -= value; }

        [Category("Elements")]
        [DefaultValue("Refresh")]
        public String ButtonRefreshText
        {
            get => this.ButtonRefresh.Caption;
            set => this.ButtonRefresh.Caption = value ?? "Refresh";
        }

        [Category("Elements")]
        [Description("Provides access to settings that allow you to set up raster and vector glyphs for this NavElement rendered as a button in the TileNavPane’s nav bar.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public NavElementImageOptions ButtonRefreshImage => this.ButtonRefresh.ImageOptions;

        [Category("Elements")]
        [DefaultValue(false)]
        public Boolean ButtonRefreshVisible
        {
            get => this.ButtonRefresh.Visible;
            set => this.ButtonRefresh.Visible = value;
        }

        [Category("Elements")]
        [Description("Occurs when the ButtonRefresh is clicked.")]
        public event NavElementClickEventHandler ButtonRefreshClick { add => this.ButtonRefresh.ElementClick += value; remove => this.ButtonRefresh.ElementClick -= value; }

        [Category("Elements")]
        [Description("Gets or sets the caption of this header.")]
        [DefaultValue("JUDUL HALAMAN")]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        public override String Text
        {
            get => this.iMainCaption.Text;
            set => this.iMainCaption.Text = value ?? "";
        }
    }


}
