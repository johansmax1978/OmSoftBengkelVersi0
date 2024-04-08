namespace OMSOFT.Bengkel.Views.Master
{
    using DevExpress.ClipboardSource.SpreadsheetML;
    using DevExpress.XtraEditors;
    using DevExpress.XtraLayout;
    using DevExpress.XtraSplashScreen;
    using OMSOFT.Bengkel.Controllers;
    using OMSOFT.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class PerusahaanMasterEdit : DevExpress.XtraEditors.XtraUserControl
    {
        private static void ClearLayout(LayoutControl layout)
        {
            var items = layout.Root.Items;
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i] is LayoutControlItem lci)
                {
                    var control = lci.Control;
                    if (control is BaseEdit baseedit)
                    {
                        baseedit.EditValue = null;
                    }
                    else if (control is MemoEdit memo)
                    {
                        memo.Text = "";
                    }
                    else if (control is ToggleSwitch toggle)
                    {
                        toggle.IsOn = false;
                    }
                }
            }
        }

        private Boolean m_SuppressEvents;
        private Perusahaan m_Data;

        public PerusahaanMasterEdit()
        {
            InitializeComponent();
            this.tabPane1.SelectedPageIndex = 0;
            this.ButtonSave.Click += this.ButtonSave_Click;
            this.ButtonSaveClose.Click += this.ButtonSaveClose_Click;
        }

        private async void ButtonSaveClose_Click(Object sender, EventArgs e)
        {
            if (!this.m_SuppressEvents)
            {
                this.m_SuppressEvents = true;
                try
                {
                    if(await this.RunUpdate())
                    {
                        this.FindForm()?.Close();
                    }
                }
                finally
                {
                    this.m_SuppressEvents = false;
                }
            }
        }

        private async void ButtonSave_Click(Object sender, EventArgs e)
        {
            if (!this.m_SuppressEvents)
            {
                this.m_SuppressEvents = true;
                try
                {
                    _ = await this.RunUpdate();
                }
                finally
                {
                    this.m_SuppressEvents = false;
                }
            }
        }

        public Perusahaan Data
        {
            get => this.m_Data;
            set
            {
                if (value != this.m_Data)
                {
                    this.m_Data = value;
                    ClearLayout(this.layoutControl1);
                    ClearLayout(this.layoutControl2);
                    if (value is not null)
                    {
                        this.iNama.Text = value.Nama;
                        if (value.PerusahaanAlamat is PerusahaanAlamat alamat)
                        {
                            this.iAlamat.Text = alamat.Alamat;
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
                        else
                        {
                            value.PerusahaanAlamat = new PerusahaanAlamat { ID = value.ID };
                        }
                        if (value.PerusahaanPajak is PerusahaanPajak pajak)
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
                        else
                        {
                            value.PerusahaanPajak = new PerusahaanPajak { ID = value.ID };
                        }
                    }
                }
            }
        }

        private void SynchronizeUpdates()
        {
            var data = this.m_Data;
            data.Nama = this.iNama.Text;
            data.LastEdited = DateTimeOffset.Now;
            var alamat = data.PerusahaanAlamat;
            if (alamat is null) data.PerusahaanAlamat = alamat = new PerusahaanAlamat { ID = data.ID };
            alamat.Alamat = this.iAlamat.Text;
            alamat.Provinsi = this.iProvinsi.Text;
            alamat.Kota = this.iKota.Text;
            alamat.Kecamatan = this.iKecamatan.Text;
            alamat.Desa = this.iDesa.Text;
            alamat.NoTelp1 = this.iTelp.Text;
            alamat.NoFaks1 = this.iFaks.Text;
            alamat.NoHp1 = this.iHP.Text;
            alamat.NoWa1 = this.iWA.Text;
            alamat.Email1 = this.iEmail.Text;
            alamat.Website = this.iWebsite.Text;
            alamat.LastEdited = DateTimeOffset.Now;
            var pajak = data.PerusahaanPajak;
            if (pajak is null) data.PerusahaanPajak = pajak = new PerusahaanPajak { ID = data.ID };
            pajak.Nama = this.iNamaPajak.Text;
            pajak.TglPKP = this.iTglPKP.EditValue is not null ? this.iTglPKP.DateTime : null;
            pajak.NoPKP = this.iNoPKP.Text;
            pajak.NPWP = this.iNPWP.Text;
            pajak.KLU = this.iKLU.Text;
            pajak.LastEdited = DateTimeOffset.Now;
        }

        public async Task<Boolean> RunUpdate()
        {
            var painter = new OverlayTextPainter("Menyimpan Informasi..")
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
            this.SynchronizeUpdates();
            var data = this.m_Data;
            Exception lastError = null;
            try
            {
                var controller = new PerusahaanController();
                data = await controller.Post(data);
            }
            catch (Exception error)
            {
                lastError = error;
            }
            finally
            {
                if (waiting is not null)
                {
                    SplashScreenManager.CloseOverlayForm(waiting);
                }
            }
            if (lastError is not null)
            {
                _ = XtraMessageBox.Show(lastError.Message, "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else
            {
                _ = XtraMessageBox.Show("Data berhasil disimpan.", "Update Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.m_Data = null;
                this.Data = data;
                return true;
            }
        }

        private void ButtonCancel_Click(Object sender, EventArgs e)
            => this.ParentForm?.Close();
    }
}
