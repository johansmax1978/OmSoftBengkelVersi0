namespace OMSOFT.Bengkel
{
    using DevExpress.XtraBars;
    using DevExpress.XtraEditors;
    using OMSOFT.Bengkel.Views;
    using OMSOFT.Bengkel.Views.Master;
    using OMSOFT.Bengkel.Views.Navigasi;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;

    public partial class AppBrowser
    {
        private INavigator CurrentNavigator;
        private Control CurrentNavigatorView;


        /// <summary>
        /// Initializes a new instance of the <see cref="AppBrowser"/> form class with default settings.
        /// </summary>
        public AppBrowser()
        {
            this.InitializeComponent();
            this.InitializeRuntime();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppBrowser"/> form class with default settings and add the given <paramref name="container"/> with this component.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> object that should be added with the current new <see cref="AppBrowser"/> as their component. Can be <see langword="null"/> to ignore.</param>
        public AppBrowser(IContainer container) : this() => container?.Add(this);

        /// <summary>
        /// Setup the current <see cref="AppBrowser"/> form instance before the form is displayed, this method is called by constructor during initialization phase.
        /// </summary>
        private void InitializeRuntime()
        {
            this.WindowState = FormWindowState.Maximized;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.fMenuGroupAccount.Text = $"User ({Environment.UserName})";
        }

        private void OnAboutClick(Object sender, ItemClickEventArgs e)
        {
            XtraMessageBox.Show("OMSOFT Bengkel Version 6.0.0.1");
        }

        private void PerformNavigationMenu(String menu)
        {
            var current = this.CurrentNavigator;
            var frame = this.iNavigationFramePage1;
            if (menu == "PENGATURAN")
            {
                if (current is not null && current is not NavigasiPengaturan)
                {
                    frame.Controls.Clear();
                    current.Control.Dispose();
                    current.Navigate -= this.OnNavigasiNavigate;
                    current = null;
                }
                if (current is null)
                {
                    current = new NavigasiPengaturan() { Dock = DockStyle.Fill };
                    current.Navigate += this.OnNavigasiNavigate;
                }
                this.CurrentNavigator = current;
                frame.Controls.Add(current.Control);                
            }
            this.iMainNavigation.SelectedPageIndex = 0;
        }

        private void OnNavigasiNavigate(Object sender, NavigasiEvent e)
        {
            var current = this.CurrentNavigatorView;
            var frame = this.iNavigationFramePage2;
            switch (e.Destination)
            {
                case "PERUSAHAAN":
                    if (current is not null && current is not PerusahaanMaster)
                    {
                        frame.Controls.Clear();
                        current.Dispose();
                        current = null;
                    }
                    if (current is null)
                    {
                        current = new PerusahaanMaster { Dock = DockStyle.Fill };                        
                    }
                    this.CurrentNavigatorView = current;
                    frame.Controls.Add(current);
                    this.iMainNavigation.SelectedPageIndex = 1;
                    break;
                case "CABANG":
                    if (current is not null && current is not CabangMaster)
                    {
                        frame.Controls.Clear();
                        current.Dispose();
                        current = null;
                    }
                    if (current is null)
                    {
                        current = new CabangMaster { Dock = DockStyle.Fill };                        
                    }
                    this.CurrentNavigatorView = current;
                    frame.Controls.Add(current);
                    this.iMainNavigation.SelectedPageIndex = 1;
                    break;


            }
        }

        private void OnSidebarNavigationClick(Object sender, DevExpress.XtraBars.Navigation.ElementClickEventArgs e)
            => this.PerformNavigationMenu(e.Element.Text.ToUpperInvariant());
    }
}
