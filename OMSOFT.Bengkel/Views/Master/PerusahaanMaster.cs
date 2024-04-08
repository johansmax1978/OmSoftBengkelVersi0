namespace OMSOFT.Bengkel.Views.Master
{
    using DevExpress.XtraEditors;
    using DevExpress.XtraSplashScreen;
    using OMSOFT.Bengkel.Controllers;
    using OMSOFT.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class PerusahaanMaster : DevExpress.XtraEditors.XtraUserControl
    {
        private Boolean m_SuppressEvents;
        private Perusahaan m_Data;


        public PerusahaanMaster()
        {
            this.InitializeComponent();
            this.InitializeRuntime();
        }

        private void InitializeRuntime()
        {
            this.tabPane1.SelectedPageIndex = 0;
            this.iAlamat.BackColor = this.BackColor;
        }

        private async void OnButtonRefreshClick(Object sender, DevExpress.XtraBars.Navigation.NavElementEventArgs e)
        {
            if (!this.m_SuppressEvents)
            {
                this.m_SuppressEvents = true;
                try
                {
                    await this.ReloadContent();
                }
                finally
                {
                    this.m_SuppressEvents = false;
                }
            }
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (AppLauncher.IsRunning)
            {
                await this.ReloadContent();
            }
        }

        public async Task ReloadContent()
        {
            var painter = new OverlayTextPainter("Memuat Informasi")
            {
                Font = this.Font,
                Color = Color.White
            };
            var waiting = SplashScreenManager.ShowOverlayForm((Control)this.FindForm() ?? this, new OverlayWindowOptions
            {
                AnimationType = WaitAnimationType.Line,
                CustomPainter = painter,
                BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Primary,
                DisableInput = true,
                FadeIn = true,
                FadeOut = true,
                Opacity = 0.9,
                UseDirectX = true,
                ForeColor = Color.White
            });
            try
            {
                var controller = new PerusahaanController();
                var perusahaan = await controller.Get();
                var alamat = perusahaan.PerusahaanAlamat;
                this.iNama.Text = perusahaan.Nama;
                if (alamat is not null)
                {
                    this.iAlamat.Text = alamat.Alamat ?? "";
                    this.iProvinsi.Text = alamat.Provinsi ?? "";
                    this.iKota.Text = alamat.Kota ?? "";
                    this.iKecamatan.Text = alamat.Kecamatan ?? "";
                    this.iDesa.Text = alamat.Desa ?? "";
                    this.iTelp.Text = alamat.NoTelp1 ?? alamat.NoTelp2 ?? "";
                    this.iFaks.Text = alamat.NoFaks1 ?? alamat.NoFaks2 ?? "";
                    this.iHP.Text = alamat.NoHp1 ?? alamat.NoHp2 ?? "";
                    this.iWA.Text = alamat.NoWa1 ?? alamat.NoWa2 ?? "";
                    this.iEmail.Text = alamat.Email1 ?? alamat.Email2 ?? "";
                    this.iWebsite.Text = alamat.Website;
                }
                var pajak = perusahaan.PerusahaanPajak;
                if (pajak is not null)
                {
                    this.iNamaPajak.Text = pajak.Nama ?? "";
                    if (pajak.TglPKP.HasValue)
                        this.iTglPKP.DateTime = pajak.TglPKP.Value;
                    else
                        this.iTglPKP.EditValue = null;
                    this.iNoPKP.Text = pajak.NoPKP ?? "";
                    this.iTipeUsaha.Text = pajak.TipeUsaha;
                    this.iNPWP.Text = pajak.NPWP ?? "";
                    this.iKLU.Text = pajak.KLU ?? "";
                }
                this.m_Data = perusahaan;
            }
            catch(Exception error)
            {
                SplashScreenManager.CloseOverlayForm(waiting);
                waiting = null;
                _ = XtraMessageBox.Show(this, error.ToString(), "Loading Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if(waiting is not null)
                {
                    SplashScreenManager.CloseOverlayForm(waiting);
                }
            }
        }

        private async void OnButtonEditClick(Object sender, DevExpress.XtraBars.Navigation.NavElementEventArgs e)
        {
            if (!this.m_SuppressEvents)
            {
                this.m_SuppressEvents = true;
                var painter = new OverlayTextPainter("Mengubah Informasi Perusahaan")
                {
                    Font = this.Font,
                    Color = Color.White
                };
                var waiting = SplashScreenManager.ShowOverlayForm((Control)this.FindForm() ?? this, new OverlayWindowOptions
                {
                    AnimationType = WaitAnimationType.Line,
                    CustomPainter = painter,
                    BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Primary,
                    DisableInput = true,
                    FadeIn = true,
                    FadeOut = true,
                    Opacity = 0.9,
                    UseDirectX = true,
                    ForeColor = Color.White
                });
                try
                {
                    using(var dialog = new PerusahaanMasterEditDialog())
                    {
                        dialog.Data = this.m_Data;
                        _ = dialog.ShowDialog(this);
                        await this.ReloadContent();
                    }
                }
                finally
                {
                    this.m_SuppressEvents = false;
                    if(waiting is not null)
                    {
                        SplashScreenManager.CloseOverlayForm(waiting);
                    }
                }
            }
        }
    }
}
