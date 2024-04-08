namespace OMSOFT.Bengkel.Views.Master
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

    public partial class KontakMasterEditDialog : DevExpress.XtraEditors.XtraForm
    {
        private KontakMasterData Data_;

        public KontakMasterEditDialog()
        {
            InitializeComponent();
        }

        public KontakMasterData Data
        {
            get => this.Data_;
            set => this.Data_ = value;
        }
    }
}