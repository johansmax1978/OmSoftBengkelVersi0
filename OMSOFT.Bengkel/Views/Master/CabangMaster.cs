namespace OMSOFT.Bengkel.Views.Master
{
    using DevExpress.Spreadsheet;
    using DevExpress.XtraEditors;
    using DevExpress.XtraSplashScreen;
    using FluentDesignForm.Properties;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class CabangMaster : DevExpress.XtraEditors.XtraUserControl
    {
        public CabangMaster()
        {
            this.InitializeComponent();
            this.InitializeRuntime();
        }

        private void InitializeRuntime()
        {
            this.memoExEdit1.Text = "Halo Dunia\r\nHello World!!";
            this.ButtonTambah.ItemClick += (sender, e) => this.DoCabangAdd();
        }

        public void LoadDemo()
        {
            var lokasi = new String[]
            {
                "Bandung",
                "Jakarta",
                "Semarang",
                "Yogyakarta",
                "Bali",
                "Surabaya",
                "Malang",
                "Tangerang",
                "Palembang",
                "Medan"
            };
            var alamat = new String[lokasi.Length];
            for (var i = 0; i < lokasi.Length; i++)
            {
                alamat[i] = $"Jl. Soekarno Hatta No. {RandomAPI.PushRange(25, 350)}, Kota {lokasi[i]}";
            }
            var items = this.cabangMasterDataBindingSource;
            if (items.Count == 0)
            {
                for (var i = 0; i < 100; i++)
                {
                    var kotaID = RandomAPI.PushRange(0, lokasi.Length - 1);
                    items.Add(new CabangMasterData
                    {
                        No = i + 1,
                        Nama = $"Cabang {i + 1}",
                        Kota = lokasi[kotaID],
                        Alamat = alamat[kotaID],
                        NoHP = $"08{RandomAPI.PushKeys(1, 10, MixedChars.Number)[0]}",
                        NoWA = $"08{RandomAPI.PushKeys(1, 10, MixedChars.Number)[0]}",
                        Kode = RandomAPI.PushSerial(9, 3)
                    });
                }
            }
            this.gridView1.ShowRibbonPrintPreview();
        }

        private void barButtonItem5_ItemClick(System.Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.LoadDemo();
        }

        public async Task ImportFromExcel()
        {
            using (var dialog = new OpenFileDialog { Filter = "Excel Document Files (*.xls;*.xlsx)|*.xls;*.xlsx", RestoreDirectory = true, Title = "Silahkan Pilih File Excel" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var waiting = SplashScreenManager.ShowOverlayForm(this, new OverlayWindowOptions
                    {
                        FadeIn = false,
                        FadeOut = false,
                        AnimationType = WaitAnimationType.Line,
                        BackColor = Color.FromArgb(35, 35, 35),
                        DisableInput = true,
                        ForeColor = Color.DodgerBlue,
                        UseDirectX = true,
                        Opacity = 0.9
                    });
                    try
                    {
                        var location = dialog.FileName;
                        var records = await Task.Factory.StartNew(LoadFromExcel, location, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                        if (records is not null && records.Length != 0)
                        {
                            var items = this.cabangMasterDataBindingSource;
                            items.Clear();
                            for (var i = 0; i < records.Length; items.Add(records[i++])) ;
                        }
                    }
                    catch (Exception error)
                    {
                        SplashScreenManager.CloseOverlayForm(waiting);
                        waiting = null;
                        XtraMessageBox.Show(error.ToString());
                    }
                    finally
                    {
                        if (waiting is not null)
                            SplashScreenManager.CloseOverlayForm(waiting);
                    }
                }
            }
        }

        private static CabangMasterData[] LoadFromExcel(Object state)
        {
            const Char FirstColumn = 'B';
            const Int32 StartRow = 3;

            var path = (String)state;
            using var workbook = new Workbook();
            _ = workbook.LoadDocument(path, DocumentFormat.Xlsx);
            var worksheet = workbook.Worksheets[0];
            var position = StartRow;
            var records = new List<CabangMasterData>();
            while (true)
            {
                var no = worksheet[$"{FirstColumn}{position}"].Value;
                var kode = worksheet[$"{(Char)(FirstColumn + 1)}{position}"].Value?.TextValue;
                var nama = worksheet[$"{(Char)(FirstColumn + 2)}{position}"].Value?.TextValue;
                var nohp = worksheet[$"{(Char)(FirstColumn + 3)}{position}"].Value?.TextValue;
                var kota = worksheet[$"{(Char)(FirstColumn + 4)}{position}"].Value?.TextValue;
                var alamat = worksheet[$"{(Char)(FirstColumn + 5)}{position}"].Value?.TextValue;
                if (kode is null || nama is null || kode.Length == 0 || nama.Length == 0) break;
                var record = new CabangMasterData
                {
                    No = (Int32)no.NumericValue,
                    Alamat = alamat,
                    Nama = nama,
                    Kode = kode,
                    NoHP = nohp,
                    NoWA = nohp,
                    Kota = kota
                };
                records.Add(record);
                position++;
            }
            return records.ToArray();

        }

        private async void OnImportExcelClick(Object sender, DevExpress.XtraBars.ItemClickEventArgs e)
            => await this.ImportFromExcel();

        public IOverlaySplashScreenHandle ShowOverlayScreen(out OverlayTextPainter painter)
        {
            painter = new OverlayTextPainter()
            {
                Color = DevExpress.LookAndFeel.DXSkinColors.ForeColors.ControlText,
                Font = this.Font
            };
            var options = new OverlayWindowOptions
            {
                CustomPainter = painter,
                AnimationType = WaitAnimationType.Line,
                LineAnimationParameters = new LineAnimationParams(10, 10, 10),
                BackColor = this.BackColor,
                DisableInput = true,
                FadeIn = true,
                FadeOut = true,
                UseDirectX = true,
                Opacity = 0.85,
                ForeColor = DevExpress.LookAndFeel.DXSkinColors.ForeColors.ControlText
            };
            try { return SplashScreenManager.ShowOverlayForm(this, options); }
            catch { return null; }
        }

        public void DoCabangAdd()
        {
            var handle = this.ShowOverlayScreen(out var painter);
            try
            {
                using (var dialog = new CabangMasterEditDialog())
                {
                    dialog.StartPosition = FormStartPosition.CenterScreen;
                    dialog.ShowDialog(this);
                }
            }
            finally
            {
                if (handle is not null)
                {
                    SplashScreenManager.CloseOverlayForm(handle);
                }
            }
        }
    }


}
