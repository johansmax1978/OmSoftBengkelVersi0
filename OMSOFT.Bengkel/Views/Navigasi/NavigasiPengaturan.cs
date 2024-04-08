namespace OMSOFT.Bengkel.Views.Navigasi
{
    using DevExpress.XtraEditors;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class NavigasiPengaturan : XtraUserControl, INavigator
    {
        public NavigasiPengaturan()
        {
            this.InitializeComponent();
            this.InitializeRuntime();
        }

        public NavigasiPengaturan(IContainer container) : this() => container?.Add(this);

        private void InitializeRuntime()
        {
            this.galleryControl1.Gallery.ItemClick += this.OnNavigationItemClick;
            this.iCaption.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Primary;
        }

        private void OnNavigationItemClick(Object sender, DevExpress.XtraBars.Ribbon.GalleryItemClickEventArgs e)
        {
            if(e.Item.Value is String target && target.Length != 0)
            {
                switch (target)
                {
                    case "PERUSAHAAN":
                    case "CABANG":
                        this.OnNavigate(target, null);
                        break;
                }
            }
        }

        protected virtual void OnNavigate(String destination, Object parameters)
        {
            if (this.Events[nameof(this.Navigate)] is EventHandler<NavigasiEvent> handler)
            {
                handler.Invoke(this, new NavigasiEvent(destination, parameters));
            }
        }

        public event EventHandler<NavigasiEvent> Navigate
        {
            add => this.Events.AddHandler(nameof(Navigate), value);
            remove => this.Events.RemoveHandler(nameof(Navigate), value);
        }

        private String GetCaption()
        {
            if (this.InvokeRequired)
                return (String)this.Invoke((Func<String>)this.GetCaption);
            else
                return this.iCaption.Text;
        }

        private void SetCaption(String value)
        {
            if (this.InvokeRequired)
                _ = this.Invoke((Action<String>)this.SetCaption, value);
            else
                this.iCaption.Text = value ?? "Pengaturan";
        }

        public String Caption
        {
            get => this.GetCaption();
            set => this.SetCaption(value);
        }

        public Control Control => this;
    }
}
