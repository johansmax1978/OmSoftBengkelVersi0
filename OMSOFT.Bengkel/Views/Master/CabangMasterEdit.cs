namespace OMSOFT.Bengkel.Views.Master
{
    using DevExpress.XtraEditors;
    using DevExpress.XtraSplashScreen;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class CabangMasterEdit : DevExpress.XtraEditors.XtraUserControl
    {
        public CabangMasterEdit()
        {
            InitializeComponent();
            this.InitializeRuntime();
            
        }

        private void InitializeRuntime()
        {
            this.fMainTabPages.SelectedPageIndex = 0;
            this.ButtonContactAdd.ItemClick += (sender, e) => this.DoContactAdd();

        }

        public IOverlaySplashScreenHandle ShowOverlayScreen(out OverlayTextPainter painter)
        {
            painter = new OverlayTextPainter()
            {
                Text = "Pemproses Permintaan Anda",
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


        public void DoContactAdd()
        {
            var handle = this.ShowOverlayScreen(out _);
            try
            {
                using (var dialog = new KontakMasterEditDialog())
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

        public void DoContactEdit(KontakMasterData data)
        {
            var handle = this.ShowOverlayScreen(out _);
            try
            {
                using (var dialog = new KontakMasterEditDialog())
                {
                    dialog.Data = data;
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

        public void DoContactDelete()
        {

        }

    }
}
